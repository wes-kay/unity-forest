using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;

namespace Domain.MVP.Equipment
{
    /// <summary>
    /// View for the Equipment tab. Manages equipment slot display and stats panel.
    /// The actual equipment window is opened via the presenter calling SoftKitty WindowsManager.
    /// </summary>
    public class EquipmentTabView : TabView
    {
        [Header("Equipment Slots")]
        [Tooltip("Container for equipment slot buttons.")]
        public RectTransform equipmentSlotContainer;

        [Tooltip("Prefab for an equipment slot button.")]
        public GameObject equipmentSlotPrefab;

        [Header("Equipment Detail")]
        [Tooltip("Container for the selected equipment detail panel.")]
        public RectTransform detailPanel;

        [Tooltip("Equipment name in detail panel.")]
        public TextMeshProUGUI detailNameText;

        [Tooltip("Equipment description in detail panel.")]
        public TextMeshProUGUI detailDescText;

        [Tooltip("Equipment icon in detail panel.")]
        public Image detailIconImage;

        /// <summary>Fired when an equipment slot is clicked.</summary>
        public event Action<string> OnEquipSlotClicked;

        /// <summary>Fired when the detail panel close button is clicked.</summary>
        public event Action OnDetailCloseClicked;

        private readonly Dictionary<string, GameObject> _equipSlots = new Dictionary<string, GameObject>();

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);
        }

        /// <summary>Refresh the equipment slot display.</summary>
        public void RefreshEquipSlots(string[] slotIds, System.Func<string, (string id, string name, string icon)> getEquipped)
        {
            if (equipmentSlotContainer == null || equipmentSlotPrefab == null) return;

            // Clear existing slots
            foreach (var kvp in _equipSlots)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _equipSlots.Clear();

            foreach (var slotId in slotIds)
            {
                var slot = Instantiate(equipmentSlotPrefab, equipmentSlotContainer);
                slot.SetActive(true);

                var text = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = slotId;

                var btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    var sid = slotId;
                    btn.onClick.AddListener(() => OnEquipSlotClicked?.Invoke(sid));
                }

                _equipSlots[slotId] = slot;
            }
        }

        /// <summary>Update the detail panel with selected equipment data.</summary>
        public void UpdateDetailPanel(string name, string description, string iconPath)
        {
            if (detailNameText != null) detailNameText.text = name;
            if (detailDescText != null) detailDescText.text = description;
            // TODO: Load icon from iconPath and set detailIconImage.sprite
        }

        /// <summary>Clear the detail panel.</summary>
        public void ClearDetailPanel()
        {
            if (detailNameText != null) detailNameText.text = string.Empty;
            if (detailDescText != null) detailDescText.text = string.Empty;
            if (detailIconImage != null) detailIconImage.sprite = null;
        }

        /// <summary>Toggle the detail panel visibility.</summary>
        public void SetDetailPanelActive(bool active)
        {
            if (detailPanel != null)
                detailPanel.gameObject.SetActive(active);
        }
    }
}
