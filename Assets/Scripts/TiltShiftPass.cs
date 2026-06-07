using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class TiltShiftPass : ScriptableRenderPass
{
    private static readonly int BlurSizeID     = Shader.PropertyToID("_BlurSize");
    private static readonly int FocusCenterID  = Shader.PropertyToID("_FocusCenter");
    private static readonly int FocusWidthID   = Shader.PropertyToID("_FocusWidth");
    private static readonly int FocusFalloffID = Shader.PropertyToID("_FocusFalloff");
    private static readonly int SaturationID   = Shader.PropertyToID("_Saturation");

    private Material _material;
    private Material _copyMaterial; // plain blit, pass index 0

    public TiltShiftPass(RenderPassEvent evt, Material mat)
    {
        renderPassEvent = evt;
        _material = mat;
        requiresIntermediateTexture = true;

        // Use URP's built-in blit shader for the final copy pass
        _copyMaterial = CoreUtils.CreateEngineMaterial(
            "Hidden/Universal Render Pipeline/Blit");
    }

    private class BlitPassData
    {
        public TextureHandle src;
        public Material      mat;
        public int           pass;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (_material == null) return;

        var stack     = VolumeManager.instance.stack;
        var component = stack.GetComponent<TiltShiftVolumeComponent>();
        if (component == null || !component.IsActive()) return;

        _material.SetFloat(BlurSizeID,     component.blurSize.value);
        _material.SetFloat(FocusCenterID,  component.focusCenter.value);
        _material.SetFloat(FocusWidthID,   component.focusWidth.value);
        _material.SetFloat(FocusFalloffID, component.focusFalloff.value);
        _material.SetFloat(SaturationID,   component.saturation.value);

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData   = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer) return;

        TextureHandle cameraColor = resourceData.activeColorTexture;

        var desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples     = 1;

        TextureHandle tempH = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "_TiltShiftTempH", false, FilterMode.Bilinear);
        TextureHandle tempV = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "_TiltShiftTempV", false, FilterMode.Bilinear);

        // Pass A — horizontal blur: cameraColor → tempH
        AddBlitPass(renderGraph, "TiltShift_H", cameraColor, tempH, _material, 0);

        // Pass B — vertical blur + composite: tempH → tempV
        AddBlitPass(renderGraph, "TiltShift_V", tempH, tempV, _material, 1);

        // Pass C — copy result back: tempV → cameraColor
        AddBlitPass(renderGraph, "TiltShift_Copy", tempV, cameraColor, _copyMaterial, 0);
    }

    private static void AddBlitPass(
        RenderGraph renderGraph,
        string name,
        TextureHandle src,
        TextureHandle dst,
        Material mat,
        int passIndex)
    {
        using var builder = renderGraph.AddRasterRenderPass<BlitPassData>(name, out var data);

        data.src  = src;
        data.mat  = mat;
        data.pass = passIndex;

        builder.UseTexture(src);               // read
        builder.SetRenderAttachment(dst, 0);   // write
        builder.AllowPassCulling(false);

        builder.SetRenderFunc((BlitPassData d, RasterGraphContext ctx) =>
        {
            Blitter.BlitTexture(ctx.cmd, d.src, new Vector4(1, 1, 0, 0), d.mat, d.pass);
        });
    }
}