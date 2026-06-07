using UnityEngine;
using System.Collections.Generic;

namespace SoftKitty.InventoryEngine
{

    [System.Serializable]
    public class InventoryModule : EntityModule
    {
        public List<ItemSaveRoot> InventorySave = new List<ItemSaveRoot>();
        public List<InventoryData> Inventory = new List<InventoryData>();
        public bool lootpackFold = false;
        public List<string> LootPacks = new List<string>();
        private Dictionary<InventoryData.HolderType, InventoryData> InventoryDic = new Dictionary<InventoryData.HolderType, InventoryData>();
        [HideInInspector, System.NonSerialized]
        public bool inventoryFold = false;
        [SerializeReference,System.NonSerialized]
        private InventoryData mEquipment = null;
        [SerializeReference, System.NonSerialized]
        private InventoryData mInventory = null;

        public override void RuntimeInit()
        {
            foreach (var inven in Inventory)
            {
                inven.Init();
            }
            InventoryDic.Clear();
            mEquipment = null;
            mInventory = null;
        }
        public override float GetAttributeValue(int _id)
        {
            if (GetEquipment() != null) return GetEquipment().GetAttributeValue(_id);
            return 0F;
        }

        public override float GetAttributeValue(string _uid)
        {
            if (GetEquipment() != null) return GetEquipment().GetAttributeValue(_uid);
            return 0F;
        }

        public override EntityModule Copy()
        {
            InventoryModule copy = new InventoryModule();
            copy.Inventory = new List<InventoryData>();
            for (int i = 0; i < Inventory.Count; i++) {
                copy.Inventory.Add(Inventory[i].Copy());
            }
            copy.LootPacks = new List<string>();
            for (int i = 0; i < LootPacks.Count; i++)
            {
                copy.LootPacks.Add(LootPacks[i]);
            }
            copy.InventorySave = new List<ItemSaveRoot>();
            return copy;
        }

        public override string CompressData()
        {
            InventorySave.Clear();
            for (int i = 0; i < Inventory.Count; i++)
            {
                InventorySave.Add(Inventory[i].GetSaveData());
            }
            Inventory.Clear();
            return JsonUtility.ToJson(this);
        }

        public override string UncompressData(string _compactJson)
        {
            if (!string.IsNullOrEmpty(_compactJson))
            {
                InventoryModule _saveData = (InventoryModule)JsonUtility.FromJson(_compactJson, typeof(InventoryModule));
                Inventory.Clear();
                for (int i = 0; i < _saveData.InventorySave.Count; i++)
                {
                    InventoryData _data = new InventoryData();
                    _data.LoadSaveData(_saveData.InventorySave[i]);
                    Inventory.Add(_data);
                }
            }
            InventorySave.Clear();
            return JsonUtility.ToJson(this);
        }

        public override string ToJson()
        {
            InventorySave.Clear();
            return JsonUtility.ToJson(this);
        }

        public override EntityModule FromJson(string _json)
        {
            if (!string.IsNullOrEmpty(_json))
            {
                return (InventoryModule)JsonUtility.FromJson(_json, typeof(InventoryModule));
            }

            return new InventoryModule();
        }

        /// <summary>
        /// Retrieve any exitsing [InventoryData] of this entity when available.
        /// </summary>
        /// <returns></returns>
        public InventoryData GetAnyInventoryData()
        {
            if (Inventory.Count > 0) return Inventory[0];
            return null;
        }

        /// <summary>
        /// Retrieve any exitsing Equipment type [InventoryData] of this entity when available.
        /// </summary>
        /// <returns></returns>
        public InventoryData GetEquipment()
        {
            if (SGD_Settings.isRuntime)
            {
                if (mEquipment != null) return mEquipment;
            }
            foreach (var obj in Inventory)
            {
                if (obj.Type == InventoryData.HolderType.PlayerEquipment || obj.Type == InventoryData.HolderType.NpcEquipment)
                {
                    if (SGD_Settings.isRuntime) mEquipment = obj;
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieve any exitsing Inventory|Crate type [InventoryData] of this entity when available.
        /// </summary>
        /// <returns></returns>
        public InventoryData GetInventory()
        {
            if (SGD_Settings.isRuntime)
            {
                if (mInventory != null) return mInventory;
            }
            foreach (var obj in Inventory)
            {
                if (obj.Type == InventoryData.HolderType.Crate || obj.Type == InventoryData.HolderType.Merchant || obj.Type == InventoryData.HolderType.PlayerInventory || obj.Type == InventoryData.HolderType.NpcInventory)
                {
                    if (SGD_Settings.isRuntime) mInventory = obj;
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieve specified type [InventoryData] of this entity when available.
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public InventoryData GetInventoryDataByType(InventoryData.HolderType _type)
        {
            InventoryData _holder = null;
            if (InventoryDic.TryGetValue(_type, out _holder))
            {
                return _holder;
            }
            InventoryDic.Clear();
            foreach (var obj in Inventory)
            {
                if (!InventoryDic.ContainsKey(obj.Type)) InventoryDic.Add(obj.Type, obj);
                if (obj.Type == _type)
                {
                    _holder = obj;
                }
            }
            return _holder;
        }

        /// <summary>
        /// Drop a Loot Pack this entity carries, pass an index number for drop a specified pack from the list, otherwise it will random pick one.
        /// </summary>
        /// <param name="_index"></param>
        public LootPack DropLootPack( int _index = -1)
        {
            if (LootPacks.Count <= 0 || _index >= LootPacks.Count) return null;
            if (_index == -1)
            {
                return ItemObject.DropLootPack(entity.Position, LootPacks[Random.Range(0, LootPacks.Count)]);
            }
            else
            {
                return ItemObject.DropLootPack(entity.Position, LootPacks[_index]);
            }
        }
    }
}
