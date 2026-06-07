using System.Runtime.InteropServices;
using UnityEngine;

// Must match the struct layout in every compute shader that uses _DotBuffer.
// Total size: 20 bytes (5 floats). GPU reads this via StructuredBuffer<DotState>.
[StructLayout(LayoutKind.Sequential)]
public struct DotState
{
    public Vector2 position;   // world-space XY
    public float   scale;      // current radius in world units
    public float   maxScale;   // target radius this dot grows toward
    public float   state;      // 0 = alive, 1 = collected (flash), 2 = dead/recycled
}
