using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UpgradeUI
// ---------
// Attach to a Canvas GameObject.
// Wire up the four Button references in the Inspector.
// The score text and stat readout update every frame.
//
// Upgrade costs scale with tier: cost = baseCost * (tier + 1)

public class UpgradeUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button spawnUpgradeBtn;
    public Button growthUpgradeBtn;
    public Button vacuumUpgradeBtn;
    public Button capacityUpgradeBtn;

    [Header("Labels (optional TMP)")]
    public TMP_Text scoreLabel;
    public TMP_Text statsLabel;

    [Header("Costs")]
    public int spawnBaseCost    = 10;
    public int growthBaseCost   = 15;
    public int vacuumBaseCost   = 20;
    public int capacityBaseCost = 25;

    DotSystemManager _mgr;

    void Start()
    {
        _mgr = FindObjectOfType<DotSystemManager>();

        spawnUpgradeBtn   .onClick.AddListener(BuySpawn);
        growthUpgradeBtn  .onClick.AddListener(BuyGrowth);
        vacuumUpgradeBtn  .onClick.AddListener(BuyVacuum);
        capacityUpgradeBtn.onClick.AddListener(BuyCapacity);
    }

    void Update()
    {
        if (_mgr == null) return;

        if (scoreLabel != null)
            scoreLabel.text = $"Score: {_mgr.score}";

        if (statsLabel != null)
        {
            statsLabel.text =
                $"Spawn: {UpgradeManager.SpawnRate:F1}/s  (tier {UpgradeManager.SpawnTier})\n" +
                $"Growth: {UpgradeManager.GrowthRate:F3} u/s  max size: {UpgradeManager.MaxDotSize:F2} u  (tier {UpgradeManager.GrowthTier})\n" +
                $"Vacuum radius: {UpgradeManager.VacuumRadius:F1} u  (tier {UpgradeManager.VacuumTier})\n" +
                $"Capacity: {UpgradeManager.MaxActiveDots}  (tier {UpgradeManager.CapacityTier})";
        }

        // Update button labels with next cost
        SetButtonLabel(spawnUpgradeBtn,    $"Spawn +\n{SpawnCost()} pts");
        SetButtonLabel(growthUpgradeBtn,   $"Growth +\n{GrowthCost()} pts");
        SetButtonLabel(vacuumUpgradeBtn,   $"Vacuum +\n{VacuumCost()} pts");
        SetButtonLabel(capacityUpgradeBtn, $"Capacity +\n{CapacityCost()} pts");
    }

    // ------------------------------------------------------------------
    // Purchase handlers
    // ------------------------------------------------------------------
    void BuySpawn()
    {
        if (_mgr.score < SpawnCost()) return;
        _mgr.score -= SpawnCost();
        UpgradeManager.UpgradeSpawn();
    }

    void BuyGrowth()
    {
        if (_mgr.score < GrowthCost()) return;
        _mgr.score -= GrowthCost();
        UpgradeManager.UpgradeGrowth();
    }

    void BuyVacuum()
    {
        if (_mgr.score < VacuumCost()) return;
        _mgr.score -= VacuumCost();
        UpgradeManager.UpgradeVacuum();
    }

    void BuyCapacity()
    {
        if (_mgr.score < CapacityCost()) return;
        _mgr.score -= CapacityCost();
        UpgradeManager.UpgradeCapacity();
    }

    // ------------------------------------------------------------------
    // Cost formulas
    // ------------------------------------------------------------------
    int SpawnCost()    => spawnBaseCost    * (UpgradeManager.SpawnTier    + 1);
    int GrowthCost()   => growthBaseCost   * (UpgradeManager.GrowthTier   + 1);
    int VacuumCost()   => vacuumBaseCost   * (UpgradeManager.VacuumTier   + 1);
    int CapacityCost() => capacityBaseCost * (UpgradeManager.CapacityTier + 1);

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------
    void SetButtonLabel(Button btn, string text)
    {
        var lbl = btn.GetComponentInChildren<TMP_Text>();
        if (lbl != null) lbl.text = text;
    }
}
