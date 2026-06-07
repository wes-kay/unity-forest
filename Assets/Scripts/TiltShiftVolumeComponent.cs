using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Volume component exposing tilt-shift parameters.
/// Add a Volume to your scene and override these settings.
/// </summary>
[System.Serializable, VolumeComponentMenu("Custom/Tilt Shift")]
public class TiltShiftVolumeComponent : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Gaussian blur radius applied outside the focus band (pixels).")]
    public ClampedFloatParameter blurSize = new ClampedFloatParameter(4f, 0f, 20f);

    [Tooltip("Vertical centre of the sharp focus band (0 = bottom, 1 = top).")]
    public ClampedFloatParameter focusCenter = new ClampedFloatParameter(0.5f, 0f, 1f);

    [Tooltip("Half-height of the fully-sharp focus band (0-1 UV space).")]
    public ClampedFloatParameter focusWidth = new ClampedFloatParameter(0.1f, 0f, 0.5f);

    [Tooltip("Softness of the blur transition at band edges.")]
    public ClampedFloatParameter focusFalloff = new ClampedFloatParameter(0.15f, 0.01f, 0.5f);

    [Tooltip("Saturation boost — tilt-shift photos often look hyper-saturated (1 = unchanged).")]
    public ClampedFloatParameter saturation = new ClampedFloatParameter(1.3f, 0f, 3f);

    public bool IsActive() => blurSize.value > 0f;
    public bool IsTileCompatible() => false;
}
