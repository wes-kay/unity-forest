using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;

namespace Domain.MVP.Settlement
{
    /// <summary>
    /// View for the Settlement tab. Manages subtab buttons, content panels,
    /// and all settlement-specific UI elements (resource cards, building grid, etc.).
    /// </summary>
    public class SettlementTabView : TabView
    {
        // ==================== Overview Subtab ====================

        [Header("Overview Panels")]
        [Tooltip("Container for resource cards in the overview.")]
        public RectTransform resourceCardsContainer;

        [Tooltip("Prefab for a resource card (icon + value + label).")]
        public GameObject resourceCardPrefab;

        [Tooltip("Container for quick action buttons.")]
        public RectTransform quickActionsContainer;

        [Tooltip("Prefab for a quick action button.")]
        public GameObject quickActionPrefab;

        [Tooltip("Container for settlement overview stats (population, reputation).")]
        public RectTransform overviewStatsContainer;

        /// <summary>Fired when a resource card is hovered.</summary>
        public event Action<string> OnResourceCardHovered;

        /// <summary>Fired when a quick action is clicked.</summary>
        public event Action<string> OnQuickActionClicked;

        // ==================== Buildings Subtab ====================

        [Header("Buildings Panels")]
        [Tooltip("Container for the building list.")]
        public RectTransform buildingListContainer;

        [Tooltip("Prefab for a building entry (icon, name, level bar, upgrade button).")]
        public GameObject buildingEntryPrefab;

        [Tooltip("Container for the upgrade confirmation panel.")]
        public GameObject upgradePanel;

        [Tooltip("Text showing the building name in the upgrade panel.")]
        public TextMeshProUGUI upgradeBuildingNameText;

        [Tooltip("Text showing the gold cost in the upgrade panel.")]
        public TextMeshProUGUI upgradeGoldCostText;

        [Tooltip("Text showing the materials cost in the upgrade panel.")]
        public TextMeshProUGUI upgradeMatCostText;

        [Tooltip("Upgrade button.")]
        public Button upgradeConfirmButton;

        [Tooltip("Cancel button in the upgrade panel.")]
        public Button upgradeCancelButton;

        /// <summary>Fired when a building entry is clicked.</summary>
        public event Action<string> OnBuildingClicked;

        /// <summary>Fired when upgrade is confirmed.</summary>
        public event Action OnUpgradeConfirmed;

        /// <summary>Fired when upgrade is cancelled.</summary>
        public event Action OnUpgradeCancelled;

        // ==================== Resources Subtab ====================

        [Header("Resources Panels")]
        [Tooltip("Container for the detailed resource list.")]
        public RectTransform resourceDetailContainer;

        [Tooltip("Prefab for a resource detail entry.")]
        public GameObject resourceEntryPrefab;

        // ==================== Projects Subtab ====================

        [Header("Projects Panels")]
        [Tooltip("Container for the active projects list.")]
        public RectTransform projectListContainer;

        [Tooltip("Prefab for a project entry (name, progress bar, priority badge).")]
        public GameObject projectEntryPrefab;

        // ==================== Factions Subtab ====================

        [Header("Factions Panels")]
        [Tooltip("Container for the faction list.")]
        public RectTransform factionListContainer;

        [Tooltip("Prefab for a faction entry (icon, name, reputation bar, status).")]
        public GameObject factionEntryPrefab;

        // ==================== Visitors Subtab ====================

        [Header("Visitors Panels")]
        [Tooltip("Container for the visitor list.")]
        public RectTransform visitorListContainer;

        [Tooltip("Prefab for a visitor entry (icon, name, role, morale bar, trust icon).")]
        public GameObject visitorEntryPrefab;

        // ==================== Shared UI References ====================

        [Header("Shared References")]
        [Tooltip("Text showing the settlement name.")]
        public TextMeshProUGUI settlementNameText;

        [Tooltip("Text showing population count.")]
        public TextMeshProUGUI populationText;

        [Tooltip("Text showing reputation level.")]
        public TextMeshProUGUI reputationText;

        private string _currentUpgradeBuildingId;
        private List<GameObject> _resourceCards;
        private List<GameObject> _buildingEntries;
        private List<GameObject> _resourceEntries;
        private List<GameObject> _projectEntries;
        private List<GameObject> _factionEntries;
        private List<GameObject> _visitorEntries;
        private List<GameObject> _quickActions;

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);

            _resourceCards = new List<GameObject>();
            _buildingEntries = new List<GameObject>();
            _resourceEntries = new List<GameObject>();
            _projectEntries = new List<GameObject>();
            _factionEntries = new List<GameObject>();
            _visitorEntries = new List<GameObject>();
            _quickActions = new List<GameObject>();

            // Wire up upgrade panel buttons
            if (upgradeConfirmButton != null)
            {
                upgradeConfirmButton.onClick.AddListener(() => OnUpgradeConfirmed?.Invoke());
            }
            if (upgradeCancelButton != null)
            {
                upgradeCancelButton.onClick.AddListener(() => OnUpgradeCancelled?.Invoke());
            }
        }

        // ==================== Overview Methods ====================

        /// <summary>Set the settlement name.</summary>
        public void SetSettlementName(string name)
        {
            if (settlementNameText != null)
            {
                settlementNameText.text = name;
            }
        }

        /// <summary>Set population display text.</summary>
        public void SetPopulationText(string text)
        {
            if (populationText != null)
            {
                populationText.text = text;
            }
        }

        /// <summary>Set reputation level display.</summary>
        public void SetReputationLevel(int level)
        {
            if (reputationText != null)
            {
                reputationText.text = string.Format("Reputation: {0}/5", level);
            }
        }

        /// <summary>Refresh resource cards with current values.</summary>
        public void RefreshResourceCards(List<ResourceInfo> resources)
        {
            // Clear existing cards
            foreach (var card in _resourceCards)
            {
                Destroy(card);
            }
            _resourceCards.Clear();

            if (resourceCardsContainer == null || resourceCardPrefab == null) return;

            foreach (var resource in resources)
            {
                var card = Instantiate(resourceCardPrefab, resourceCardsContainer);
                card.SetActive(true);

                var valueText = card.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
                var labelText = card.transform.Find("LabelText")?.GetComponent<TextMeshProUGUI>();
                var iconImage = card.transform.Find("Icon")?.GetComponent<Image>();
                var barImage = card.transform.Find("BarFill")?.GetComponent<Image>();

                if (valueText != null) valueText.text = resource.Current.ToString();
                if (labelText != null) labelText.text = resource.Name;
                if (barImage != null) barImage.fillAmount = resource.CapacityPercent;

                var cardId = resource.Name;
                card.GetComponent<Button>().onClick.AddListener(() => OnResourceCardHovered?.Invoke(cardId));

                _resourceCards.Add(card);
            }
        }

        /// <summary>Refresh quick action buttons.</summary>
        public void RefreshQuickActions(List<string> actions)
        {
            foreach (var action in _quickActions)
            {
                Destroy(action);
            }
            _quickActions.Clear();

            if (quickActionsContainer == null || quickActionPrefab == null) return;

            foreach (var action in actions)
            {
                var btn = Instantiate(quickActionPrefab, quickActionsContainer);
                btn.SetActive(true);

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = action;

                var actionId = action;
                btn.GetComponent<Button>().onClick.AddListener(() => OnQuickActionClicked?.Invoke(actionId));

                _quickActions.Add(btn);
            }
        }

        // ==================== Buildings Methods ====================

        /// <summary>Refresh the building list grid.</summary>
        public void RefreshBuildingGrid(List<BuildingInfo> buildings)
        {
            foreach (var entry in _buildingEntries)
            {
                Destroy(entry);
            }
            _buildingEntries.Clear();

            if (buildingListContainer == null || buildingEntryPrefab == null) return;

            foreach (var building in buildings)
            {
                var entry = Instantiate(buildingEntryPrefab, buildingListContainer);
                entry.SetActive(true);

                var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var levelText = entry.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
                var levelBar = entry.transform.Find("LevelBar")?.GetComponent<Image>();
                var upgradeBtn = entry.transform.Find("UpgradeButton")?.GetComponent<Button>();
                var levelBarFill = entry.transform.Find("LevelBarFill")?.GetComponent<Image>();

                if (nameText != null) nameText.text = building.Name;
                if (levelText != null) levelText.text = string.Format("Lv {0}/{1}", building.Level, building.MaxLevel);
                if (levelBarFill != null) levelBarFill.fillAmount = (float)building.Level / building.MaxLevel;

                // Show/hide upgrade button
                if (upgradeBtn != null)
                {
                    upgradeBtn.gameObject.SetActive(building.IsUpgradable);
                    var upgradeBtnText = upgradeBtn.GetComponentInChildren<TextMeshProUGUI>();
                    if (upgradeBtnText != null) upgradeBtnText.text = "Upgrade";
                }

                if (levelBar != null) levelBar.fillAmount = 1f;

                var buildingId = building.Id;
                entry.GetComponent<Button>().onClick.AddListener(() => OnBuildingClicked?.Invoke(buildingId));

                _buildingEntries.Add(entry);
            }
        }

        /// <summary>Show the upgrade confirmation panel for a building.</summary>
        public void ShowUpgradePanel(string buildingId, string buildingName, int goldCost, int matCost)
        {
            _currentUpgradeBuildingId = buildingId;

            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
            }

            if (upgradeBuildingNameText != null) upgradeBuildingNameText.text = buildingName;
            if (upgradeGoldCostText != null) upgradeGoldCostText.text = goldCost.ToString();
            if (upgradeMatCostText != null) upgradeMatCostText.text = matCost.ToString();
        }

        /// <summary>Hide the upgrade confirmation panel.</summary>
        public void HideUpgradePanel()
        {
            _currentUpgradeBuildingId = null;
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
        }

        /// <summary>Get the currently selected building ID from the upgrade panel.</summary>
        public string GetCurrentUpgradeBuildingId()
        {
            return _currentUpgradeBuildingId;
        }

        /// <summary>Disable the upgrade button (e.g. when resources insufficient).</summary>
        public void SetUpgradeButtonEnabled(bool enabled)
        {
            if (upgradeConfirmButton != null)
            {
                upgradeConfirmButton.interactable = enabled;
            }
        }

        // ==================== Resources Detail Methods ====================

        /// <summary>Refresh the detailed resource list.</summary>
        public void RefreshResourceDetails(List<ResourceInfo> resources)
        {
            foreach (var entry in _resourceEntries)
            {
                Destroy(entry);
            }
            _resourceEntries.Clear();

            if (resourceDetailContainer == null || resourceEntryPrefab == null) return;

            foreach (var resource in resources)
            {
                var entry = Instantiate(resourceEntryPrefab, resourceDetailContainer);
                entry.SetActive(true);

                var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var valueText = entry.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
                var productionText = entry.transform.Find("ProductionText")?.GetComponent<TextMeshProUGUI>();
                var barFill = entry.transform.Find("BarFill")?.GetComponent<Image>();

                if (nameText != null) nameText.text = resource.Name;
                if (valueText != null) valueText.text = string.Format("{0} / {1}", resource.Current, resource.Max);
                if (productionText != null) productionText.text = string.Format("+{0}/tick", resource.Production);
                if (barFill != null) barFill.fillAmount = resource.CapacityPercent;

                _resourceEntries.Add(entry);
            }
        }

        // ==================== Projects Methods ====================

        /// <summary>Refresh the active projects list.</summary>
        public void RefreshProjectList(List<ProjectInfo> projects)
        {
            foreach (var entry in _projectEntries)
            {
                Destroy(entry);
            }
            _projectEntries.Clear();

            if (projectListContainer == null || projectEntryPrefab == null) return;

            foreach (var project in projects)
            {
                var entry = Instantiate(projectEntryPrefab, projectListContainer);
                entry.SetActive(true);

                var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var progressText = entry.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
                var barFill = entry.transform.Find("BarFill")?.GetComponent<Image>();
                var priorityBadge = entry.transform.Find("PriorityBadge")?.GetComponent<TextMeshProUGUI>();

                if (nameText != null) nameText.text = project.Name;
                if (progressText != null) progressText.text = string.Format("{0} / {1} ({2:P0})", project.Progress, project.Total, project.CompletionPercent);
                if (barFill != null) barFill.fillAmount = project.CompletionPercent;
                if (priorityBadge != null)
                {
                    priorityBadge.gameObject.SetActive(project.Priority > 0);
                    priorityBadge.text = project.Priority == 1 ? "High" : "Medium";
                }

                _projectEntries.Add(entry);
            }
        }

        // ==================== Factions Methods ====================

        /// <summary>Refresh the faction list.</summary>
        public void RefreshFactionList(List<FactionInfo> factions)
        {
            foreach (var entry in _factionEntries)
            {
                Destroy(entry);
            }
            _factionEntries.Clear();

            if (factionListContainer == null || factionEntryPrefab == null) return;

            foreach (var faction in factions)
            {
                var entry = Instantiate(factionEntryPrefab, factionListContainer);
                entry.SetActive(true);

                var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var repText = entry.transform.Find("RepText")?.GetComponent<TextMeshProUGUI>();
                var statusText = entry.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
                var repBarFill = entry.transform.Find("RepBarFill")?.GetComponent<Image>();

                if (nameText != null) nameText.text = faction.Name;
                if (repText != null) repText.text = string.Format("{0}", faction.Reputation);
                if (statusText != null) statusText.text = faction.Status;

                // Reputation bar: -100 (left/empty) to 100 (right/full) → map to 0-1
                if (repBarFill != null)
                {
                    repBarFill.fillAmount = (faction.Reputation + 100f) / 200f;
                    // Color based on reputation: red (hostile) → yellow (neutral) → green (friendly)
                    if (faction.Reputation < -20)
                    {
                        repBarFill.color = new Color(0.7f, 0.15f, 0.1f);
                    }
                    else if (faction.Reputation < 20)
                    {
                        repBarFill.color = new Color(0.8f, 0.75f, 0.1f);
                    }
                    else
                    {
                        repBarFill.color = new Color(0.15f, 0.6f, 0.15f);
                    }
                }

                _factionEntries.Add(entry);
            }
        }

        // ==================== Visitors Methods ====================

        /// <summary>Refresh the visitor list.</summary>
        public void RefreshVisitorList(List<VisitorInfo> visitors)
        {
            foreach (var entry in _visitorEntries)
            {
                Destroy(entry);
            }
            _visitorEntries.Clear();

            if (visitorListContainer == null || visitorEntryPrefab == null) return;

            foreach (var visitor in visitors)
            {
                var entry = Instantiate(visitorEntryPrefab, visitorListContainer);
                entry.SetActive(true);

                var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                var roleText = entry.transform.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
                var moraleFill = entry.transform.Find("MoraleBarFill")?.GetComponent<Image>();
                var trustIcon = entry.transform.Find("TrustIcon")?.GetComponent<Image>();

                if (nameText != null) nameText.text = visitor.Name;
                if (roleText != null) roleText.text = visitor.Role;
                if (moraleFill != null) moraleFill.fillAmount = visitor.Morale / 100f;
                if (trustIcon != null) trustIcon.gameObject.SetActive(visitor.IsTrusted);

                _visitorEntries.Add(entry);
            }
        }

        // ==================== TabView Override ====================

        /// <summary>Called when a subtab button is clicked (from base class).</summary>
        public void OnSubtabButtonClicked(string subtabId)
        {
            // The base TabView handles showing/hiding subtab content panels.
            // Override this method in concrete view to handle subtab-specific logic.
        }
    }
}
