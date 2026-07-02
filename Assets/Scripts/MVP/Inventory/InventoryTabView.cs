using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;

namespace Domain.MVP.Inventory
{
    /// <summary>
    /// View for the Inventory tab. Manages item grids, category panels, and equip slots.
    /// </summary>
    public class InventoryTabView : TabView
    {
        [Header("Item Grid")]
        [Tooltip("Grid layout for inventory items.")]
        public GridLayoutGroup itemGrid;

        [Tooltip("Prefab for an inventory item slot.")]
        public GameObject itemSlotPrefab;

        [Tooltip("Text showing item count (e.g. '3/50').")]
        public TextMeshProUGUI itemCountText;

        [Header("Equipment Slots")]
        [Tooltip("Container for equipment slot buttons.")]
        public RectTransform equipmentSlotContainer;

        [Tooltip("Prefab for an equipment slot button.")]
        public GameObject equipmentSlotPrefab;

        [Header("Item Detail Panel")]
        [Tooltip("Container for the item detail/info panel.")]
        public RectTransform detailPanel;

        [Tooltip("Item name in the detail panel.")]
        public TextMeshProUGUI detailNameText;

        [Tooltip("Item description in the detail panel.")]
        public TextMeshProUGUI detailDescText;

        [Tooltip("Item icon in the detail panel.")]
        public Image detailIconImage;

        [Tooltip("Equip button in the detail panel.")]
        public Button equipButton;

        [Tooltip("Drop button in the detail panel.")]
        public Button dropButton;

        /// <summary>Fired when an item slot is clicked.</summary>
        public event Action<string> OnItemClick;

        /// <summary>Fired when an equipment slot is clicked.</summary>
        public event Action<string> OnEquipSlotClick;

        /// <summary>Fired when the equip button is pressed.</summary>
        public event Action OnEquipPressed;

        /// <summary>Fired when the drop button is pressed.</summary>
        public event Action OnDropPressed;

        /// <summary>Fired when the search input is submitted.</summary>
        public event Action<string> OnSearchSubmitted;

        private Dictionary<string, GameObject> _itemSlots;
        private Dictionary<string, GameObject> _equipSlots;

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);
            _itemSlots = new Dictionary<string, GameObject>();
            _equipSlots = new Dictionary<string, GameObject>();
        }

        /// <summary>Set the item count text (e.g. '3/50').</summary>
        public void SetItemCountText(int current, int max)
        {
            if (itemCountText != null)
                itemCountText.text = $"{current}/{max}";
        }

        /// <summary>Refresh the item grid for a given category.</summary>
        public void RefreshItemGrid(string categoryId, List<(string id, string name, int count, string icon, bool isEquippable)> items)
        {
            if (itemGrid == null || itemSlotPrefab == null) return;

            // Clear existing slots
            foreach (var kvp in _itemSlots)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _itemSlots.Clear();

            foreach (var item in items)
            {
                var slot = Instantiate(itemSlotPrefab, itemGrid.transform);
                slot.SetActive(true);

                var nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null) nameText.text = item.name;

                var btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    var itemId = item.id;
                    btn.onClick.AddListener(() => OnItemClick?.Invoke(itemId));
                }

                _itemSlots[item.id] = slot;
            }
        }

        /// <summary>Refresh the equipment slot buttons.</summary>
        public void RefreshEquipSlots(List<string> slotIds, Func<string, (string id, string name, string icon)> getEquipped)
        {
            if (equipmentSlotContainer == null || equipmentSlotPrefab == null) return;

            foreach (var kvp in _equipSlots)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _equipSlots.Clear();

            foreach (var slotId in slotIds)
            {
                var slot = Instantiate(equipmentSlotPrefab, equipmentSlotContainer);
                slot.SetActive(true);

                var btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    var sid = slotId;
                    btn.onClick.AddListener(() => OnEquipSlotClick?.Invoke(sid));
                }

                _equipSlots[slotId] = slot;
            }
        }

        /// <summary>Update the detail panel with the selected item's data.</summary>
        public void UpdateDetailPanel(string itemName, string itemDesc, string iconPath)
        {
            if (detailNameText != null) detailNameText.text = itemName;
            if (detailDescText != null) detailDescText.text = itemDesc;
            // TODO: Load icon from iconPath and set detailIconImage.sprite
        }

        /// <summary>Clear the detail panel.</summary>
        public void ClearDetailPanel()
        {
            if (detailNameText != null) detailNameText.text = string.Empty;
            if (detailDescText != null) detailDescText.text = string.Empty;
            if (detailIconImage != null) detailIconImage.sprite = null;
        }

        public override void ShowSubtab(string subtabId)
        {
            base.ShowSubtab(subtabId);

            // Hide detail panel when switching categories
            if (detailPanel != null)
                detailPanel.gameObject.SetActive(false);
        }
    }
}
