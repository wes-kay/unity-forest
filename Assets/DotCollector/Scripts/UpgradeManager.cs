using UnityEngine;

// UpgradeManager
// --------------
// Central source of truth for every upgrade-driven parameter.
// All other systems read from the static properties — no direct references needed.
//
// Upgrade tiers scale these values:
//   SpawnRate      — dots spawned per second (base 5, +3 per tier)
//   GrowthRate     — world-units per second a dot grows (base 0.08, +0.04 per tier)
//   MinDotSize     — smallest possible dot radius on spawn (world units)
//   MaxDotSize     — largest possible dot radius a dot can grow to
//   VacuumRadius   — click-collect radius in world units (base 1.5, +0.5 per tier)
//   MaxActiveDots  — hard cap on simultaneous live dots (base 2000, +1000 per tier)

public static class UpgradeManager
{
    // ------------------------------------------------------------------
    // Runtime state — incremented by UI buttons
    // ------------------------------------------------------------------
    private static int _spawnTier   = 0;
    private static int _growthTier  = 0;
    private static int _vacuumTier  = 0;
    private static int _capacityTier = 0;

    // ------------------------------------------------------------------
    // Public read properties used by DotSystemManager & compute shaders
    // ------------------------------------------------------------------

    /// <summary>Dots spawned per second.</summary>
    public static float SpawnRate     => 5f  + _spawnTier   * 3f;

    /// <summary>World-units of radius growth per second.</summary>
    public static float GrowthRate    => 0.08f + _growthTier * 0.04f;

    /// <summary>Smallest spawn radius (world units).</summary>
    public static float MinDotSize    => 0.04f;

    /// <summary>
    /// Maximum radius a dot can grow to (world units).
    /// Randomised per dot between MinDotSize and this value at spawn time.
    /// </summary>
    public static float MaxDotSize    => 0.18f + _growthTier * 0.06f;

    /// <summary>Click-collect vacuum radius in world units.</summary>
    public static float VacuumRadius  => 1.5f + _vacuumTier * 0.5f;

    /// <summary>Maximum simultaneously alive dots (hard cap).</summary>
    public static int   MaxActiveDots => 2000 + _capacityTier * 1000;

    // ------------------------------------------------------------------
    // Upgrade methods — call these from your UI buttons
    // ------------------------------------------------------------------
    public static void UpgradeSpawn()    { _spawnTier++;    Debug.Log($"[Upgrade] SpawnRate → {SpawnRate}/s"); }
    public static void UpgradeGrowth()   { _growthTier++;   Debug.Log($"[Upgrade] GrowthRate → {GrowthRate} u/s, MaxDotSize → {MaxDotSize}"); }
    public static void UpgradeVacuum()   { _vacuumTier++;   Debug.Log($"[Upgrade] VacuumRadius → {VacuumRadius} u"); }
    public static void UpgradeCapacity() { _capacityTier++; Debug.Log($"[Upgrade] MaxActiveDots → {MaxActiveDots}"); }

    // Read-only tier accessors (for UI display)
    public static int SpawnTier    => _spawnTier;
    public static int GrowthTier   => _growthTier;
    public static int VacuumTier   => _vacuumTier;
    public static int CapacityTier => _capacityTier;
}
