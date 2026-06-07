using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP Renderer Feature for Tilt-Shift (Unity 6000.4 / URP 17+).
/// Add this to your URP Renderer Asset via the Inspector.
/// </summary>
public class TiltShiftRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class TiltShiftSettings
    {
        [Tooltip("Material using the TiltShift shader.")]
        public Material tiltShiftMaterial;

        [Tooltip("When in the render pipeline this pass executes.")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public TiltShiftSettings settings = new TiltShiftSettings();

    private TiltShiftPass _pass;

    public override void Create()
    {
        _pass = new TiltShiftPass(settings.renderPassEvent, settings.tiltShiftMaterial);
    }

    // URP 17 signature — no RenderingData parameter
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.tiltShiftMaterial == null)
        {
            Debug.LogWarning("[TiltShift] No material assigned to TiltShiftRendererFeature.");
            return;
        }

        // Skip preview cameras
        if (renderingData.cameraData.cameraType == CameraType.Preview) return;

        renderer.EnqueuePass(_pass);
    }
}
