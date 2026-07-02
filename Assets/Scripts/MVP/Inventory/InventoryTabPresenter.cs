using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Domain.MVP.Tab;

namespace Domain.MVP.Inventory
{
    /// <summary>
    /// Presenter for the Inventory tab. Handles category switching, item selection, and equip/drop operations.
    /// </summary>
    public class InventoryTabPresenter : TabPresenter<InventoryTabModel, InventoryTabView>
    {
        [Inject] private InventoryTabModel _inventoryModel;
        [Inject] private InventoryTabView _inventoryView;

        /// <summary>Currently selected item ID (null = none).</summary>
        private string _selectedItemId;

        public override void OnTabActivated()
        {
            if (!_inventoryModel.IsLoaded)
            {
                _inventoryModel.LoadFromService();
            }

            // Sync item grid for the active category
            RefreshItemGrid();

            // Sync equipment slots
            RefreshEquipSlots();

            // Show the correct subtab
            _inventoryView.ShowSubtab(_inventoryModel.ActiveSubtab);

            // Update item count text
            _inventoryView.SetItemCountText(
                _inventoryModel.GetTotalItemCount(),
                50 // TODO: Get max capacity from InventoryEngine
            );
        }

        public override void OnTabDeactivated()
        {
            // No cleanup needed — data stays cached
        }

        public override void OnSubtabChanged(string subtabId)
        {
            // Category changed — refresh the item grid
            RefreshItemGrid();

            // Hide detail panel when switching categories
            _inventoryView.ClearDetailPanel();
        }

        /// <summary>Handle item slot click from the view.</summary>
        public void OnItemClick(string itemId)
        {
            _selectedItemId = itemId;

            // Get item data from model
            var items = _inventoryModel.GetCategoryItems(_inventoryModel.SelectedCategoryId);
            foreach (var item in items)
            {
                if (item.id == itemId)
                {
                    _inventoryView.UpdateDetailPanel(item.name, item.name, item.icon);
                    _inventoryView.detailPanel.gameObject.SetActive(true);
                    return;
                }
            }
        }

        /// <summary>Handle equipment slot click from the view.</summary>
        public void OnEquipSlotClick(string slotId)
        {
            var equipped = _inventoryModel.GetEquippedItem(slotId);
            if (equipped.id != null)
            {
                _selectedItemId = equipped.id;
                _inventoryView.UpdateDetailPanel(equipped.name, equipped.name, equipped.icon);
                _inventoryView.detailPanel.gameObject.SetActive(true);
            }
        }

        /// <summary>Handle equip button press.</summary>
        public void OnEquipPressed()
        {
            if (_selectedItemId == null) return;

            // TODO: Call InventoryEngine to equip the item
            // InventoryEngine.EquipItem(_selectedItemId);

            // Refresh grids after equip
            RefreshItemGrid();
            RefreshEquipSlots();
        }

        /// <summary>Handle drop button press.</summary>
        public void OnDropPressed()
        {
            if (_selectedItemId == null) return;

            // TODO: Call InventoryEngine to drop the item
            // InventoryEngine.DropItem(_selectedItemId);

            _selectedItemId = null;
            _inventoryView.ClearDetailPanel();

            // Refresh grids after drop
            RefreshItemGrid();
            RefreshEquipSlots();
        }

        /// <summary>Handle search input submission.</summary>
        public void OnSearchSubmitted(string query)
        {
            // TODO: Filter items by search query
            // var filtered = _inventoryModel.SearchItems(query);
            // _inventoryView.RefreshItemGrid(_inventoryModel.SelectedCategoryId, filtered);
        }

        // ==================== Helpers ====================

        private void RefreshItemGrid()
        {
            var items = _inventoryModel.GetCategoryItems(_inventoryModel.SelectedCategoryId);
            _inventoryView.RefreshItemGrid(_inventoryModel.SelectedCategoryId, items);
        }

        private void RefreshEquipSlots()
        {
            var slotIds = new List<string> { "weapon", "armor", "helmet", "accessory" }; // TODO: Get from config
            _inventoryView.RefreshEquipSlots(slotIds, slotId => _inventoryModel.GetEquippedItem(slotId));
        }

        public override void Initialize()
        {
            base.Initialize();

            // Subscribe to view events
            _inventoryView.OnItemClick += OnItemClick;
            _inventoryView.OnEquipSlotClick += OnEquipSlotClick;
            _inventoryView.OnEquipPressed += OnEquipPressed;
            _inventoryView.OnDropPressed += OnDropPressed;
            _inventoryView.OnSearchSubmitted += OnSearchSubmitted;

            // Subscribe to model events
            _inventoryModel.OnCategoryChanged += OnModelCategoryChanged;
            _inventoryModel.OnItemDataChanged += OnModelItemDataChanged;
        }

        private void OnModelCategoryChanged(string categoryId)
        {
            if (_inventoryView != null)
            {
                _inventoryView.ShowSubtab(categoryId);
            }
        }

        private void OnModelItemDataChanged()
        {
            // Refresh the grid when model data changes externally
            RefreshItemGrid();
            _inventoryView.SetItemCountText(
                _inventoryModel.GetTotalItemCount(),
                50 // TODO: Get max capacity from InventoryEngine
            );
        }

        public override void Destroy()
        {
            _inventoryView.OnItemClick -= OnItemClick;
            _inventoryView.OnEquipSlotClick -= OnEquipSlotClick;
            _inventoryView.OnEquipPressed -= OnEquipPressed;
            _inventoryView.OnDropPressed -= OnDropPressed;
            _inventoryView.OnSearchSubmitted -= OnSearchSubmitted;

            _inventoryModel.OnCategoryChanged -= OnModelCategoryChanged;
            _inventoryModel.OnItemDataChanged -= OnModelItemDataChanged;

            base.Destroy();
        }
    }
}
