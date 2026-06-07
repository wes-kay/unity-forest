# Tilt-Shift Post Process Effect — Unity 6000.4 URP
## Files
| File | Purpose |
|---|---|
| `TiltShift.shader` | Two-pass Gaussian blur + focus-band composite shader |
| `TiltShiftRendererFeature.cs` | Registers the effect with the URP renderer |
| `TiltShiftPass.cs` | Executes the blit passes each frame |
| `TiltShiftVolumeComponent.cs` | Volume override component (editable at runtime) |

---
## Setup (5 steps)

### 1. Import files
Drop all four files anywhere under `Assets/` (e.g. `Assets/TiltShift/`).

### 2. Create the Material
- **Assets → Create → Material**
- Name it `TiltShiftMat`
- Set **Shader** to `Custom/TiltShift`

### 3. Add the Renderer Feature
- Select your **URP Renderer Asset** (usually in `Settings/` — the asset referenced by your URP Pipeline Asset under *Renderer List*)
- In the Inspector, click **Add Renderer Feature → Tilt Shift Renderer Feature**
- Drag `TiltShiftMat` into the **Tilt Shift Material** slot

### 4. Add a Volume
- **GameObject → Volume → Global Volume** (or use an existing one)
- In the Volume's Profile, click **Add Override → Custom → Tilt Shift**
- Enable and tweak the parameters (see below)

### 5. Enable Post Processing on the Camera
- Select your Main Camera
- Check **Post Processing** in the Camera component

---
## Parameters (Volume Override)

| Parameter | Default | Description |
|---|---|---|
| **Blur Size** | 4 | Gaussian blur radius in pixels. Higher = more miniature look. |
| **Focus Center** | 0.5 | Vertical position (0 = bottom, 1 = top) of the sharp band. |
| **Focus Width** | 0.1 | Half-height of the fully-sharp zone in UV space. |
| **Focus Falloff** | 0.15 | Softness of the blur transition at the band edges. |
| **Saturation** | 1.3 | Colour saturation multiplier (1 = unchanged, >1 = vivid). |

---
## Tips
- **Aerial / city shots** work best. Point the camera down at ~45°.
- Try **Focus Center 0.45**, **Focus Width 0.08**, **Blur Size 6** for a classic tilt-shift miniature look.
- The effect runs **BeforeRenderingPostProcessing** so it composites correctly with Bloom/Color Grading.
- For even softer bokeh, increase **Blur Size** and decrease **Focus Falloff**.

---
## Upgrading to Render Graph (optional)
Unity 6 ships with a new Render Graph API. This implementation uses the **compatibility path** (`ScriptableRenderPass.Execute`) which remains fully supported in 6000.4. To migrate, implement `RecordRenderGraph` in `TiltShiftPass.cs` using `IRasterRenderGraphBuilder`.
