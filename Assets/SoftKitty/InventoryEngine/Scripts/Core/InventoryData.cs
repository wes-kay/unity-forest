using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
 
namespace SoftKitty.InventoryEngine
{
    


    /// <summary>
    /// The InventoryData class is responsible for managing all items and currencies for each unit.
    /// </summary>
    [System.Serializable]
    public class InventoryData
    {
        #region Variables
        /// <summary>
        /// The display name of this InventoryData.
        /// </summary>
        public string Name = "Inventory";
        /// <summary>
        /// The UID of the owner Entity of this InventoryData
        /// </summary>
        public string EntityUid = "";
        /// <summary>
        /// The owner Entity of this InventoryData
        /// </summary>
        public Entity mEntity
        {
            get
            {
                return GameManager.EntityManagerData.GetEntity(EntityUid);
            }
        }
        public enum HolderType
        {
            Crate,
            Merchant,
            PlayerInventory,
            PlayerEquipment,
            NpcInventory,
            NpcEquipment
        }

        /// <summary>
        ///  The type of this InventoryData class.
        ///  - Crate: Represents a crate or container.
        ///  - Merchant: Represents a merchant or store inventory.
        ///  - PlayerInventory: Represents the player¡¯s inventory.
        ///  - PlayerEquipment: Represents the player¡¯s equipped items.
        ///  - NpcInventory: Represents an NPC¡¯s inventory.
        ///  - NpcEquipment: Represents an NPC¡¯s equipped items.
        /// </summary>
        public HolderType Type = HolderType.PlayerInventory;
        /// <summary>
        /// The list of InventoryStack objects. All normal items are stored here.
        /// </summary>
        public List<InventoryStack> Stacks = new List<InventoryStack>();
        /// <summary>
        /// The list of InventoryStack objects where all hidden items are stored
        /// </summary>
        public List<InventoryStack> HiddenStacks = new List<InventoryStack>();
        /// <summary>
        /// The class to managing the currencies. Use the following functions to manipulate it:
        /// Currency.GetCurrency, Currency.SetCurrency, Currency.AddCurrency
        /// </summary>
        public CurrencySet Currency = new CurrencySet();
        /// <summary>
        /// The list of the currency values.
        /// </summary>
        public List<int> CurrencyValue
        {
            get
            {
                return Currency.Currency;
            }
        }
        /// <summary>
        /// The size of the Stacks. Items will be rejected if the inventory is full.
        /// </summary>
        public int InventorySize = 10;
        /// <summary>
        /// The maximum carry weight for this InventoryData.
        /// </summary>
        public float MaxiumCarryWeight = 1000F;
        /// <summary>
        /// This multiplier is applied when selling items. For example, a merchant NPC with a SellPriceMultiplier of 1.2 will sell an item priced at 1000 for 1200.
        /// </summary>
        public float SellPriceMultiplier = 1F;
        /// <summary>
        /// This multiplier is applied when buying items. For example, a merchant NPC with a BuyPriceMultiplier of 0.8 will buy an item priced at 1000 for 800.
        /// </summary>
        public float BuyPriceMultiplier = 1F;
        /// <summary>
        /// A list to specific price multiplier for certain items.x=item id, y=sell price multiplier, z=buy price multiplier
        /// </summary>
        public List<Vector3> SpecificPriceMultiplier = new List<Vector3>();
        /// <summary>
        /// (For merchant only)When set to false, this merchant will accept items within the 'TradeList' and 'TradeCategoryList'.
        /// </summary>
        public bool TradeAllItems = true;
        /// <summary>
        /// (For merchant only)When 'TradeAllItems'set to false, this merchant will accept items within this list.
        /// </summary>
        public List<int> TradeList = new List<int>();
        /// <summary>
        /// (For merchant only)When 'TradeAllItems'set to false, this merchant will accept items from categories within this list.
        /// </summary>
        public List<int> TradeCategoryList = new List<int>();

        /// <summary>
        /// Returns if a crafting job is processing.
        /// </summary>
        /// <returns>true = a crafting job is processing   |   false = no crafting job is procssing</returns>
        public bool isCrafting { get { return mCrafting; } }

        /// <summary>
        /// Returns the crafting item id.
        /// </summary>
        /// <returns> returns -1 if there is no crafting job is processing.</returns>
        public int CraftingItemId { get { return mCrafting ? mCraftingItemId : -1; } }

        /// <summary>
        /// Returns the crafting item number. 
        /// </summary>
        /// <returns>Returns 0 if there is no crafting job is processing.</returns>
        public int CraftingItemNumber { get { return mCrafting ? mCraftingItemNumber : 0; } }

        /// <summary>
        /// Returns the crafted item number. 
        /// </summary>
        /// <returns>Returns 0 if there is no crafting job is processing.</returns>
        public int CraftedItemNumber { get { return mCrafting ? mCraftedItemNumber : 0; } }

        /// <summary>
        /// Returns the crafting progress(0~1). 
        /// </summary>
        /// <returns>Returns 0 if there is no crafting job is processing.</returns>
        public float CraftingProgress { get { return mCrafting ? mCraftingProgress : 0; } }

        /// <summary>
        /// Returns true if the crafting failed.
        /// </summary>
        /// <returns></returns>
        public bool CraftingFailed { get { return mCraftingFailed; } }


        #endregion

        #region Internal Variables
        [System.NonSerialized]
        public bool runtimeFold = false;
        [System.NonSerialized]
        public bool uiFold = false;
        [System.NonSerialized]
        public bool uiMerchantExpand = false;
        [System.NonSerialized]
        public bool uiItemExpand = false;
        [System.NonSerialized]
        public bool uiHiddenExpand = false;
        [System.NonSerialized]
        public bool uiPriceFold = false;
        [System.NonSerialized]
        public bool uiTradeTypeFold = false;
        [System.NonSerialized]
        public bool uiTradeListFold = false;
        [System.NonSerialized]
        public bool uiCurrencyExpand = false;
        [System.NonSerialized]
        public ItemDropCallback mItemDropCallback;
        private ItemUseCallback mItemUseCallbacks;
        private ItemChangeCallback mItemChangeCallbacks;
        private CraftingUi mCraftingWindow;
        private float mCraftingProgress = 0F;
        private int mCraftingItemId;
        private int mCraftingItemNumber;
        private int mCraftedItemNumber;
        private bool mCraftingFailed = false;
        private bool mCrafting = false;
        private float _weight = 0F;
        private CraftingStateCallback OnCraftingStateChange;
        #endregion

        /// <summary>
        /// Return a copy of this InventoryData
        /// </summary>
        /// <returns></returns>
        public InventoryData Copy()
        {
            InventoryData _copy = new InventoryData();
            _copy.Name = this.Name;
            _copy.EntityUid = this.EntityUid;
            _copy.Type = this.Type;
            _copy.Stacks = new List<InventoryStack>();
            foreach (var obj in Stacks)
            {
                _copy.Stacks.Add(obj.Copy());
            }
            _copy.HiddenStacks = new List<InventoryStack>();
            foreach (var obj in HiddenStacks)
            {
                _copy.HiddenStacks.Add(obj.Copy());
            }
            _copy.Currency = Currency.Copy();
            _copy.InventorySize = this.InventorySize;
            _copy.MaxiumCarryWeight = this.MaxiumCarryWeight;
            _copy.SellPriceMultiplier = this.SellPriceMultiplier;
            _copy.BuyPriceMultiplier = this.BuyPriceMultiplier;
            _copy.SpecificPriceMultiplier = new List<Vector3>();
            foreach (var obj in SpecificPriceMultiplier)
            {
                _copy.SpecificPriceMultiplier.Add(obj);
            }
            _copy.TradeAllItems = this.TradeAllItems;
            _copy.TradeList = new List<int>();
            foreach (var obj in TradeList)
            {
                _copy.TradeList.Add(obj);
            }
            _copy.TradeCategoryList = new List<int>();
            foreach (var obj in TradeCategoryList)
            {
                _copy.TradeCategoryList.Add(obj);
            }
            _copy.uiFold = uiFold;
            return _copy;
        }

        

        #region Internal Methods
        public void Init()
        {
            Currency.Init();
            RefreshItemData();
            Currency.AutoExchange();
        }



        /// <summary>
        /// Return the difference value of currency between this InventoryData and the target.
        /// </summary>
        /// <param name="_targetList"></param>
        /// <returns></returns>
        public List<int> GetCurrencyDifference(List<int> _targetList)// 
        {
            CurrencySet _source = new CurrencySet(Currency.Currency);
            _source.KeepPostive = false;
            _source.CollapseAllToLowestCurrenc();

            CurrencySet _target = new CurrencySet(_targetList);
            _target.CollapseAllToLowestCurrenc();

            int _maxLength = Mathf.Max(_target.Count, Currency.Count);
            for (int i = 0; i < _maxLength; i++)
            {
                _source.AddCurrency(i, -_target.GetCurrency(i));
            }
            return _source.Currency;
        }

        /// <summary>
        /// In case the saved data is outdated, pull newest settings from ItemObject.instance by the uid of items.
        /// </summary>
        public void RefreshItemData()
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (!Stacks[i].Empty && Stacks[i].Item != null)
                {
                    int _uid = Stacks[i].GetItemId();
                    if (_uid!=-1 && ItemObject.instance.TryGetItem(_uid) != null)
                    {
                        int _level = Stacks[i].Item.upgradeLevel;
                        int _socketingCount = Stacks[i].Item.socketingSlots;
                        List<int> _socketedItems = new List<int>();
                        _socketedItems.AddRange(Stacks[i].Item.socketedItems);
                        List<int> _enchantments = new List<int>();
                        _enchantments.AddRange(Stacks[i].Item.enchantments);
                        Dictionary<string, bool> _attLock = new Dictionary<string, bool>();
                        Dictionary<string, string> _attValue = new Dictionary<string, string>();
                        foreach (var obj in Stacks[i].Item.attributes)
                        {
                            if (!_attLock.ContainsKey(obj.uid))
                            {
                                _attLock.Add(obj.uid, obj.locked);
                                if (GameManager.GetAttribute(obj.uid).stringValue)
                                {
                                    _attValue.Add(obj.uid, obj.GetString());
                                }
                                else
                                {
                                    _attValue.Add(obj.uid, obj.GetFloat().ToString());
                                }
                            }
                        }
                        Stacks[i].Item = ItemObject.instance.TryGetItem(_uid).Copy();
                        Stacks[i].Item.upgradeLevel = _level;
                        Stacks[i].Item.socketingSlots = _socketingCount;
                        Stacks[i].Item.socketedItems = _socketedItems;
                        Stacks[i].Item.enchantments = _enchantments;
                        foreach (var obj in Stacks[i].Item.attributes)
                        {
                            if (_attLock.ContainsKey(obj.uid))
                            {
                                obj.locked = _attLock[obj.uid];
                                if (GameManager.GetAttribute(obj.uid).stringValue)
                                {
                                    obj.stringValue = _attValue[obj.uid];
                                }
                                else
                                {
                                    float _value = 0F;
                                    float.TryParse(_attValue[obj.uid], out _value);
                                    obj.floatValue = _value;
                                }
                                _attLock.Remove(obj.uid);
                            }
                        }
                       
                        _attLock.Clear();
                        _attValue.Clear();
                    }
                    else
                    {
                        Stacks[i].Delete();
                    }
                }
            }
            for (int i = 0; i < HiddenStacks.Count; i++)
            {
                if (!HiddenStacks[i].Empty && HiddenStacks[i].Item != null)
                {
                    int _uid = HiddenStacks[i].GetItemId();
                    if (ItemObject.instance.TryGetItem(_uid) != null)
                    {
                        int _level = HiddenStacks[i].Item.upgradeLevel;
                        int _socketingCount = HiddenStacks[i].Item.socketingSlots;
                        List<int> _socketedItems = new List<int>();
                        _socketedItems.AddRange(HiddenStacks[i].Item.socketedItems);
                        List<int> _enchantments = new List<int>();
                        _enchantments.AddRange(HiddenStacks[i].Item.enchantments);
                        Dictionary<string, bool> _attLock = new Dictionary<string, bool>();
                        Dictionary<string, string> _attValue = new Dictionary<string, string>();
                        foreach (var obj in HiddenStacks[i].Item.attributes)
                        {
                            if (!_attLock.ContainsKey(obj.uid))
                            {
                                _attLock.Add(obj.uid, obj.locked);
                                if (GameManager.GetAttribute(obj.uid).stringValue)
                                {
                                    _attValue.Add(obj.uid, obj.GetString());
                                }
                                else
                                {
                                    _attValue.Add(obj.uid, obj.GetFloat().ToString());
                                }
                            }
                        }
                        HiddenStacks[i].Item = ItemObject.instance.TryGetItem(_uid).Copy();
                        HiddenStacks[i].Item.upgradeLevel = _level;
                        HiddenStacks[i].Item.socketingSlots = _socketingCount;
                        HiddenStacks[i].Item.socketedItems = _socketedItems;
                        HiddenStacks[i].Item.enchantments = _enchantments;
                        foreach (var obj in HiddenStacks[i].Item.attributes)
                        {
                            if (_attLock.ContainsKey(obj.uid))
                            {
                                obj.locked = _attLock[obj.uid];
                                if (GameManager.GetAttribute(obj.uid).stringValue)
                                {
                                    obj.stringValue = _attValue[obj.uid];
                                }
                                else
                                {
                                    float _value = 0F;
                                    float.TryParse(_attValue[obj.uid], out _value);
                                    obj.floatValue = _value;
                                }
                                _attLock.Remove(obj.uid);
                            }
                        }
                        _attLock.Clear();
                        _attValue.Clear();
                    }
                    else
                    {
                        HiddenStacks[i].Delete();
                    }
                }
            }
            if (Currency.Count != ItemObject.instance.currencies.Count)
            {
                List<int> _temp = new List<int>();
                _temp.AddRange(Currency.GetCurrencyArray());
                Currency.Reset();
                for (int i = 0; i < ItemObject.instance.currencies.Count; i++)
                {
                    if (i < _temp.Count)
                        Currency.Add(_temp[i]);
                    else
                        Currency.Add(0);
                }
            }

        }


        public void StartCrafting(int _itemId, int _number, List<Vector2> _materials, float _craftingTime)
        {
            string _uid = "Cafting" + Type.ToString() + "_" + mEntity.uid;
            if (SoftKittyJobs.isJobExist(_uid)) SoftKittyJobs.EndJob(_uid);
            mCraftingItemId = _itemId;
            mCraftingItemNumber = _number;
            mCraftingFailed = false;
            mCraftedItemNumber = 0;
            mCrafting = true;
            mCraftingProgress = 0F;
            if (OnCraftingStateChange != null) OnCraftingStateChange(CraftingState.StartCrafting, _craftingTime);
            SoftKittyJobs.StartJob("Cafting_" + Type.ToString() + mEntity.uid, CaftingCallback, new Arg("count", 0), new Arg("itemId", _itemId), new Arg("number", _number), new Arg("materials", _materials), new Arg("craftingTime", _craftingTime));
        }

        public void CaftingCallback(ref SoftKittyJob _job)
        {
            int _count = (int)_job.GetArg("count");
            int _number = (int)_job.GetArg("number");
            float _craftingTime = (float)_job.GetArg("craftingTime");
            int _itemId = (int)_job.GetArg("itemId");
            List<Vector2> _materials = (List<Vector2>)_job.GetArg("materials");
            if (_count < _number)
            {
                if (mCraftingProgress < 1F)
                {
                    mCraftingProgress = Mathf.MoveTowards(mCraftingProgress, 1F, Time.unscaledDeltaTime * (1F / _craftingTime));
                    if (OnCraftingStateChange != null) OnCraftingStateChange( CraftingState.CraftingProgress, (_number-1 - _count)* _craftingTime+(1F - mCraftingProgress) * _craftingTime);
                    return;
                }
                else
                {
                    mCraftingProgress = 0F;
                    bool _goodToGo = true;
                    for (int i = 0; i < _materials.Count; i++)
                    {
                        if (GetItemNumber(Mathf.FloorToInt(_materials[i].x)) < Mathf.FloorToInt(_materials[i].y)) _goodToGo = false;
                    }
                    if (_goodToGo)
                    {
                        for (int i = 0; i < _materials.Count; i++)
                        {
                            RemoveItem(Mathf.FloorToInt(_materials[i].x), Mathf.FloorToInt(_materials[i].y));
                        }
                        InventoryStack _leftStack = AddItem(new Item(_itemId, true, true), 1);
                        if (_leftStack.isEmpty())
                        {
                            mCraftedItemNumber++;
                            mCraftingFailed = false;
                        }
                        else
                        {
                            DynamicMsg.PopMsg(ItemObject.instance.msgBagFull);
                            mCraftingFailed = true;
                            _count = _number;
                        }
                        _job.SetArg("count", _count + 1);
                    }
                    else
                    {
                        mCraftingFailed = true;
                        _job.Skip(1);
                        mCrafting = false;
                        SoftKittyJobs.EndJob("Cafting_" + Type.ToString() + mEntity.uid);
                        if (OnCraftingStateChange != null) OnCraftingStateChange(CraftingState.EndCrafting, -1F);
                    }
                    _job.Skip(1);
                }
                return;
            }
            if (Type == HolderType.PlayerInventory && mCraftedItemNumber > 0) DynamicMsg.PopItem(ItemObject.instance.TryGetItem(_itemId), mCraftedItemNumber);
            _job.Skip(1);
            SoftKittyJobs.EndJob("Cafting_" + Type.ToString() + mEntity.uid);
            if (OnCraftingStateChange != null) OnCraftingStateChange(CraftingState.EndCrafting, mCraftingFailed?-1F:0F);
            mCrafting = false;
        }



        public void CalWeight()
        {
            _weight = 0F;
            for (int i = 0; i < Stacks.Count; i++) _weight += Stacks[i].GetWeight();
        }


        #endregion

        /// <summary>
        /// Returns the InventoryData of specified HolderType from the providing Entity.
        /// </summary>
        /// <param name="_gameObject"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static InventoryData GetInventoryDataByType(Entity _entity, HolderType _type) //
        {
#if MASTER_INVENTORY_ENGINE
            if (_entity == null) return null;
            return _entity.GetModule<InventoryModule>().GetInventoryDataByType(_type);
#else
            return null;
#endif
        }

        /// <summary>
        /// Register the callback for the change of the crafting state.
        /// </summary>
        /// <param name="_callback"></param>
        public void RegisterCraftingStateCallback(CraftingStateCallback _callback)
        {
            OnCraftingStateChange += _callback;
        }

        /// <summary>
        /// Unregister the callback for the change of the crafting state.
        /// </summary>
        /// <param name="_callback"></param>
        public void UnRegisterCraftingStateCallback(CraftingStateCallback _callback)
        {
            OnCraftingStateChange -= _callback;
        }

        /// <summary>
        /// Clear the callback for the change of the crafting state.
        /// </summary>
        public void ClearCraftingStateCallback()
        {
            OnCraftingStateChange = null;
        }

        /// <summary>
        /// Opens the appropriate interface based on the type of this InventoryData.
        /// </summary>
        /// <returns></returns>
        public UiWindow OpenWindow()
        {
            if (this == null)
            {
                Debug.LogError("The InventoryData you are trying to access is missing.");
                return null;
            }
            UiWindow _newWindow = null;
#if MASTER_INVENTORY_ENGINE
            switch (Type)
            {
                case HolderType.PlayerInventory:
                    _newWindow = WindowsManager.GetWindow(ItemObject.instance.PlayerInventoryWindowName, this);
                    if (_newWindow != null) _newWindow.Initialize(this, ItemObject.PlayerEquipmentData, Name);
                    break;
                case HolderType.PlayerEquipment:
                    _newWindow = WindowsManager.GetWindow(ItemObject.instance.PlayerEquipWindowName, this);
                    if (_newWindow != null) _newWindow.Initialize(ItemObject.PlayerInventoryData, this, Name);
                    break;
                case HolderType.Merchant:
                    _newWindow = WindowsManager.GetWindow(ItemObject.instance.MerchantWindowName + (ItemObject.instance.MerchantStyle > 0 ? ItemObject.instance.MerchantStyle.ToString() : ""), this);
                    if (_newWindow != null) _newWindow.Initialize(ItemObject.PlayerInventoryData, this, Name);
                    break;
                case HolderType.Crate:
                    _newWindow = WindowsManager.GetWindow(ItemObject.instance.StorageWindowName, this);
                    if (_newWindow != null) _newWindow.Initialize(this, null, Name);
                    break;
                case HolderType.NpcInventory:
                    _newWindow = WindowsManager.GetWindow(ItemObject.instance.NpcInventoryWindowName, this);
                    if (_newWindow != null) _newWindow.Initialize(this, null, Name);
                    break;
                case HolderType.NpcEquipment:
                    InventoryData _inventory = mEntity.GetModule<InventoryModule>().GetInventoryDataByType(HolderType.NpcInventory);
                    if (_inventory == null)
                    {
                        _inventory = ItemObject.PlayerInventoryData;
                    }
                    _newWindow = WindowsManager.GetWindow(ItemObject.instance.NpcEquipmentWindowName, this);
                    if (_newWindow != null)
                    {
                        _newWindow.Initialize(_inventory, this, Name);
                        _newWindow.GetComponent<EquipmentUi>().SetPlayerName(Name);
                        _newWindow.GetComponent<EquipmentUi>().SetPlayerLevelText("<NPC>");
                    }
                    break;
            }
#endif
            return _newWindow;
        }

        /// <summary>
        /// Close the opened window associated with this InventoryData.
        /// </summary>
        public void CloseWindow()
        {
            if (WindowsManager.getOpenedWindow(this) != null) WindowsManager.getOpenedWindow(this).Close();
        }


        /// <summary>
        /// Opens the crafting window for this InventoryData, using the items in Stacks as materials.
        /// </summary>
        /// <param name="_enableCrafting">Enable Crafting Panel</param>
        /// <param name="_enableEnhancing">Enable Enhancing Panel</param>
        /// <param name="_enableEnchanting">Enable Enchanting Panel</param>
        /// <param name="_enableSocketing">Enable Socketing Panel</param>
        /// <param name="_craftingTimeMultiplier">The multiplier for crafting time, 1x as 100% </param>
        /// <param name="_name">The name of the window</param>
        /// <param name="_blueprintSubTags">The list of sub-tags to filter the crafting blueprints, eg.Food,Weapon,Armor... </param>
        /// <returns></returns>
        public UiWindow OpenForgeWindow( bool _enableCrafting = true, bool _enableEnhancing = true, bool _enableEnchanting = true, bool _enableSocketing = true, float _craftingTimeMultiplier = 1F, string _name = "Forge", List<string> _blueprintSubTags = null)
        {
            UiWindow _newWindow = WindowsManager.GetWindow(ItemObject.instance.ForgeWindowName, this);
            if (_newWindow != null)
            {
                _newWindow.GetComponent<CraftingUi>().BlueprintTags.Clear();
                if (_blueprintSubTags!=null) {
                    _newWindow.GetComponent<CraftingUi>().BlueprintTags.AddRange(_blueprintSubTags);
                }
                _newWindow.GetComponent<CraftingUi>().EnableCrafting = _enableCrafting;
                _newWindow.GetComponent<CraftingUi>().EnableEnhancing = _enableEnhancing;
                _newWindow.GetComponent<CraftingUi>().EnableEnchanting = _enableEnchanting;
                _newWindow.GetComponent<CraftingUi>().EnableSocketing = _enableSocketing;
                _newWindow.GetComponent<CraftingUi>().CraftingTimeMultiplier = _craftingTimeMultiplier;
                mCraftingWindow = _newWindow.GetComponent<CraftingUi>();
                mCraftingWindow.Initialize(this, null, _name);
            }
            else
            {
                mCraftingWindow = null;
            }
            return _newWindow;
        }


        /// <summary>
        /// Opens a window prefab inheriting from UiWindow.cs and sets its title.
        /// </summary>
        /// <param name="_prefabName"></param>
        /// <param name="_displayName"></param>
        /// <returns></returns>
        public UiWindow OpenWindowByName(string _prefabName, string _displayName)
        {
            UiWindow _newWindow = WindowsManager.GetWindow(_prefabName, this);
            if (_newWindow != null) _newWindow.Initialize(this, null, _displayName);
            return _newWindow;
        }


        /// <summary>
        /// Retrieves the total value of an attribute by its uid from all equipped items and the base stats. 
        /// </summary>
        /// <param name="_attributeuid"></param>
        /// <returns></returns>
        public float GetAttributeValue(string _uid)
        {
            float _value = 0F;
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (!Stacks[i].isEmpty())
                {
                    _value += Stacks[i].Item.GetAttributeFloat(_uid);
                }
            }
            return _value;
        }

        /// <summary>
        /// Retrieves the total value of an attribute by its id from all equipped items and the base stats. 
        /// </summary>
        /// <param name="_attributeuid"></param>
        /// <returns></returns>
        public float GetAttributeValue(int _id)
        {
            float _value = 0F;
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (!Stacks[i].isEmpty())
                {
                    _value += Stacks[i].Item.GetAttributeFloat(_id);
                }
            }
            return _value;
        }



        /// <summary>
        /// Retrieves the total value of an attribute by its uid, counting only equipment with matching tags.
        /// </summary>
        /// <param name="_attributeuid"></param>
        /// <param name="_tags"></param>
        /// <returns></returns>
        public float GetAttributeValueByTag(string _uid, List<string> _tags)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (!Stacks[i].isEmpty() && Stacks[i].Item.isTagsMatchList(_tags, false))
                {
                    return Stacks[i].Item.GetAttributeFloat(_uid);
                }
            }
            return 0F;
        }

        /// <summary>
        /// Retrieves the total value of an attribute by its integer id, counting only equipment with matching tags.
        /// </summary>
        /// <param name="_attributeuid"></param>
        /// <param name="_tags"></param>
        /// <returns></returns>
        public float GetAttributeValueByTag(int _id, List<string> _tags)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (!Stacks[i].isEmpty() && Stacks[i].Item.isTagsMatchList(_tags, false))
                {
                    return Stacks[i].Item.GetAttributeFloat(_id);
                }
            }
            return 0F;
        }

 


        /// <summary>
        /// Retrieves the number of items with a specific UID. Set _includeHiddenStacks to indicate whether include the hidden stacks.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_includeHiddenStacks"></param>
        /// <returns></returns>
        public int GetItemNumber(int _uid, bool _includeHiddenStacks=true)
        {
            int _result = 0;
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid))
                {
                    _result += Stacks[i].Number;
                }
            }
            if (_includeHiddenStacks) {
                for (int i = 0; i < HiddenStacks.Count; i++)
                {
                    if (HiddenStacks[i].isSameItem(_uid))
                    {
                        _result += HiddenStacks[i].Number;
                    }
                }
            }
            return _result;
        }


        /// <summary>
        /// Retrieves the number of items with a specific UID, but only counts the items with the highest upgrade level if there are multiple items with the same UID.
        /// </summary>
        /// <param name="_uid"></param>
        /// <returns></returns>
        public int GetItemNumberWithHighestUpgradeLevel(int _uid)
        {
            int _result = 0;
            int _level = GetHighestUpgradeLevel(_uid);
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid) && Stacks[i].GetUpgradeLevel() >= _level)
                {
                    _result += Stacks[i].Number;
                }
            }
            for (int i = 0; i < HiddenStacks.Count; i++)
            {
                if (HiddenStacks[i].isSameItem(_uid) && HiddenStacks[i].GetUpgradeLevel() >= _level)
                {
                    _result += HiddenStacks[i].Number;
                }
            }
            return _result;
        }

        /// <summary>
        /// Retrieves the highest upgrade level of the item with a specific UID.
        /// </summary>
        /// <param name="_uid"></param>
        /// <returns></returns>
        public int GetHighestUpgradeLevel(int _uid)
        {
            int _level = 0;
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid) && Stacks[i].GetUpgradeLevel() > _level)
                {
                    _level = Stacks[i].GetUpgradeLevel();
                }
            }
            return _level;
        }

        /// <summary>
        /// Returns how many more items with the specified UID can be stacked in this InventoryData. Set a cap number to improve performance.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_maxmiumNumber"></param>
        /// <returns></returns>
        public int GetAvailableSpace(int _uid, int _maxmiumNumber = 999)
        {
            if (ItemObject.instance.TryGetItem(_uid) == null)
            {
                Debug.LogError("Trying to access item with invalid item uid.");
                return 0;
            }
            int _result = 0;
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid))
                {
                    _result += Stacks[i].GetAvailableSpace();
                }
                else if (Stacks[i].isEmpty())
                {
                    _result += ItemObject.instance.TryGetItem(i).maxiumStack;
                }

                if (_result >= _maxmiumNumber) break;
            }
            return _result;
        }


        /// <summary>
        /// Retrieves the currency value by its index number.Set _includeExchangedValue to true if you want to get the total amount of this currency including the exchanged value from other currencies.
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_includeExchangedValue"></param>
        /// <returns></returns>
        public int GetCurrency(int _type, bool _includeExchangedValue = false)
        {
            return Currency.GetCurrency(_type, _includeExchangedValue);
        }


        /// <summary>
        /// Adds to the currency value by its index number. The _add value can be negative.
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_add"></param>
        public void AddCurrency(int _type, int _add)
        {
            Currency.AddCurrency(_type, _add);
        }

        /// <summary>
        /// Overrides the currency value by its index number.
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="_value"></param>
        public void SetCurrency(int _type, int _value)
        {
            Currency.SetCurrency(_type, _value);
        }


        /// <summary>
        /// Returns a save class of all the item data in this InventoryData.
        /// </summary>
        /// <returns></returns>
        public ItemSaveRoot GetSaveData()
        {
            ItemSave[] _saveItems = new ItemSave[Stacks.Count];
            for (int i = 0; i < _saveItems.Length; i++)
            {
                _saveItems[i] = new ItemSave();
                if (Stacks[i].isEmpty())
                {
                    _saveItems[i].id = -1;
                    _saveItems[i].upgrade = 0;
                    _saveItems[i].enchantments = new int[0];
                    _saveItems[i].socketedItem = new int[0];
                    _saveItems[i].number = 0;
                    _saveItems[i].attributeLock = new bool[0];
                    _saveItems[i].attributeValue = new string[0];
                    _saveItems[i].fav = false;
                }
                else
                {
                    _saveItems[i].id = Stacks[i].Item.id;
                    _saveItems[i].upgrade = Stacks[i].Item.upgradeLevel;
                    _saveItems[i].enchantments = Stacks[i].Item.enchantments.ToArray();
                    _saveItems[i].socketedItem = Stacks[i].Item.socketedItems.ToArray();
                    _saveItems[i].number = Stacks[i].Number;
                    _saveItems[i].attributeLock = new bool[Stacks[i].Item.attributes.Count];
                    _saveItems[i].attributeValue = new string[Stacks[i].Item.attributes.Count];
                    for (int u = 0; u < Stacks[i].Item.attributes.Count; u++)
                    {
                        _saveItems[i].attributeLock[u] = Stacks[i].Item.attributes[u].locked;
                        if (GameManager.GetAttribute(Stacks[i].Item.attributes[u].uid).stringValue)
                        {
                            _saveItems[i].attributeValue[u] = Stacks[i].Item.attributes[u].stringValue;
                        }
                        else
                        {
                            _saveItems[i].attributeValue[u] = Stacks[i].Item.attributes[u].floatValue.ToString();
                        }
                    }
                    _saveItems[i].fav = Stacks[i].Item.favorite;
                }
            }
            ItemSave[] _hiddenItems = new ItemSave[HiddenStacks.Count];
            for (int i = 0; i < _hiddenItems.Length; i++)
            {
                _hiddenItems[i] = new ItemSave();
                if (HiddenStacks[i].isEmpty())
                {
                    _hiddenItems[i].id = -1;
                    _hiddenItems[i].upgrade = 0;
                    _hiddenItems[i].enchantments = new int[0];
                    _hiddenItems[i].socketedItem = new int[0];
                    _hiddenItems[i].number = 0;
                    _hiddenItems[i].attributeLock = new bool[0];
                    _hiddenItems[i].attributeValue = new string[0];
                    _hiddenItems[i].fav = false;
                }
                else
                {
                    _hiddenItems[i].id = HiddenStacks[i].Item.id;
                    _hiddenItems[i].upgrade = HiddenStacks[i].Item.upgradeLevel;
                    _hiddenItems[i].enchantments = HiddenStacks[i].Item.enchantments.ToArray();
                    _hiddenItems[i].socketedItem = HiddenStacks[i].Item.socketedItems.ToArray();
                    _hiddenItems[i].number = HiddenStacks[i].Number;
                    _hiddenItems[i].attributeLock = new bool[HiddenStacks[i].Item.attributes.Count];
                    _hiddenItems[i].attributeValue = new string[HiddenStacks[i].Item.attributes.Count];
                    for (int u = 0; u < HiddenStacks[i].Item.attributes.Count; u++)
                    {
                        _hiddenItems[i].attributeLock[u] = HiddenStacks[i].Item.attributes[u].locked;
                        if (GameManager.GetAttribute(HiddenStacks[i].Item.attributes[u].uid).stringValue)
                        {
                            _hiddenItems[i].attributeValue[u] = HiddenStacks[i].Item.attributes[u].stringValue;
                        }
                        else
                        {
                            _hiddenItems[i].attributeValue[u] = HiddenStacks[i].Item.attributes[u].floatValue.ToString();
                        }
                    }
                    _hiddenItems[i].fav = HiddenStacks[i].Item.favorite;
                }
            }
            int[] _currency = Currency.GetCurrencyArray();
            ItemSaveRoot _saveRoot = new ItemSaveRoot();
            _saveRoot.items = _saveItems;
            _saveRoot.hiddenItems = _hiddenItems;
            _saveRoot.currency = _currency;
            _saveRoot.Name = Name;
            _saveRoot.EntityUid = EntityUid;
            _saveRoot.Type = Type;
            _saveRoot.InventorySize = InventorySize;
            _saveRoot.MaxiumCarryWeight = MaxiumCarryWeight;
            _saveRoot.SellPriceMultiplier = SellPriceMultiplier;
            _saveRoot.BuyPriceMultiplier = BuyPriceMultiplier;
            _saveRoot.SpecificPriceMultiplier = new List<Vector3>();
            for (int i=0;i< SpecificPriceMultiplier.Count;i++) _saveRoot.SpecificPriceMultiplier.Add(SpecificPriceMultiplier[i]);
            _saveRoot.TradeAllItems = TradeAllItems;
            _saveRoot.TradeList = new List<int>();
            for (int i = 0; i < TradeList.Count; i++) _saveRoot.TradeList.Add(TradeList[i]);
            _saveRoot.TradeCategoryList = new List<int>();
            for (int i = 0; i < TradeCategoryList.Count; i++) _saveRoot.TradeCategoryList.Add(TradeCategoryList[i]);
            return _saveRoot;
        }


        /// <summary>
        /// Loads item data into this InventoryData from a save class.
        /// </summary>
        /// <param name="_data"></param>
        public void LoadSaveData(ItemSaveRoot _saveRoot)
        {
            Name = _saveRoot.Name;
            EntityUid = _saveRoot.EntityUid;
            Type = _saveRoot.Type;
            InventorySize = _saveRoot.InventorySize;
            MaxiumCarryWeight = _saveRoot.MaxiumCarryWeight;
            SellPriceMultiplier = _saveRoot.SellPriceMultiplier;
            BuyPriceMultiplier = _saveRoot.BuyPriceMultiplier;
            SpecificPriceMultiplier = new List<Vector3>();
            for (int i = 0; i < _saveRoot.SpecificPriceMultiplier.Count; i++) SpecificPriceMultiplier.Add(_saveRoot.SpecificPriceMultiplier[i]);
            TradeAllItems = _saveRoot.TradeAllItems;
            TradeList = new List<int>();
            for (int i = 0; i < _saveRoot.TradeList.Count; i++) TradeList.Add(_saveRoot.TradeList[i]);
            TradeCategoryList = new List<int>();
            for (int i = 0; i < _saveRoot.TradeCategoryList.Count; i++) TradeCategoryList.Add(_saveRoot.TradeCategoryList[i]);

            Stacks.Clear();
            HiddenStacks.Clear();
            for (int i = 0; i < _saveRoot.items.Length; i++)
            {
                if (ItemObject.instance.TryGetItem(_saveRoot.items[i].id) != null)
                {
                    Stacks.Add(new InventoryStack(_saveRoot.items[i]));
                }
                else
                {
                    Stacks.Add(new InventoryStack());
                }
            }
            for (int i = 0; i < _saveRoot.hiddenItems.Length; i++)
            {
                if (ItemObject.instance.TryGetItem(_saveRoot.hiddenItems[i].id) != null)
                {
                    HiddenStacks.Add(new InventoryStack(_saveRoot.hiddenItems[i]));
                }
                else
                {
                    HiddenStacks.Add(new InventoryStack());
                }
            }
            Currency.Reset();
            Currency.Init(_saveRoot.currency);

        }


        /// <summary>
        /// Whenever you manually change the item data of an InventoryData, be sure to call ItemChanged() to inform other scripts to update linked data.
        /// </summary>
        /// <param name="_changedItems"></param>
        public void ItemChanged(Dictionary<Item, int> _changedItems)
        {
            CalWeight();
            if (mItemChangeCallbacks != null) mItemChangeCallbacks(_changedItems);
        }



        /// <summary>
        /// Captures a snapshot of the current data state of this InventoryData. You can revert to this snapshot later if needed.
        /// </summary>
        /// <returns></returns>
        public InventorySnapShot GetSnapShot()
        {
            return new InventorySnapShot(this);
        }

        /// <summary>
        /// Reverts all item data to the provided snapshot.
        /// </summary>
        /// <param name="_snapshot"></param>
        public void RevertSnapShot(InventorySnapShot _snapshot)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                Stacks[i].Set(_snapshot.Stacks[i]);
            }
            for (int i = 0; i < HiddenStacks.Count; i++)
            {
                HiddenStacks[i].Set(_snapshot.HiddenStacks[i]);
            }
            Currency = _snapshot.Currency.Copy();
            _weight = _snapshot.Weight;
        }

        /// <summary>
        /// Registers a callback that is triggered when an item from this InventoryData is used. Remember to call UnRegisterItemUseCallback() when the game object of your script is destroyed.
        /// </summary>
        /// <param name="_callback"></param>
        public void RegisterItemUseCallback(ItemUseCallback _callback)
        {
            mItemUseCallbacks += _callback;
        }


        /// <summary>
        /// Unregisters the callback. It¡¯s best to call this in OnDestroy() when the callback receiver is destroyed.
        /// </summary>
        /// <param name="_callback"></param>
        public void UnRegisterItemUseCallback(ItemUseCallback _callback)
        {
            try
            {
                mItemUseCallbacks -= _callback;
            }
            catch
            {

            }
        }


        /// <summary>
        /// Clears all registered callbacks for item use.
        /// </summary>
        public void ClearAllItemUseCallback()
        {
            mItemUseCallbacks = null;
        }


        /// <summary>
        /// Registers a callback that is triggered when items in this InventoryData are changed. Remember to call UnRegisterItemChangeCallback() when the game object of your script is destroyed.
        /// </summary>
        /// <param name="_callback"></param>
        public void RegisterItemChangeCallback(ItemChangeCallback _callback)
        {
            mItemChangeCallbacks += _callback;
        }


        /// <summary>
        /// Unregisters the callback. It¡¯s best to call this in OnDestroy() when the callback receiver is destroyed.
        /// </summary>
        /// <param name="_callback"></param>
        public void UnRegisterItemChangeCallback(ItemChangeCallback _callback)
        {
            try
            {
                mItemChangeCallbacks -= _callback;
            }
            catch
            {

            }
        }

        /// <summary>
        /// Clears all registered callbacks for item changes.
        /// </summary>
        public void ClearAllItemChangeCallback()
        {
            mItemChangeCallbacks = null;
        }

        /// <summary>
        /// Registers a callback that is triggered when items in this InventoryData has been dropped by dragging out of the window. Remember to call UnRegisterItemDropCallback() when the game object of your script is destroyed.
        /// </summary>
        /// <param name="_callback"></param>
        public void RegisterItemDropCallback(ItemDropCallback _callback)
        {
            mItemDropCallback += _callback;
        }


        /// <summary>
        /// Unregisters the callback. It¡¯s best to call this in OnDestroy() when the callback receiver is destroyed.
        /// </summary>
        /// <param name="_callback"></param>
        public void UnRegisterItemDropCallback(ItemDropCallback _callback)
        {
            try
            {
                mItemDropCallback -= _callback;
            }
            catch
            {

            }
        }


        /// <summary>
        /// Clears all registered callbacks for item drop.
        /// </summary>
        public void ClearAllItemDropCallback()
        {
            mItemDropCallback = null;
        }


        /// <summary>
        /// Clears all normal items in this InventoryData.
        /// </summary>
        public void ClearInventoryItems()
        {
            Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
            foreach (var obj in Stacks)
            {
                if (!obj.isEmpty()) _changedItems.Add(obj.Item, -obj.Number);
            }
            Stacks.Clear();
            for (int i = 0; i < InventorySize; i++)
            {
                Stacks.Add(new InventoryStack());
            }
            ItemChanged(_changedItems);
        }

        /// <summary>
        /// Clears all hidden items in this InventoryData.
        /// </summary>
        public void ClearHiddenItems()
        {
            HiddenStacks.Clear();
        }


        /// <summary>
        /// Retrieves the total weight of all items in this InventoryData (hidden items are not counted).
        /// </summary>
        /// <returns></returns>
        public float GetWeight()
        {
            return _weight;
        }


        /// <summary>
        /// Equip an item from _inventoryData to _equipmentData by specific item uid and slot index of inventory.
        /// </summary>
        /// <param name="_inventoryData"></param>
        /// <param name="_equipmentData"></param>
        /// <param name="_itemId"></param>
        /// <param name="_inventorySlotIndex"></param>
        public static void EquipItem(InventoryData _inventoryData, InventoryData _equipmentData, int _itemId, int _inventorySlotIndex)
        {
            if (WindowsManager.getOpenedWindow(_equipmentData) != null) return;
            EquipmentUi _equipment = Resources.Load<GameObject>("InventoryEngine/UiWindows/Equipment").GetComponent<EquipmentUi>();

            foreach (InventoryItem item in _equipment.EquipItems)
            {
                
                if (ItemObject.instance.TryGetItem(_itemId).isTagMatchText(item.LimitedByTag))
                {
                    if (_inventorySlotIndex != -1)
                    {
                        if (_inventoryData.Stacks[_inventorySlotIndex].GetItemId() == _itemId && _inventoryData.Stacks[_inventorySlotIndex].Number > 0)
                        {
                            int _matchIndex = -1;
                            int _emptyIndex = -1;

                            for (int i = 0; i < _equipmentData.Stacks.Count; i++)
                            {
                                if (_equipmentData.Stacks[i].isTagMatchText(item.LimitedByTag) && _matchIndex == -1)
                                {
                                    _matchIndex = i;
                                }
                                else if (_equipmentData.Stacks[i].isEmpty() && _emptyIndex == -1)
                                {
                                    _emptyIndex = i;
                                }

                            }
                            if (_matchIndex != -1 || _emptyIndex != -1)
                            {
                                Dictionary<Item, int> _swapDic = new Dictionary<Item, int>();
                                Dictionary<Item, int> _equipDic = new Dictionary<Item, int>();
                                _swapDic.Add(_inventoryData.Stacks[_inventorySlotIndex].Item, -_inventoryData.Stacks[_inventorySlotIndex].Number);
                                _equipDic.Add(_inventoryData.Stacks[_inventorySlotIndex].Item, _inventoryData.Stacks[_inventorySlotIndex].Number);
                                _inventoryData.Stacks[_inventorySlotIndex].Set(_equipmentData.Stacks[_matchIndex != -1 ? _matchIndex : _emptyIndex].Merge(_inventoryData.Stacks[_inventorySlotIndex]));
                                _equipmentData.ItemChanged(_equipDic);
                                _inventoryData.ItemChanged(_swapDic);
                                return;
                            }

                        }

                    }
                }
            }

        }


        /// <summary>
        /// Adds a specified number of items. Returns any items that could not be received (if any).
        /// </summary>
        /// <param name="_item"></param>
        /// <param name="_number"></param>
        /// <returns></returns>
        public InventoryStack AddItem(Item _item, int _number = 1)
        {
            if (!_item.visible)
            {
                return AddHiddenItem(_item, _number);
            }

            InventoryStack _newStack = new InventoryStack(_item, _number);
            //First find if there is any slot has same item to stack.
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (_newStack.Number > 0)
                {
                    if (Stacks[i].isSameItem(_item.id, _item.upgradeLevel, _item.enchantments, _item.socketedItems))
                    {
                        if (Stacks[i].Number < Stacks[i].Item.maxiumStack) _newStack = Stacks[i].Merge(_newStack);
                    }
                }
                else
                {
                    break;
                }
            }

            //Find empty slots to stack the rest of the items.
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (_newStack.Number > 0)
                {
                    if (Stacks[i].isEmpty())
                    {
                        _newStack = Stacks[i].Merge(_newStack);
                    }
                }
                else
                {
                    break;
                }
            }
            Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
            _changedItems.Add(_item, _number);
            ItemChanged(_changedItems);
            //return the items not been able to recieve due to the bag size. return 0 number stack if all items recieved
            if (_newStack.Number <= 0)
            {
                _newStack.Empty = true;
                _newStack.Item = null;
            }
            return _newStack;
        }


        /// <summary>
        /// Adds a specified number of hidden items. Returns any items that could not be received (if any).
        /// </summary>
        /// <param name="_item"></param>
        /// <param name="_number"></param>
        /// <returns></returns>
        public InventoryStack AddHiddenItem(Item _item, int _number = 1)
        {
            InventoryStack _newStack = new InventoryStack(_item, _number);
            //First find if there is any slot has same item to stack.
            bool _foundSame = false;
            for (int i = 0; i < HiddenStacks.Count; i++)
            {
                if (_newStack.Number > 0)
                {
                    if (HiddenStacks[i].isSameItem(_item.id, _item.upgradeLevel, _item.enchantments, _item.socketedItems))
                    {
                        _newStack = HiddenStacks[i].Merge(_newStack);
                        _foundSame = true;
                    }
                }
                else
                {
                    break;
                }
            }
            //Add new stack if we did not found any slot has same item .
            if (!_foundSame)
            {
                _newStack.Number = Mathf.Min(_newStack.Number, _item.maxiumStack);
                HiddenStacks.Add(_newStack.Copy());
                _newStack.Number = Mathf.Max(0, _number - _newStack.Number);
            }
            Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
            _changedItems.Add(_item, _number);
            ItemChanged(_changedItems);
            return _newStack;
        }


        /// <summary>
        /// Moves an item from one index to another.
        /// </summary>
        /// <param name="sourceIndex"></param>
        /// <param name="_targetIndex"></param>
        public void MoveItem(int sourceIndex, int _targetIndex)
        {
            Stacks[sourceIndex] = Stacks[_targetIndex].Merge(Stacks[sourceIndex]);
        }


        /// <summary>
        /// Splits the stack and returns the split stack.
        /// </summary>
        /// <param name="sourceIndex"></param>
        /// <param name="_number"></param>
        /// <returns></returns>
        public InventoryStack Split(int sourceIndex, int _number)
        {
            //return the temp InventoryStack took from the source
            return Stacks[sourceIndex].Split(_number);
        }


        /// <summary>
        /// Deletes an item with a specific UID, upgrade level, socketing and enchantments.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_upgradeLevel"></param>
        /// <param name="_enchantments"></param>
        /// <param name="_sockets"></param>
        public void DeleteItem(int _uid, int _upgradeLevel, List<int> _enchantments, List<int> _sockets)
        {
            bool _found = false;
            Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (!_found && Stacks[i].isSameItem(_uid, _upgradeLevel, _enchantments, _sockets))
                {
                    _changedItems.Add(Stacks[i].Item, -Stacks[i].Number);
                    Stacks[i].Delete();
                    _found = true;
                    break;
                }
            }
            if (_found) ItemChanged(_changedItems);
        }

        /// <summary>
        /// Returns the item matches the specific tag list, set _allMatch to false if you require the item matches any tag in the list, set _allMatch to true if require the item matches all tags in the list.
        /// </summary>
        /// <param name="_tags"></param>
        /// <param name="_allMatch"></param>
        /// <returns></returns>
        public Item GetItemByTag(List<string> _tags, bool _allMatch = true)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isTagsMatchList(_tags, _allMatch))
                {
                    return Stacks[i].Item;
                }
            }
            return null;
        }
        /// <summary>
        /// Returns the slot index of an item with a specific UID, upgrade level, socketing and enchantments.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_upgradeLevel"></param>
        /// <param name="_enchantments"></param>
        /// <param name="_sockets"></param>
        /// <returns></returns>
        public int GetItemIndex(int _uid, int _upgradeLevel, List<int> _enchantments, List<int> _sockets)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid, _upgradeLevel, _enchantments, _sockets))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the slot index of an item with a specific UID, upgrade level.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_upgradeLevel"></param>
        /// <returns></returns>
        public int GetItemIndex(int _uid, int _upgradeLevel)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid, _upgradeLevel))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the slot index of an item with a specific UID, if you have multiple items with same uid in your inventory, it will return the index of the first one.
        /// </summary>
        /// <param name="_uid"></param>
        /// <returns></returns>
        public int GetItemIndex(int _uid)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid))
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// Returns an item with a specific UID, upgrade level, socketing and enchantments.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_upgradeLevel"></param>
        /// <param name="_enchantments"></param>
        /// <param name="_sockets"></param>
        /// <returns></returns>
        public Item FindItem(int _uid, int _upgradeLevel, List<int> _enchantments, List<int> _sockets)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid, _upgradeLevel, _enchantments, _sockets))
                {
                    return Stacks[i].Item;
                }
            }
            for (int i = 0; i < HiddenStacks.Count; i++)
            {
                if (HiddenStacks[i].isSameItem(_uid, _upgradeLevel, _enchantments, _sockets))
                {
                    return HiddenStacks[i].Item;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an item with a specific UID, upgrade level.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_upgradeLevel"></param>
        /// <param name="_enchantments"></param>
        /// <param name="_sockets"></param>
        /// <returns></returns>
        public Item FindItem(int _uid, int _upgradeLevel)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid, _upgradeLevel))
                {
                    return Stacks[i].Item;
                }
            }
            for (int i = 0; i < HiddenStacks.Count; i++)
            {
                if (HiddenStacks[i].isSameItem(_uid, _upgradeLevel))
                {
                    return HiddenStacks[i].Item;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns an item with a specific UID.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_upgradeLevel"></param>
        /// <param name="_enchantments"></param>
        /// <param name="_sockets"></param>
        /// <returns></returns>
        public Item FindItem(int _uid)
        {
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (Stacks[i].isSameItem(_uid))
                {
                    return Stacks[i].Item;
                }
            }
            for (int i = 0; i < HiddenStacks.Count; i++)
            {
                if (HiddenStacks[i].isSameItem(_uid))
                {
                    return HiddenStacks[i].Item;
                }
            }
            return null;
        }


        /// <summary>
        /// Removes a specified number of items with a specific UID and index. Set _index to -1 if you don¡¯t know the index. Returns the total number of items removed.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_number"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        public int RemoveItem(int _uid, int _number, int _index = -1)
        {
            int _totalRemoved = 0;
            Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
            for (int i = 0; i < Stacks.Count; i++)
            {
                if (_number > 0)
                {
                    if (Stacks[i].isSameItem(_uid) && (_index == -1 || i == _index))
                    {
                        int _tookNumber = Mathf.Min(_number, Stacks[i].Number);
                        _totalRemoved += _tookNumber;
                        Stacks[i].AddNumber(-_tookNumber);
                        _number -= _tookNumber;
                    }
                }
                else
                {
                    break;
                }
            }
            for (int i = HiddenStacks.Count - 1; i >= 0; i--)
            {
                if (_number > 0)
                {
                    if (HiddenStacks[i].isSameItem(_uid))
                    {
                        int _tookNumber = Mathf.Min(_number, HiddenStacks[i].Number);
                        _totalRemoved += _tookNumber;
                        HiddenStacks[i].AddNumber(-_tookNumber);
                        _number -= _tookNumber;
                        if (HiddenStacks[i].Number <= 0) HiddenStacks.RemoveAt(i);
                    }
                }
                else
                {
                    break;
                }
            }
            _changedItems.Add(ItemObject.instance.TryGetItem(_uid).Copy(), -_totalRemoved);
            ItemChanged(_changedItems);
            // return the actual removed number of this item (the require number may larger than the number this holder has);
            return _totalRemoved;
        }

        /// <summary>
        /// Uses a specified number of items with a specific UID and index. Set _index to -1 if you don¡¯t know the index.
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_number"></param>
        /// <param name="_index"></param>
        public void UseItem(int _uid, int _number, int _index = -1)
        {
            if (ItemObject.instance.TryGetItem(_uid) != null)
            {
                if (Type == HolderType.PlayerInventory)
                {
                    if (!ItemObject.instance.TryGetItem(_uid).AbleToUse(GetInventoryDataByType(mEntity, HolderType.PlayerEquipment)))
                    {
                        if (ItemObject.instance.GetAtttibute(ItemObject.instance.TryGetItem(_uid).restrictionKey) != null) DynamicMsg.PopMsg(ItemObject.instance.msgItemUseRestricted.Replace("{name}", ItemObject.instance.GetAtttibute(ItemObject.instance.TryGetItem(_uid).restrictionKey).name).Replace("{value}", ItemObject.instance.TryGetItem(_uid).restrictionValue.ToString()));
                        return;
                    }
                }
                else if (Type == HolderType.NpcInventory)
                {
                    if (!ItemObject.instance.TryGetItem(_uid).AbleToUse(GetInventoryDataByType(mEntity, HolderType.NpcEquipment)))
                    {
                        if (ItemObject.instance.GetAtttibute(ItemObject.instance.TryGetItem(_uid).restrictionKey) != null) DynamicMsg.PopMsg(ItemObject.instance.msgItemUseRestricted.Replace("{name}", ItemObject.instance.GetAtttibute(ItemObject.instance.TryGetItem(_uid).restrictionKey).name).Replace("{value}", ItemObject.instance.TryGetItem(_uid).restrictionValue.ToString()));
                        return;
                    }
                }
                if (!ItemObject.instance.TryGetItem(_uid).useable) return;
                if (ItemObject.instance.TryGetItem(_uid).isCoolDown()) return;
                int _count = ItemObject.instance.TryGetItem(_uid).consumable ? RemoveItem(_uid, _number, _index) : 1;
                ItemObject.instance.TryGetItem(_uid).SetCoolDownTimeStamp();
                ItemObject.instance.SetSharedGlobalCoolDown(ItemObject.instance.SharedGlobalCoolDown);
                for (int i = 0; i < _count; i++)
                {
                    if (mItemUseCallbacks != null)
                    {
                        foreach (var _action in ItemObject.instance.TryGetItem(_uid).actions) mItemUseCallbacks(_action, _uid, _index);
                    }
                }
                Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
                _changedItems.Add(ItemObject.instance.TryGetItem(_uid).Copy(), -_count);
                ItemChanged(_changedItems);
            }
            else
            {
                Debug.LogError("Trying to use item with invalid item uid.");
            }
        }


    }
}
