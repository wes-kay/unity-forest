using System;
using Domain.MVP.Tab;

namespace Domain.MVP.Equipment
{
    /// <summary>
    /// Model for the Equipment tab. Manages equipment slots and stats state.
    /// </summary>
    public class EquipmentTabModel : TabModel
    {
        /// <summary>Currently selected equipment slot UID (empty = none).</summary>
        public string SelectedSlotId { get; private set; }

        /// <summary>Fired when an equipment slot is selected.</summary>
        public event Action<string> OnSlotSelected;

        /// <summary>Fired when equipment data refreshes.</summary>
        public event Action OnEquipmentDataChanged;

        public EquipmentTabModel()
            : base("equipment", "Equipment", new[] { "gear", "stats" })
        {
        }

        public override void LoadFromService()
        {
            // TODO: Load equipment data from InventoryEngine
        }

        public void SelectSlot(string slotId)
        {
            SelectedSlotId = slotId;
            OnSlotSelected?.Invoke(slotId);
        }

        public void ClearSelection()
        {
            SelectedSlotId = string.Empty;
        }

        /// <summary>Get equipped items for a slot. TODO: implement with inventory data.</summary>
        public (string id, string name, string icon) GetEquippedItem(string slotId)
        {
            // TODO: Query InventoryEngine for equipped item
            return (null, null, null);
        }
    }
}
