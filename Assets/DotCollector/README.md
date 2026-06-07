# Dot Collector — Unity URP 6000.4
GPU-driven dot spawning, growing, and vacuum collection system.

## Files

```
Scripts/
  DotState.cs          — shared struct (CPU + GPU must match)
  UpgradeManager.cs    — static class, all tunable parameters
  DotSystemManager.cs  — main MonoBehaviour, drives the GPU pipeline
  UpgradeUI.cs         — Canvas wiring for upgrade buttons + score display

ComputeShaders/
  SimulateDots.compute  — per-dot grow & drift
  SpatialHash.compute   — 64×64 hash grid builder
  VacuumCollect.compute — cursor radius collect

Shaders/
  Dot.shader            — URP Unlit, 1 draw call via DrawMeshInstancedIndirect
```

---

## Setup (5 steps)

### 1. Import files
Drop all folders into your Unity `Assets/` directory.

### 2. Create the DotSystemManager GameObject
- Create an empty GameObject, name it `DotSystem`.
- Add `DotSystemManager` component.
- In the Inspector, assign:
  - **Simulate CS** → `SimulateDots.compute`
  - **Spatial Hash CS** → `SpatialHash.compute`
  - **Vacuum CS** → `VacuumCollect.compute`
  - **Dot Material** → Create a new Material using `Custom/DotInstanced` shader
  - **Dot Mesh** → Leave blank (auto-generates a quad) or assign your own Quad mesh
  - **Spawn Area Half Extents** → matches your camera's visible world area (default `8, 5`)

### 3. Camera setup
- Use an Orthographic camera facing –Z.
- Set Size to cover your spawn area (e.g. Size = 5 for a 16:9 screen at half-extents `8,5`).

### 4. Set up the UI (optional)
- Create a Canvas (Screen Space – Overlay).
- Add 4 Buttons and 2 TMP Text fields.
- Add `UpgradeUI` component to the Canvas.
- Wire up the buttons and labels in the Inspector.

### 5. URP asset settings
- Ensure **Depth Priming** is off (transparent pass requirement).
- Confirm your URP asset targets **Shader Model 4.5** or higher (for StructuredBuffer support).

---

## How spawn works

Dots spawn at `SpawnRate` dots per second (fractional accumulation, so at 5/s you get
exactly 5 dots per second regardless of framerate).

Each new dot gets:
| Property | Value |
|---|---|
| `position` | Random inside `spawnAreaHalfExtents` |
| `scale` (initial) | `UpgradeManager.MinDotSize` (0.04 world units) |
| `maxScale` (target) | Random in `[MinDotSize*2, MaxDotSize]` |
| `state` | `0` (alive) |

Every frame the compute shader grows `scale` toward `maxScale` at `GrowthRate` world-units/second,
with a soft easing so large dots feel heavier and slow near their max size.

---

## Upgrade parameters at each tier

### Spawn Rate (`UpgradeManager.SpawnRate`)
`5 + tier × 3` dots/second

| Tier | Dots/sec |
|---|---|
| 0 | 5 |
| 1 | 8 |
| 2 | 11 |
| 5 | 20 |

### Growth Rate (`UpgradeManager.GrowthRate`)
`0.08 + tier × 0.04` world-units/second

### Max Dot Size (`UpgradeManager.MaxDotSize`)
`0.18 + tier × 0.06` world-units radius

| Tier | Max radius |
|---|---|
| 0 | 0.18 u |
| 1 | 0.24 u |
| 3 | 0.36 u |

### Vacuum Radius (`UpgradeManager.VacuumRadius`)
`1.5 + tier × 0.5` world-units

### Capacity (`UpgradeManager.MaxActiveDots`)
`2000 + tier × 1000` simultaneous dots

---

## Performance notes

- All simulation runs in compute shaders. Zero per-dot C# code per frame.
- One `DrawMeshInstancedIndirect` call renders all dots regardless of count.
- `SpatialHash` limits vacuum queries to nearby cells — O(1) per cell vs O(n).
- Score uses `AsyncGPUReadback` — never stalls the GPU pipeline.
- Upgrading capacity triggers a buffer resize and re-uploads existing dot data.

---

## Extending

**Add dot types** — Add a `uint type` field to `DotState`. Branch in the fragment shader for colour.

**Dot lifetime** — Add a `float lifetime` field; decrement in SimulateDots, set `state = 2` at zero.

**Attraction** — In VacuumCollect, instead of instantly collecting, nudge `position` toward `_CursorPos` each frame for a smooth pull effect.

**VFX on collect** — Read `state == 1` dots in a VFX Graph `GraphicsBuffer` event to emit particles.
