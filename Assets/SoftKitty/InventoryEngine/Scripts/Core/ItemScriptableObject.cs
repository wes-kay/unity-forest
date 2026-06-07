using UnityEngine;

namespace SoftKitty.InventoryEngine
{
    [CreateAssetMenu(fileName = "ItemScriptableObject", menuName = "Soft Kitty/Inventory Engine/Item Scriptable Object")]
    public class ItemScriptableObject : ScriptableObject
    {
        public Item mItem;
        
    }
}
