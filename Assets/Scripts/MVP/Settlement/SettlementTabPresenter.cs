using System;
using System.Collections.Generic;
using Domain.MVP.Tab;
using UnityEngine;
using Zenject;

namespace Domain.MVP.Settlement
{
    /// <summary>
    /// Presenter for the Settlement tab. Mediates between SettlementTabModel and SettlementTabView,
    /// handles building upgrades, resource updates, and subtab content refresh.
    /// </summary>
    public class SettlementTabPresenter : TabPresenter<SettlementTabModel, SettlementTabView>
    {
        [Inject] private SettlementTabView _settlementView;

        private SettlementTabModel SettlementModel => _model as SettlementTabModel;

        // Tracks which subtab is currently displayed so RefreshCurrentSubtab always has a target
        private string _currentSubtab;

        public override void Initialize()
        {
            base.Initialize();

            // Seed _currentSubtab from the model so the first OnTabActivated refresh works
            _currentSubtab = _model?.ActiveSubtab;

            _settlementView.OnSubtabClicked += OnViewSubtabClicked;
            _settlementView.OnResourceCardHovered += OnResourceCardHovered;
            _settlementView.OnQuickActionClicked += OnQuickActionClicked;
            _settlementView.OnBuildingClicked += OnBuildingClicked;
            _settlementView.OnUpgradeConfirmed += OnUpgradeConfirmed;
            _settlementView.OnUpgradeCancelled += OnUpgradeCancelled;
        }

        public override void OnTabActivated()
        {
            SettlementModel.LoadFromService();

            // Ensure _currentSubtab is valid before refreshing
            if (string.IsNullOrEmpty(_currentSubtab))
                _currentSubtab = _model?.ActiveSubtab;

            RefreshCurrentSubtab();
        }

        public override void OnTabDeactivated()
        {
            // No persistent state to save in MVP; data is reloaded on activate
        }

        public override void OnSubtabChanged(string subtabId)
        {
            _currentSubtab = subtabId;
            RefreshCurrentSubtab();
        }

        // ==================== Content Refresh ====================

        private void RefreshCurrentSubtab()
        {
            if (string.IsNullOrEmpty(_currentSubtab) || SettlementModel == null) return;

            switch (_currentSubtab)
            {
                case "overview":   RefreshOverview();   break;
                case "buildings":  RefreshBuildings();  break;
                case "resources":  RefreshResources();  break;
                case "projects":   RefreshProjects();   break;
                case "factions":   RefreshFactions();   break;
                case "visitors":   RefreshVisitors();   break;
            }
        }

        private void RefreshOverview()
        {
            if (_settlementView == null || SettlementModel == null) return;

            _settlementView.SetSettlementName("The Hamlet");
            _settlementView.SetPopulationText(string.Format("{0} / {1}", SettlementModel.Population, SettlementModel.MaxPopulation));
            _settlementView.SetReputationLevel(SettlementModel.ReputationLevel);

            var resourceList = new List<ResourceInfo>(SettlementModel.Resources);
            _settlementView.RefreshResourceCards(resourceList);

            var quickActions = new List<string> { "Send Scouts", "Send Foragers", "Post Bounties", "Open Market" };
            _settlementView.RefreshQuickActions(quickActions);
        }

        private void RefreshBuildings()
        {
            if (_settlementView == null || SettlementModel == null) return;
            _settlementView.RefreshBuildingGrid(new List<BuildingInfo>(SettlementModel.Buildings));
        }

        private void RefreshResources()
        {
            if (_settlementView == null || SettlementModel == null) return;
            _settlementView.RefreshResourceDetails(new List<ResourceInfo>(SettlementModel.Resources));
        }

        private void RefreshProjects()
        {
            if (_settlementView == null || SettlementModel == null) return;
            _settlementView.RefreshProjectList(new List<ProjectInfo>(SettlementModel.ActiveProjects));
        }

        private void RefreshFactions()
        {
            if (_settlementView == null || SettlementModel == null) return;
            _settlementView.RefreshFactionList(new List<FactionInfo>(SettlementModel.Factions));
        }

        private void RefreshVisitors()
        {
            if (_settlementView == null || SettlementModel == null) return;
            _settlementView.RefreshVisitorList(new List<VisitorInfo>(SettlementModel.Visitors));
        }

        // ==================== Event Handlers ====================

        private void OnViewSubtabClicked(string subtabId)
        {
            // Handled by base TabPresenter via _model.SetActiveSubtab → OnSubtabChanged
        }

        private void OnResourceCardHovered(string resourceName)
        {
            Debug.LogFormat("Resource hovered: {0}", resourceName);
        }

        private void OnQuickActionClicked(string actionName)
        {
            Debug.LogFormat("Quick action: {0}", actionName);
        }

        private void OnBuildingClicked(string buildingId)
        {
            if (SettlementModel == null) return;
            var building = SettlementModel.GetBuilding(buildingId);
            if (building.Id == null) return;

            if (building.IsUpgradable)
            {
                var costs = SettlementModel.GetUpgradeCost(building);
                _settlementView.ShowUpgradePanel(buildingId, building.Name, costs.gold, costs.materials);
            }
        }

        private void OnUpgradeConfirmed()
        {
            if (SettlementModel == null || _settlementView == null) return;

            var buildingId = _settlementView.GetCurrentUpgradeBuildingId();
            if (string.IsNullOrEmpty(buildingId)) return;

            var building = SettlementModel.GetBuilding(buildingId);
            if (building.Id == null) return;

            var costs = SettlementModel.GetUpgradeCost(building);

            if (SettlementModel.CanAffordUpgrade(building, costs.gold, costs.materials))
            {
                if (SettlementModel.PerformUpgrade(building, costs.gold, costs.materials))
                {
                    _settlementView.HideUpgradePanel();
                    RefreshBuildings();
                }
                else
                {
                    Debug.LogWarning("Upgrade failed: insufficient resources.");
                    _settlementView.SetUpgradeButtonEnabled(false);
                }
            }
            else
            {
                Debug.LogWarning("Cannot afford upgrade.");
                _settlementView.SetUpgradeButtonEnabled(false);
            }
        }

        private void OnUpgradeCancelled()
        {
            _settlementView.HideUpgradePanel();
        }

        public override void Destroy()
        {
            base.Destroy();
            if (_settlementView != null)
            {
                _settlementView.OnSubtabClicked -= OnViewSubtabClicked;
                _settlementView.OnResourceCardHovered -= OnResourceCardHovered;
                _settlementView.OnQuickActionClicked -= OnQuickActionClicked;
                _settlementView.OnBuildingClicked -= OnBuildingClicked;
                _settlementView.OnUpgradeConfirmed -= OnUpgradeConfirmed;
                _settlementView.OnUpgradeCancelled -= OnUpgradeCancelled;
            }
        }
    }
}
