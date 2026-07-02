using System.Collections.Generic;
using Domain.MVP.Tab;

namespace Domain.MVP.Inventory
{
    /// <summary>
    /// Model for the Inventory tab. Manages inventory categories and item data.
    /// </summary>
    public class InventoryTabModel : TabModel
    {
        /// <summary>All inventory category IDs.</summary>
        public string[] CategoryIds { get; }

        /// <summary>Currently selected category ID.</summary>
        public string SelectedCategoryId { get; private set; }

        /// <summary>Category display names.</summary>
        public Dictionary<string, string> CategoryNames { get; } = new Dictionary<string, string>();

        /// <summary>Fired when category is changed.</summary>
        public event System.Action<string> OnCategoryChanged;

        /// <summary>Fired when item data refreshes.</summary>
        public event System.Action OnItemDataChanged;

        /// <summary>Get items in a category. Returns (itemId, name, count, icon, isEquippable).</summary>
        public List<(string id, string name, int count, string icon, bool isEquippable)> GetCategoryItems(string categoryId)
        {
            // TODO: Query InventoryEngine for items in this category
            return new List<(string, string, int, string, bool)>();
        }

        /// <summary>Get the equipped item for a slot.</summary>
        public (string id, string name, string icon) GetEquippedItem(string slot)
        {
            // TODO: Query equipped items
            return (null, null, null);
        }

        /// <summary>Get total item count across all categories.</summary>
        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var catId in CategoryIds)
            {
                total += GetCategoryItems(catId).Count;
            }
            return total;
        }

        public InventoryTabModel()
            : base("inventory", "Inventory", new[] { "equipment", "backpack", "dropbox" })
        {
            CategoryIds = new[] { "equipment", "backpack", "dropbox" };
            CategoryNames["equipment"] = "Equipment";
            CategoryNames["backpack"] = "Backpack";
            CategoryNames["dropbox"] = "Dropbox";
            SelectedCategoryId = CategoryIds[0];
        }

        public override void LoadFromService()
        {
            // TODO: Load inventory categories and items from InventoryEngine
            // SelectedCategoryId = "backpack"; // Default to backpack
        }

        public void SelectCategory(string categoryId)
        {
            if (System.Array.IndexOf(CategoryIds, categoryId) < 0) return;
            SelectedCategoryId = categoryId;
            OnCategoryChanged?.Invoke(categoryId);
        }
    }
}
