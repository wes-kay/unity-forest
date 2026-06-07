using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace SoftKitty.InventoryEngine
{
    /// <summary>
    /// The <InventoryHolder> component is deprecated. Inventory data is now managed in:
    /// "Project Settings ¡ú SoftKitty ¡ú Entity Manager"
    /// This class is for transfer data from Lite version to Pro version only.
    /// </summary>
    public class InventoryHolder : MonoBehaviour
    {
        public string uid = "";
        public string Name = "Inventory";
       
        public enum HolderType
        {
            Crate,
            Merchant,
            PlayerInventory,
            PlayerEquipment,
            NpcInventory,
            NpcEquipment
        }
        public HolderType Type;
        public List<InventoryStack> Stacks = new List<InventoryStack>();
        public List<InventoryStack> HiddenStacks = new List<InventoryStack>();
        public CurrencySet Currency = new CurrencySet();
        public int InventorySize = 10;
        public float MaxiumCarryWeight = 1000F;
        public float SellPriceMultiplier = 1F;
        public float BuyPriceMultiplier = 1F;
        public List<Vector3> SpecificPriceMultiplier = new List<Vector3>();
        public bool TradeAllItems = true;
        public List<int> TradeList = new List<int>();
        public List<int> TradeCategoryList = new List<int>();
        public List<Attribute> BaseStats = new List<Attribute>();



        
    }
}
