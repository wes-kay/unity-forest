using UnityEngine;
using System.Collections.Generic;
using SoftKitty.InventoryEngine;
using System.IO;
using System.Collections.Concurrent;

namespace SoftKitty
{
    /// <summary>
    /// Callback function when an item is being used.
    /// </summary>
    /// <param name="_action"></param>
    /// <param name="_id"></param>
    /// <param name="_index"></param>
    public delegate void ItemUseCallback(string _action, int _id, int _index);
    /// <summary>
    /// Call back function when Items in this InventoryData have been changed.
    /// </summary>
    /// <param name="_changedItems"></param>
    public delegate void ItemChangeCallback(Dictionary<Item, int> _changedItems);
    /// <summary>
    /// Call back function when Item in this InventoryData has been dropped by dragging out the window.
    /// </summary>
    /// <param name="_droppedItem"></param>
    /// <param name="_number"></param>
    public delegate void ItemDropCallback(Item _droppedItem, int _number);

    [CreateAssetMenu(fileName = "ItemObject", menuName = "Soft Kitty/Data Objects/Item Data")]
    public class ItemObject : DataObject
    {
        #region DataObject Variables

        public override string DataName() { return "Item Data"; }
        public override string TypeString() { return "SoftKitty.ItemObject"; }
        public StringIdManager IdManager = new StringIdManager();
        
        private static bool runtimeInited = false;
        private Item nullPlaceHolder = null;

        #endregion

        #region Editor
        public string SearchText = "";
        public int SearchType = 0;
        public bool _skinExpand = false;
        public bool _generalExpand = false;
        public bool _typeExpand = false;
        public bool _qualityExpand = false;
        public bool _currencyExpand = false;
        public bool _attExpand = false;
        public bool _attViewExpand = false;
        public bool _entExpand = false;
        public bool _craftExpand = false;
        public bool _upgradeExpand = false;
        public bool _itemExpand = false;
        public bool _socketingExpand = false;
        public bool _lootPackExpand = false;
        public int _newAttSel = 0;
#if MASTER_CHARACTER_CREATOR
        MasterCharacterCreator.CharacterDataSetting MccData;
#endif
        #endregion

        #region DataObject Methods

        public int IndexOfItems(string _uid)
        {
            if (DataMode == ItemDataMode.Unified)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (_uid == items[i].uid) return i;
                }
            }
            else
            {
                for (int i = 0; i < itemScriptableObjects.Count; i++)
                {
                    if (_uid == itemScriptableObjects[i].mItem.uid) return i;
                }
            }

            return -1;
        }

        public int IndexOfItems(int _id)
        {
            if (DataMode == ItemDataMode.Unified)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (_id == items[i].id) return i;
                }
            }
            else
            {
                for (int i = 0; i < itemScriptableObjects.Count; i++)
                {
                    if (_id == itemScriptableObjects[i].mItem.id) return i;
                }
            }
            return -1;
        }

        public override string GetDataJson()
        {
            string json = "";
            if (DataMode == ItemDataMode.Unified)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Item _temp = items[i].Copy();
                    _temp.fold = false;
                    json += JsonUtility.ToJson(_temp);
                }
            }
            else
            {
                for (int i = 0; i < itemScriptableObjects.Count; i++)
                {
                    Item _temp = itemScriptableObjects[i].mItem.Copy();
                    _temp.fold = false;
                    json += JsonUtility.ToJson(_temp);
                }
            }
            for (int i = 0; i < itemTypes.Count; i++)
            {
                json += JsonUtility.ToJson(itemTypes[i]);
            }
            for (int i = 0; i < itemQuality.Count; i++)
            {
                json += JsonUtility.ToJson(itemQuality[i]);
            }
            for (int i = 0; i < currencies.Count; i++)
            {
                json += JsonUtility.ToJson(currencies[i]);
            }
            for (int i = 0; i < itemEnchantments.Count; i++)
            {
                Enchantment _temp = itemEnchantments[i].Copy();
                _temp.fold = false;
                json += JsonUtility.ToJson(_temp);
            }
            return json;
        }

        public override int GetDataCount()
        {
            return (DataMode == ItemDataMode.Unified? items.Count:itemScriptableObjects.Count)+ itemTypes.Count+ itemQuality.Count+ currencies.Count+ itemEnchantments.Count;
        }
        #endregion


        #region Variables

        /// <summary>
        /// List of loot packs.
        /// </summary>
        public List<LootPackData> lootPacks = new List<LootPackData>();

        /// <summary>
        /// List of item category.
        /// </summary>
        public List<StringColorData> itemTypes = new List<StringColorData>();
        /// <summary>
        /// List of item quality levels.
        /// </summary>
        public List<StringColorData> itemQuality = new List<StringColorData>();
        /// <summary>
        /// Access the item data by its UID.  Use itemDic[_uid].Copy() to get an instance of the item data with specified UID.
        /// </summary>
        private ConcurrentDictionary<int, Item> ItemDic = new ConcurrentDictionary<int, Item>();
        public List<ClickSetting> clickSettings = new List<ClickSetting>();
        public string itemAttributeObjectHash = "";
        /// <summary>
        /// List of Enchantments.
        /// </summary>
        public List<Enchantment> itemEnchantments = new List<Enchantment>();
        /// <summary>
        /// Access the [Item Enchantment] data by the id.
        /// </summary>
        public ConcurrentDictionary<int, Enchantment> enchantmentDic = new ConcurrentDictionary<int, Enchantment>();
        /// <summary>
        /// Access the [Loot Pack] data by the uid.
        /// </summary>
        public ConcurrentDictionary<string, LootPackData> lootPackDic = new ConcurrentDictionary<string, LootPackData>();
        public List<Currency> currencies = new List<Currency>();
        public List<Item> items = new List<Item>();
        public List<ItemScriptableObject> itemScriptableObjects = new List<ItemScriptableObject>();
        public ItemDataMode DataMode = ItemDataMode.Unified;



        public string NameAttributeKey = "name";
        public string LevelAttributeKey = "lvl";
        public string XpAttributeKey = "xp";
        public string MaxXpAttributeKey = "mxp";
        public string CoolDownAttributeKey = "cd";
        public float SharedGlobalCoolDown = 0.5F;
        public int SearchFilterMode = 0;
        public bool MergeStacksOnSort = true;

        public Color AttributeNameColor = new Color(0.17F, 0.53F, 0.82F, 1F);
        public bool UseQualityColorForItemName = true;

        public int MerchantStyle = 0;
        public bool HighlightEquipmentSlotWhenHoverItem = true;
        public bool AllowDropItem = true;
        public string CanvasTag = "";

        public bool EnableCrafting = true;
        public int CraftingMaterialCategoryID = 0;
        public string CraftingBlueprintTag = "Blueprint";
        public string PlayerName = "Player";
        public float CraftingTime = 0.5F;

        public Vector2[] EnhancingMaterials = new Vector2[2];
        public int EnhancingCurrencyType = 0;
        public int EnhancingCurrencyNeed = 0;
        public bool EnableEnhancing = true;
        public bool DestroyEquipmentWhenFail = false;
        public int DestroyEquipmentWhenFailLevel = 3;
        public int MaxiumEnhancingLevel = 10;
        public AnimationCurve EnhancingSuccessCurve;
        public int EnhancingCategoryID = 0;
        public float EnhancingTime = 0.5F;
        public bool EnableEnhancingGlow = true;
        public AnimationCurve EnhancingGlowCurve;

        public Vector2 EnchantingMaterial = new Vector2(1, 1);
        public int EnchantingCurrencyType = 0;
        public int EnchantingCurrencyNeed = 0;
        public Vector2 EnchantmentNumberRange = new Vector2(1, 3);
        public bool EnableEnchanting = true;
        public bool RandomEnchantmentsForNewItem = false;
        public int EnchantingSuccessRate = 30;
        public int EnchantingCategoryID = 0;
        public float EnchantingTime = 0.5F;
        public Color EnchantingPrefixesColor = new Color(0.32F, 0.55F, 0.18F, 1F);
        public Color EnchantingSuffxesColor = new Color(0.32F, 0.55F, 0.18F, 1F);
        public Color EnchantingNameColor = new Color(0.32F, 0.55F, 0.18F, 1F);

        public bool EnableSocketing = true;
        public string SocketingTagFilter = "";
        public int SocketingCategoryFilter = 0;
        public int SocketedCategoryFilter = 0;
        public bool EnableRemoveSocketing = true;
        public int RemoveSocketingPrice = 100;
        public int RemoveSocketingCurrency = 0;
        public bool DestroySocketItemWhenRemove = false;
        public bool RandomSocketingSlotsNummber = true;
        public int MinimalSocketingSlotsNumber = 0;
        public int MaxmiumSocketingSlotsNumber = 3;
        public bool LockSocketingSlotsByDefault = true;
        public int RandomChanceToLockSocketingSlots = 25;
        public int UnlockSocketingSlotsPrice = 50;
        public int UnlockSocketingSlotsCurrency = 0;

        public string PlayerInventoryWindowName = "Inventory";
        public string PlayerEquipWindowName = "Equipment";
        public string MerchantWindowName = "Merchant";
        public string NpcInventoryWindowName = "NpcInventory";
        public string NpcEquipmentWindowName = "Equipment";
        public string StorageWindowName = "Storage";
        public string ForgeWindowName = "Forge";

        public string msgBagFull = "Sorry, the bag is full.";
        public string msgItemUseRestricted = "Sorry, you can not use this item because of your {name} is less than {value}.";
        public string msgItemAssign = "Sorry, you can not assign this item here.";
        public string msgEnhancingFail = "Failed! [ <color=#FFA209>{name}</color> ] break into pieces.";

        public int UiStyle = 1;
        public float UiScale = 1F;
        public float InventorySlotScale = 1F;
        public Color EmptyItemBackColor = new Color(0.33F, 0.33F, 0.33F, 1F);
        public Color ItemSelectedColor = new Color(1F, 0.28F, 0F, 0.4F);
        public Color ItemHoverColor = new Color(1F, 0.54F, 0F, 0.1F);
        public Color FavoriteColor = new Color(1F, 0.54F, 0F, 1F);

        /// <summary>
        /// Retrieve the instance of the ItemObject instance assigned in SoftKitty Data Settings.
        /// </summary>
        public static ItemObject instance
        {
            get
            {
                return SGD_Settings.Instance.GetData<ItemObject>();
            }
        }

        [SerializeReference]
        private static InventoryEngine.InventoryData _playerInventoryData = null;
        [SerializeReference]
        private static InventoryEngine.InventoryData _playerEquipmentHolder = null;
        /// <summary>
        /// Retrieve the InventoryData class for player's invetory.
        /// </summary>
        /// <returns></returns>
        public static InventoryEngine.InventoryData PlayerInventoryData
        {
            get
            {
                if (_playerInventoryData != null) return _playerInventoryData;
                if (GameManager.GetPlayer() != null)
                {
                    _playerInventoryData = GameManager.GetPlayer().GetModule<InventoryModule>().GetInventoryDataByType(InventoryEngine.InventoryData.HolderType.PlayerInventory);
                    return _playerInventoryData;
                }
                return null;
            }
        }

        /// <summary>
        /// Retrieve the InventoryData class for player's equipments.
        /// </summary>
        /// <returns></returns>
        public static InventoryEngine.InventoryData PlayerEquipmentData
        {
            get
            {
                if (_playerEquipmentHolder != null) return _playerEquipmentHolder;
                if (GameManager.GetPlayer() != null)
                {
                    _playerEquipmentHolder = GameManager.GetPlayer().GetModule<InventoryModule>().GetInventoryDataByType(InventoryEngine.InventoryData.HolderType.PlayerEquipment);
                    return _playerEquipmentHolder;
                }
                return null;
            }
        }

        /// <summary>
        /// Drop a Loot Pack at the providing position via the unique string id.
        /// </summary>
        /// <param name="_uid"></param>
        public static InventoryEngine.LootPack DropLootPack(Vector3 _pos, string _uid)
        {
            InventoryEngine.LootPackData _data = instance.TryGetLootPackByUid(_uid);
            if (_data != null)
            {
                GameObject _newPack = new GameObject("LootPack [" + _uid + "]");
                _newPack.transform.position = _pos;
                InventoryEngine.LootPack _loot = _newPack.AddComponent<InventoryEngine.LootPack>();
                _loot.Init(_data);
                return _loot;
            }
            return null;
        }

        /// <summary>
        /// An array of Item categories. This provides a simple list of the Item categories for quick reference.
        /// </summary>
        public string[] ItemCategoryNames
        {
            get
            {
                string[] _names = new string[itemTypes.Count];
                for (int i = 0; i < _names.Length; i++)
                {
                    _names[i] = itemTypes[i].name;
                }
                return _names;
            }
        }

        /// <summary>
        /// An array of Item qualities. This provides a simple list of the Item qualities for quick reference.
        /// </summary>
        public string[] ItemQualityNames
        {
            get
            {
                string[] _names = new string[itemQuality.Count];
                for (int i = 0; i < _names.Length; i++)
                {
                    _names[i] = itemQuality[i].name;
                }
                return _names;
            }
        }

        /// <summary>
        /// An array of Item names. This provides a simple list of the Item names for quick reference.
        /// </summary>
        public string[] ItemNames
        {
            get
            {
                if (DataMode == ItemDataMode.Unified)
                {
                    string[] _names = new string[items.Count];
                    for (int i = 0; i < _names.Length; i++)
                    {
                        _names[i] = items[i].name;
                    }
                    return _names;
                }
                else
                {
                    string[] _names = new string[itemScriptableObjects.Count];
                    for (int i = 0; i < _names.Length; i++)
                    {
                        _names[i] = itemScriptableObjects[i].mItem.name;
                    }
                    return _names;
                }
            }
        }

        /// <summary>
        /// An array of Item UIDs. This contains the unique identifiers for each Item, allowing for direct reference by UID.
        /// </summary>
        public string[] ItemUidArray
        {
            get
            {
                if (DataMode == ItemDataMode.Unified)
                {
                    string[] _names = new string[items.Count];
                    for (int i = 0; i < _names.Length; i++)
                    {
                        _names[i] = items[i].uid;
                    }
                    return _names;
                }
                else
                {
                    string[] _names = new string[itemScriptableObjects.Count];
                    for (int i = 0; i < _names.Length; i++)
                    {
                        _names[i] = itemScriptableObjects[i].mItem.uid;
                    }
                    return _names;
                }
            }
        }

        /// <summary>
        /// A List of Item UIDs. This contains the unique identifiers for each Item, allowing for direct reference by UID.
        /// </summary>
        public List<string> ItemUidList
        {
            get
            {
                List<string> _names = new List<string>();
                if (DataMode == ItemDataMode.Unified)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        _names.Add(items[i].uid);
                    }
                }
                else
                {
                    for (int i = 0; i < itemScriptableObjects.Count; i++)
                    {
                        _names.Add(itemScriptableObjects[i].mItem.uid);
                    }
                }
                return _names;
            }
        }

        /// <summary>
        /// A List of Item IDs. This contains the unique identifiers for each Item, allowing for direct reference by ID.
        /// </summary>
        public List<int> ItemIdList
        {
            get
            {
                List<int> _names = new List<int>();
                if (DataMode == ItemDataMode.Unified)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        _names.Add(items[i].id);
                    }
                }
                else
                {
                    for (int i = 0; i < itemScriptableObjects.Count; i++)
                    {
                        _names.Add(itemScriptableObjects[i].mItem.id);
                    }
                }
                return _names;
            }
        }
        #endregion




        #region Internal Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            runtimeInited = false;
            if(instance!=null) instance.Init();
        }

        public static void Refresh()
        {
            _playerEquipmentHolder = null;
            _playerInventoryData = null;
        }

        public void Init()
        {
            ItemDic.Clear();
            enchantmentDic.Clear();
            lootPackDic.Clear();
            if (DataMode == ItemDataMode.Unified)
            {
                foreach (var obj in items)
                {
                    ItemDic.TryAdd(obj.id, obj.Copy());
                }
            }
            else
            {
                foreach (var obj in itemScriptableObjects)
                {
                    ItemDic.TryAdd(obj.mItem.id, obj.mItem.Copy());
                }
            }
            
            for (int i = 0; i < itemEnchantments.Count; i++)
            {
                enchantmentDic.TryAdd(itemEnchantments[i].uid, itemEnchantments[i]);
            }
            for (int i = 0; i < lootPacks.Count; i++)
            {
                lootPackDic.TryAdd(lootPacks[i].uid, lootPacks[i]);
            }
            Refresh();
            GameManager.RefreshCallback -= Refresh;
            GameManager.RefreshCallback += Refresh;
            runtimeInited = true;
            SetCoolDownForAll(0F);
        }

        public void UpdatePrefab()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        #endregion


        #region Methods

        /// <summary>
        /// Try get the [Item Catogory] data by the id.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public StringColorData TryGetItemTypesById(int _id)
        {
            if (instance == null) return null;
            if (_id < 0 || _id >= itemTypes.Count) return null;
            return itemTypes[_id];
        }

        /// <summary>
        /// Try get the [Item Quality] data by the id.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public StringColorData TryGetItemQualityById(int _id)
        {
            if (instance == null) return null;
            if (_id < 0 || _id >= itemQuality.Count) return null;
            return itemQuality[_id];
        }

        /// <summary>
        /// Try get the [Enchantment] data by the id.(thread-safe)
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Enchantment TryGetEnchantmentById(int _id)
        {
            if (SGD_Settings.isRuntime)
            {
                if (instance == null) return null;
                Enchantment _enchantment;
                if (enchantmentDic.TryGetValue(_id, out _enchantment))
                {
                    return _enchantment;
                }
            }
            else
            {
                for (int i = 0; i < itemEnchantments.Count; i++)
                {
                    if (itemEnchantments[i].uid == _id) return itemEnchantments[i];
                }
            }
            return null;
        }


        /// <summary>
        /// Try get the [Enchantment] data by the id.(thread-safe)
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public LootPackData TryGetLootPackByUid(string _uid)
        {
            if (SGD_Settings.isRuntime)
            {
                if (instance == null) return null;
                LootPackData _lootPack;
                if (lootPackDic.TryGetValue(_uid, out _lootPack))
                {
                    return _lootPack;
                }
            }
            else
            {
                for (int i = 0; i < lootPacks.Count; i++)
                {
                    if (lootPacks[i].uid == _uid) return lootPacks[i];
                }
            }
            return null;
        }

        /// <summary>
        /// The UID list of the loot packs.
        /// </summary>
        public List<string> LootPackUids
        {
            get
            {
                List<string> _list = new List<string>();
                for (int i = 0; i < lootPacks.Count; i++) _list.Add(lootPacks[i].uid);
                return _list;
            }
        }


        private readonly object _coolDownModificationLock = new object();
        /// <summary>
        /// Set cool down time for all items, this can be used for global shared cool down timer.
        /// </summary>
        /// <param name="_coolDownTime"></param>
        /// <param name="_onlyUseableItem"></param>
        public void SetCoolDownForAll(float _coolDownTime, bool _onlyUseableItem = true)
        {
            lock (_coolDownModificationLock)
            {
                foreach (var key in ItemDic.Keys)
                {
                    if (ItemDic[key].useable || !_onlyUseableItem) ItemDic[key].SetRemainCoolDownTime(_coolDownTime);
                }
            }
        }

        /// <summary>
        /// Add cool down time for all items, this can be used for global shared cool down timer.
        /// </summary>
        /// <param name="_addValue"></param>
        /// <param name="_onlyUseableItem"></param>
        public void AddCoolDownForAll(float _addValue, bool _onlyUseableItem = true)
        {
            lock (_coolDownModificationLock)
            {
                foreach (var key in ItemDic.Keys)
                {
                    if (ItemDic[key].useable || !_onlyUseableItem) ItemDic[key].AddRemainCoolDownTime(_addValue);
                }
            }
        }

        /// <summary>
        /// Quick method to set global shared cool down timer.
        /// </summary>
        /// <param name="_coolDownTime"></param>
        /// <param name="_onlyUseableItem"></param>
        public void SetSharedGlobalCoolDown(float _coolDownTime, bool _onlyUseableItem = true)
        {
            lock (_coolDownModificationLock)
            {
                foreach (var key in ItemDic.Keys)
                {
                    if (ItemDic[key].GetRemainCoolDownTime() < _coolDownTime && (ItemDic[key].useable || !_onlyUseableItem)) ItemDic[key].SetRemainCoolDownTime(_coolDownTime);
                }
            }
        }




        /// <summary>
        /// ( deprecated! Use GetItem(_id) instead )Try get the item data by uid. (thread-safe)
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Item TryGetItem(int _id)
        {
            return GetItem(_id);
        }


        /// <summary>
        /// Get the item data by index. (Editor only!)
        /// </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public Item GetItemByIndex(int _index)
        {
            if (DataMode == ItemDataMode.Unified )
            {
                if (_index < items.Count && _index >= 0)
                {
                    return items[_index];
                }
                if (items.Count > 0) return items[0];
            }
            else
            {
                if (_index < itemScriptableObjects.Count && _index >= 0)
                {
                    return itemScriptableObjects[_index].mItem;
                }
                if (itemScriptableObjects.Count > 0) return itemScriptableObjects[0].mItem;
            }
            if (nullPlaceHolder == null) nullPlaceHolder = new Item() { name = "Invalid{" + _index + "}", uid = "Invalid" };
            return nullPlaceHolder;
        }

        /// <summary>
        /// Get the item count.
        /// </summary>
        /// <returns></returns>
        public int ItemCount
        {
            get
            {
                if (SGD_Settings.isRuntime)
                {
                    return ItemDic.Count;
                }
                else
                {
                    return (DataMode == ItemDataMode.Unified ? items.Count : itemScriptableObjects.Count);
                }
            }
        }

        /// <summary>
        /// ( Deprecated! use GameManager.GetAttribute(_uid) instead ) Get the Attribute setting by its uid.
        /// </summary>
        /// <param name="_uid"></param>
        /// <returns></returns>
        public Attribute GetAtttibute(string _uid)
        {
            return GameManager.GetAttribute(_uid);
        }



        /// <summary>
        /// Export the item data into a json txt file.
        /// </summary>
        /// <param name="_path"></param>
        public void ExportItemDataToJson(string _path)
        {
            ItemJsonData _root = new ItemJsonData();
            _root.items = new Item[(DataMode == ItemDataMode.Unified? items.Count: itemScriptableObjects.Count)];
            for (int i = 0; i < _root.items.Length; i++)
            {
                Item _newItem=nullPlaceHolder;
                if (DataMode == ItemDataMode.Unified)
                {
                    _newItem = items[i].Copy();
                }
                else
                {
                    _newItem = itemScriptableObjects[i].mItem.Copy();
                }
#if UNITY_EDITOR
                if (_newItem.iconLoadMethod == LoadMethod.DirectReference && _newItem.icon != null)
                {
                    _newItem.iconPath = UnityEditor.AssetDatabase.GetAssetPath(_newItem.icon);
                }
                foreach (var obj in _newItem.customData)
                {
                    if (obj.loadMethod == LoadMethod.DirectReference && obj.value != null)
                    {
                        obj.loadPath = UnityEditor.AssetDatabase.GetAssetPath(obj.value);
                    }
                }
#endif
                _newItem.fold = false;
                _root.items[i] = _newItem;
            }
            string _json = JsonUtility.ToJson(_root);
            File.WriteAllText(_path, _json, System.Text.Encoding.UTF8);
        }


        /// <summary>
        /// Import item data from a json txt file. 
        /// You can store the json file in your game install folder and let modder to modify it, then import it back when game launch.
        /// </summary>
        /// <param name="_path"></param>
        public void ImportItemDataFromJson(string _path)
        {
            if (!File.Exists(_path)) return;
            string _json = File.ReadAllText(_path, System.Text.Encoding.UTF8);
            ItemJsonData _root = null;
            try
            {
                _root = JsonUtility.FromJson<ItemJsonData>(_json);

            }
            catch
            {
                return;
            }

            if (_root != null)
            {
                for (int i = 0; i < _root.items.Length; i++)
                {
#if UNITY_EDITOR
                    if (_root.items[i].iconLoadMethod == LoadMethod.DirectReference && _root.items[i].iconPath.Length > 0)
                    {
                        _root.items[i].icon = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath(_root.items[i].iconPath, typeof(Texture2D));
                    }
                    foreach (var obj in _root.items[i].customData)
                    {
                        if (obj.loadMethod == LoadMethod.DirectReference && obj.loadPath.Length > 0)
                        {
                            obj.value = UnityEditor.AssetDatabase.LoadAssetAtPath(obj.loadPath, typeof(UnityEngine.Object));
                        }
                    }
#endif
                    _root.items[i].fold = false;
                }

                if (Application.isPlaying)
                {
                    ItemDic.Clear();
                    foreach (var obj in _root.items)
                    {
                        ItemDic.TryAdd(obj.id, obj.Copy());
                    }
                }
                else
                {
                    if (DataMode == ItemDataMode.Unified)
                    {
                        items.Clear();
                        items.AddRange(_root.items);
                    }
                    else
                    {
                        if (SGD_Settings.isRuntime)
                        {
                            for (int i = 0; i < itemScriptableObjects.Count; i++)
                            {
                                if (i < _root.items.Length)
                                {
                                    itemScriptableObjects[i].mItem = _root.items[i].Copy();
                                }
                                else
                                {
                                    itemScriptableObjects.RemoveAt(i);
                                }
                            }
                        }
                        else
                        {
#if UNITY_EDITOR
                            for (int i = 0; i < _root.items.Length; i++)
                            {
                                if (i < itemScriptableObjects.Count)
                                {
                                    itemScriptableObjects[i].mItem = _root.items[i].Copy();
                                }
                                else
                                {
                                    Debug.LogError("There is no enough ScriptableObjects for item [" + _root.items[i].name+ "] , please add the same count of ScriptableObjects as the json data before trying to import (Count:"+_root.items.Length.ToString()+ "). ");
                                }
                            }
#endif
                        }
                    }
                }
                 
            }

        }



        /// <summary>
        /// Export the Enchantment data into a json txt file.
        /// </summary>
        /// <param name="_path"></param>
        public void ExportEnchantmentDataToJson(string _path)
        {
            EnchantmentJsonData _root = new EnchantmentJsonData();
            _root.enchantments = new Enchantment[itemEnchantments.Count];
            for (int i = 0; i < _root.enchantments.Length; i++)
            {
                Enchantment _newObj = itemEnchantments[i];
                _newObj.fold = false;
                _root.enchantments[i] = _newObj;
            }
            string _json = JsonUtility.ToJson(_root);
            File.WriteAllText(_path, _json, System.Text.Encoding.UTF8);
        }


        /// <summary>
        /// Import Enchantment data from a json txt file. 
        /// You can store the json file in your game install folder and let modder to modify it, then import it back when game launch.
        /// </summary>
        /// <param name="_path"></param>
        public void ImportEnchantmentDataFromJson(string _path)
        {
            if (!File.Exists(_path)) return;
            string _json = File.ReadAllText(_path, System.Text.Encoding.UTF8);
            EnchantmentJsonData _root = null;
            try
            {
                _root = JsonUtility.FromJson<EnchantmentJsonData>(_json);

            }
            catch
            {
                return;
            }

            if (_root != null)
            {
                for (int i = 0; i < _root.enchantments.Length; i++)
                {
                    _root.enchantments[i].fold = false;
                }
                itemEnchantments.Clear();
                itemEnchantments.AddRange(_root.enchantments);
                if (Application.isPlaying)
                {
                    Init();
                }
            }

        }



        /// <summary>
        /// Retrieves whether the Item with unique integer ID exists.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public bool ItemExist(int _id)
        {
            if (!SGD_Settings.isRuntime)
            {
                if (DataMode == ItemDataMode.Unified)
                {
                    foreach (var obj in items)
                    {
                        if (obj.uid == IdManager.GetStringKey(_id)) return true;
                    }
                }
                else
                {
                    foreach (var obj in itemScriptableObjects)
                    {
                        if (obj.mItem.uid == IdManager.GetStringKey(_id)) return true;
                    }
                }
                return false;
            }

            if (!runtimeInited || ItemDic.Count == 0) Init();
            if (ItemDic.ContainsKey(_id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Retrieves the Item setting based on its unique string UID. This is used for fetching specific Item data efficiently.
        /// </summary>
        /// <param name="_uid"></param>
        /// <returns></returns>
        public Item GetItem(string _uid)
        {
            return GetItem(IdManager.GetId(_uid));
        }

        /// <summary>
        /// Retrieves the Item setting based on its integer ID. This method allows for fetching by index for faster lookups in certain use cases.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public Item GetItem(int _id)
        {

            if (!SGD_Settings.isRuntime)
            {
                if (DataMode == ItemDataMode.Unified)
                {
                    foreach (var obj in items)
                    {
                        if (obj.uid == IdManager.GetStringKey(_id)) return obj;
                    }
                }
                else
                {
                    foreach (var obj in itemScriptableObjects)
                    {
                        if (obj.mItem.uid == IdManager.GetStringKey(_id)) return obj.mItem;
                    }
                }
                if (nullPlaceHolder == null) nullPlaceHolder = new Item() { name = "Invalid{" + _id + "}", uid = "Invalid" };
                return nullPlaceHolder;
            }

            if (!runtimeInited || ItemDic.Count == 0) Init();
            if (ItemDic.ContainsKey(_id))
            {
                return ItemDic[_id];
            }
            return null;
        }

        /// <summary>
        /// Convert Item int id to string uid.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public string GetStringKey(int _id)
        {
            return IdManager.GetStringKey(_id);
        }
        /// <summary>
        /// Convert Item string uid to int id.
        /// </summary>
        /// <param name="_uid"></param>
        /// <returns></returns>
        public int GetId(string _uid)
        {
            return IdManager.GetId(_uid);
        }

        #endregion
    }
}
