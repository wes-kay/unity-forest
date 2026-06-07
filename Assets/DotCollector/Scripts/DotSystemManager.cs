using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

// DotSystemManager — attach to one GameObject in scene.
//
// GPU pipeline each frame:
//   1. Spawn   — CPU writes new DotState entries, partial SetData upload
//   2. Simulate — SimulateDots.compute   (grow, drift)
//   3. ClearHash — SpatialHash.compute::ClearHash
//   4. BuildHash — SpatialHash.compute::BuildHash
//   5. Vacuum  — VacuumCollect.compute  (pull dots toward cursor on click)
//   6. Draw    — DrawMeshInstancedIndirect, 1 draw call, 6 verts per dot (no mesh needed)
//
// Inspector wiring:
//   simulateCS    → SimulateDots.compute
//   spatialHashCS → SpatialHash.compute
//   vacuumCS      → VacuumCollect.compute
//   dotMaterial   → material using Dot.shader
//   (dotMesh can be left null — a procedural quad is generated)

public class DotSystemManager : MonoBehaviour
{
    [Header("Compute Shaders")]
    public ComputeShader simulateCS;
    public ComputeShader spatialHashCS;
    public ComputeShader vacuumCS;

    [Header("Rendering")]
    public Mesh     dotMesh;      // leave null to auto-generate
    public Material dotMaterial;  // Dot.shader

    [Header("Spawn Area")]
    [Tooltip("Half-extents of the rectangular spawn area in world units.")]
    public Vector2 spawnAreaHalfExtents = new Vector2(8f, 5f);

    [Header("Vacuum")]
    [Tooltip("Speed dots travel toward the cursor when inside vacuum radius (world units/sec).")]
    public float pullSpeed = 12f;

    [Header("Score")]
    public int score = 0;

    // ---------------------------------------------------------------
    // GPU buffers
    // ---------------------------------------------------------------
    GraphicsBuffer _dotBuffer;
    GraphicsBuffer _argsBuffer;
    GraphicsBuffer _hashBuffer;
    GraphicsBuffer _scoreBuffer;

    const int HASH_GRID_W = 64;
    const int HASH_GRID_H = 64;
    const int HASH_SLOTS  = 8;
    const int DOT_STRIDE  = 5 * 4;   // 5 floats × 4 bytes

    // ---------------------------------------------------------------
    // CPU state
    // ---------------------------------------------------------------
    DotState[] _dotArray;
    int        _capacity   = 0;
    int        _activeDots = 0;
    float      _spawnAccum = 0f;

    int _simKernel;
    int _clearHashKernel;
    int _buildHashKernel;
    int _vacKernel;

    bool _readbackPending;

    // ---------------------------------------------------------------
    // Lifecycle
    // ---------------------------------------------------------------
    void Start()
    {
        _capacity = UpgradeManager.MaxActiveDots;
        AllocateBuffers(_capacity);
        CacheKernels();

        if (dotMesh == null)
            dotMesh = BuildQuadMesh();
    }

    void Update()
    {
        if (UpgradeManager.MaxActiveDots > _capacity)
            ResizeBuffers(UpgradeManager.MaxActiveDots);

        SpawnDotsThisFrame();
        RunSimulate();
        RunSpatialHash();

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            RunVacuum();

        Draw();
    }

    void OnDestroy()
    {
        _dotBuffer?.Dispose();
        _argsBuffer?.Dispose();
        _hashBuffer?.Dispose();
        _scoreBuffer?.Dispose();
    }

    // ---------------------------------------------------------------
    // Buffer allocation
    // ---------------------------------------------------------------
    void AllocateBuffers(int cap)
    {
        _dotBuffer?.Dispose();
        _argsBuffer?.Dispose();
        _hashBuffer?.Dispose();
        _scoreBuffer?.Dispose();

        _dotBuffer   = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                            cap, DOT_STRIDE);
        _argsBuffer  = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments,
                            5, sizeof(uint));
        _hashBuffer  = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                            HASH_GRID_W * HASH_GRID_H * HASH_SLOTS, sizeof(uint));
        _scoreBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured,
                            1, sizeof(int));

        _dotArray = new DotState[cap];
        _scoreBuffer.SetData(new int[] { 0 });
        UpdateDrawArgs();
    }

    void ResizeBuffers(int newCap)
    {
        var oldArray      = _dotArray;
        int oldActiveDots = _activeDots;
        AllocateBuffers(newCap);
        _capacity = newCap;

        int toCopy = Mathf.Min(oldActiveDots, newCap);
        System.Array.Copy(oldArray, _dotArray, toCopy);
        if (toCopy > 0)
            _dotBuffer.SetData(_dotArray, 0, 0, toCopy);
        _activeDots = toCopy;
        UpdateDrawArgs();
    }

    void CacheKernels()
    {
        _simKernel       = simulateCS.FindKernel("SimulateDots");
        _clearHashKernel = spatialHashCS.FindKernel("ClearHash");
        _buildHashKernel = spatialHashCS.FindKernel("BuildHash");
        _vacKernel       = vacuumCS.FindKernel("VacuumCollect");
    }

    // ---------------------------------------------------------------
    // Spawning
    // Each dot spawns at a random position inside spawnAreaHalfExtents.
    //   scale    = MinDotSize  (starts tiny)
    //   maxScale = random [MinDotSize*2 .. MaxDotSize]  (per-dot growth target)
    //   state    = 0  (alive)
    // ---------------------------------------------------------------
    void SpawnDotsThisFrame()
    {
        _spawnAccum += UpgradeManager.SpawnRate * Time.deltaTime;
        int toSpawn = Mathf.FloorToInt(_spawnAccum);
        _spawnAccum -= toSpawn;

        toSpawn = Mathf.Min(toSpawn, _capacity - _activeDots);
        if (toSpawn <= 0) return;

        float minS = UpgradeManager.MinDotSize;
        float maxS = UpgradeManager.MaxDotSize;
        Vector2 half = spawnAreaHalfExtents;

        int writeStart = _activeDots;
        for (int i = 0; i < toSpawn; i++)
        {
            _dotArray[writeStart + i] = new DotState
            {
                position = new UnityEngine.Vector2(
                    UnityEngine.Random.Range(-half.x, half.x),
                    UnityEngine.Random.Range(-half.y, half.y)),
                scale    = minS,
                maxScale = UnityEngine.Random.Range(minS * 2f, maxS),
                state    = 0f
            };
        }

        _dotBuffer.SetData(_dotArray, writeStart, writeStart, toSpawn);
        _activeDots += toSpawn;
        UpdateDrawArgs();
    }

    // ---------------------------------------------------------------
    // Compute: Simulate (grow + drift)
    // ---------------------------------------------------------------
    void RunSimulate()
    {
        if (_activeDots == 0) return;
        simulateCS.SetBuffer(_simKernel, "_Dots",      _dotBuffer);
        simulateCS.SetInt   ("_DotCount",              _activeDots);
        simulateCS.SetFloat ("_DeltaTime",             Time.deltaTime);
        simulateCS.SetFloat ("_GrowthRate",            UpgradeManager.GrowthRate);
        simulateCS.SetFloat ("_Time",                  Time.time);
        simulateCS.Dispatch (_simKernel, Mathf.CeilToInt(_activeDots / 64f), 1, 1);
    }

    // ---------------------------------------------------------------
    // Compute: Spatial hash (clear then build — must be two dispatches)
    // ---------------------------------------------------------------
    void RunSpatialHash()
    {
        if (_activeDots == 0) return;

        int totalSlots = HASH_GRID_W * HASH_GRID_H * HASH_SLOTS;

        // Pass 1 — clear
        spatialHashCS.SetBuffer(_clearHashKernel, "_Hash", _hashBuffer);
        spatialHashCS.Dispatch(_clearHashKernel, Mathf.CeilToInt(totalSlots / 64f), 1, 1);

        // Pass 2 — insert
        spatialHashCS.SetBuffer(_buildHashKernel, "_Dots",      _dotBuffer);
        spatialHashCS.SetBuffer(_buildHashKernel, "_Hash",      _hashBuffer);
        spatialHashCS.SetInt   ("_DotCount",                    _activeDots);
        spatialHashCS.SetVector("_SpawnHalf", new Vector4(
            spawnAreaHalfExtents.x, spawnAreaHalfExtents.y, 0, 0));
        spatialHashCS.Dispatch(_buildHashKernel, Mathf.CeilToInt(_activeDots / 64f), 1, 1);
    }

    // ---------------------------------------------------------------
    // Compute: Vacuum — pull dots toward cursor, collect on arrival
    // ---------------------------------------------------------------
    void RunVacuum()
    {
        if (_activeDots == 0) return;

        // Screen → world (orthographic camera on z=0 plane)
        Vector2 screenPos = Mouse.current.position.ReadValue();
        float   camZ      = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 wp        = Camera.main.ScreenToWorldPoint(
                                new Vector3(screenPos.x, screenPos.y, camZ));

        vacuumCS.SetBuffer(_vacKernel, "_Dots",        _dotBuffer);
        vacuumCS.SetBuffer(_vacKernel, "_ScoreBuffer", _scoreBuffer);
        vacuumCS.SetVector("_CursorPos",  new Vector4(wp.x, wp.y, 0, 0));
        vacuumCS.SetFloat ("_Radius",     UpgradeManager.VacuumRadius);
        vacuumCS.SetFloat ("_PullSpeed",  pullSpeed);
        vacuumCS.SetFloat ("_DeltaTime",  Time.deltaTime);
        vacuumCS.SetInt   ("_DotCount",   _activeDots);

        vacuumCS.Dispatch(_vacKernel, Mathf.CeilToInt(_activeDots / 64f), 1, 1);

        if (!_readbackPending)
        {
            _readbackPending = true;
            AsyncGPUReadback.Request(_scoreBuffer, OnScoreReadback);
        }
    }

    // ---------------------------------------------------------------
    // Render — one DrawMeshInstancedIndirect call
    // ---------------------------------------------------------------
    void Draw()
    {
        if (_activeDots == 0 || dotMaterial == null) return;

        dotMaterial.SetBuffer("_DotBuffer", _dotBuffer);

        Graphics.DrawMeshInstancedIndirect(
            dotMesh, 0, dotMaterial,
            new Bounds(Vector3.zero, new Vector3(
                spawnAreaHalfExtents.x * 2f + 2f,
                spawnAreaHalfExtents.y * 2f + 2f, 1f)),
            _argsBuffer);
    }

    void UpdateDrawArgs()
    {
        if (dotMesh == null || _argsBuffer == null) return;
        _argsBuffer.SetData(new uint[] {
            (uint)dotMesh.GetIndexCount(0),
            (uint)_activeDots,
            0, 0, 0
        });
    }

    // ---------------------------------------------------------------
    // Score readback (non-blocking)
    // ---------------------------------------------------------------
    void OnScoreReadback(AsyncGPUReadbackRequest req)
    {
        _readbackPending = false;
        if (req.hasError) return;
        var data = req.GetData<int>();
        if (data.Length > 0 && data[0] > 0)
        {
            score += data[0];
            _scoreBuffer.SetData(new int[] { 0 });
            Debug.Log($"[Score] +{data[0]}  Total: {score}");
        }
    }

    // ---------------------------------------------------------------
    // Procedural quad mesh — 6 verts (2 tris), matches shader layout
    // ---------------------------------------------------------------
    static Mesh BuildQuadMesh()
    {
        var m = new Mesh { name = "DotQuad" };
        // 6 unique vertices so SV_VertexID == index into POS[6] in shader
        m.vertices = new UnityEngine.Vector3[]
        {
            new(-0.5f, -0.5f, 0),  // 0 BL
            new( 0.5f, -0.5f, 0),  // 1 BR
            new(-0.5f,  0.5f, 0),  // 2 TL
            new( 0.5f, -0.5f, 0),  // 3 BR (dup)
            new( 0.5f,  0.5f, 0),  // 4 TR
            new(-0.5f,  0.5f, 0),  // 5 TL (dup)
        };
        m.uv = new UnityEngine.Vector2[]
        {
            new(0,0), new(1,0), new(0,1),
            new(1,0), new(1,1), new(0,1)
        };
        m.triangles = new int[] { 0,1,2, 3,4,5 };
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
