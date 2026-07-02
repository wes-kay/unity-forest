using System.Collections.Generic;
using UnityEngine;
using Zenject;
using SoftKitty.InventoryEngine;
using Domain.MVP.Tab;

namespace Domain.MVP.Equipment
{
    /// <summary>
    /// Presenter for the Equipment tab. Bridges MVP architecture with SoftKitty InventoryEngine.
    /// When the tab is activated, opens the SoftKitty Equipment window prefab.
    /// </summary>
    public class EquipmentTabPresenter : TabPresenter<EquipmentTabModel, EquipmentTabView>
    {
        [Inject] private EquipmentTabModel _model;
        [Inject] private EquipmentTabView _view;

        /// <summary>The currently open SoftKitty Equipment window instance.</summary>
        private UiWindow _openedWindow;

        public override void OnTabActivated()
        {
            if (!_model.IsLoaded)
            {
                _model.LoadFromService();
            }

            // Open the SoftKitty Equipment window
            OpenEquipmentWindow();

            // Sync equipment slots
            RefreshEquipSlots();
        }

        public override void OnTabDeactivated()
        {
            // Close the SoftKitty window when tab is deactivated
            CloseEquipmentWindow();
        }

        public override void OnSubtabChanged(string subtabId)
        {
            // Subtab changed — refresh equipment slots
            RefreshEquipSlots();
            _view.ClearDetailPanel();
        }

        /// <summary>Handle equipment slot click from the view.</summary>
        public void OnEquipSlotClicked(string slotId)
        {
            _model.SelectSlot(slotId);
            var equipped = _model.GetEquippedItem(slotId);
            _view.UpdateDetailPanel(equipped.name, equipped.name, equipped.icon);
            _view.SetDetailPanelActive(true);
        }

        /// <summary>Handle detail panel close from the view.</summary>
        public void OnDetailCloseClicked()
        {
            _model.ClearSelection();
            _view.ClearDetailPanel();
            _view.SetDetailPanelActive(false);
        }

        // ==================== SoftKitty Window Management ====================

        private void OpenEquipmentWindow()
        {
            if (_openedWindow != null)
            {
                _openedWindow.gameObject.SetActive(true);
                return;
            }

            // Use the Equipment prefab from SoftKitty Resources
            // _openedWindow = WindowsManager.GetWindow("Equipment", ItemObject.PlayerInventoryData);
            if (_openedWindow == null) return;

            // Reparent the window under the MVP content panel
            var rt = _openedWindow.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.SetParent(_view.contentContainer, false);
                rt.localPosition = Vector3.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(500f, 400f);
            }

            _openedWindow.ActiveWindow();
        }

        private void CloseEquipmentWindow()
        {
            if (_openedWindow != null)
            {
                _openedWindow.Close();
                _openedWindow = null;
            }
        }

        // ==================== Helpers ====================

        private void RefreshEquipSlots()
        {
           
        }

        public override void Initialize()
        {
            base.Initialize();

            // Subscribe to view events
            _view.OnEquipSlotClicked += OnEquipSlotClicked;
            _view.OnDetailCloseClicked += OnDetailCloseClicked;
        }

        public override void Destroy()
        {
            _view.OnEquipSlotClicked -= OnEquipSlotClicked;
            _view.OnDetailCloseClicked -= OnDetailCloseClicked;

            CloseEquipmentWindow();

            base.Destroy();
        }
    }
}
