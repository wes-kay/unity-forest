using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.InventoryEngine
{
    [System.Serializable]
    public class LootPackData
    {
        public string uid = "LootPack0000001";
        public GameObject VFX;
        public List<int> ItemPool = new List<int>();
        public List<int> CurrencyMin = new List<int>();
        public List<int> CurrencyMax = new List<int>();
        public int MaxiumItemCount = 5;
        public int MaxiumCountEachItem = 3;
        public float DropChanceMultiplier = 1F;
        public bool RandomLevel = false;
        public bool DestoryWhenPlayerCloseLootWindow = true;
        public bool RandomEnchantment = false;
        public List<int> EnchantmentPool = new List<int>();
        public int MaxiumEnhancingLevel = 10;
        public bool fold = false;
        public bool currencyExpand = false;
        public bool itemExpand = false;

        public LootPackData Copy()
        {
            LootPackData _copy = new LootPackData();
            _copy.uid = uid;
            _copy.ItemPool = new List<int>();
            for (int i = 0; i < ItemPool.Count; i++) _copy.ItemPool.Add(ItemPool[i]);
            _copy.CurrencyMin = new List<int>();
            for (int i = 0; i < CurrencyMin.Count; i++) _copy.CurrencyMin.Add(CurrencyMin[i]);
            _copy.CurrencyMax = new List<int>();
            for (int i = 0; i < CurrencyMax.Count; i++) _copy.CurrencyMax.Add(CurrencyMax[i]);
            _copy.MaxiumItemCount = MaxiumItemCount;
            _copy.MaxiumCountEachItem = MaxiumCountEachItem;
            _copy.DropChanceMultiplier = DropChanceMultiplier;
            _copy.RandomLevel = RandomLevel;
            _copy.DestoryWhenPlayerCloseLootWindow = DestoryWhenPlayerCloseLootWindow;
            _copy.RandomEnchantment = RandomEnchantment;
            _copy.EnchantmentPool = new List<int>();
            for (int i = 0; i < EnchantmentPool.Count; i++) _copy.EnchantmentPool.Add(EnchantmentPool[i]);
            _copy.MaxiumEnhancingLevel = MaxiumEnhancingLevel;
            return _copy;
        }
    }


    public class LootPack : MonoBehaviour
    {
        #region Variables
        private LootPackData mData;
        public InventoryData mHolder;
        private LootUi lootWindow;
        private bool inited = false;
        private bool windowWasOpen = false;
        #endregion

        #region Internal Methods
        public static void GetRandomLoot(InventoryData _holder, List<int> _itemIdPool, int[] _currencyMinArray, int[] _currencyMaxArray, List<int> _enchantmentPool, int _maxiumItemCount = 5, float _dropChanceMultiplier = 1F, bool _randomLevel=false, int _maxiumCountEachItem = 3, int _maxiumEnhancingLevel=1,  bool _randomEnchantments=false )
        {
            List<Item> _dropList = new List<Item>();
            Random.InitState(System.DateTime.Now.Millisecond + System.DateTime.Now.Hour * 10000 + System.DateTime.Now.Minute * 1000);
            foreach (int obj in _itemIdPool)
            {
                if (ItemObject.instance.TryGetItem(obj)!=null && Random.Range(0, 100) < ItemObject.instance.TryGetItem(obj).dropRates * _dropChanceMultiplier)
                {
                    Item _newItem = new Item(obj, false, true);
                    if (ItemObject.instance.EnableEnchanting && _randomEnchantments && ItemObject.instance.enchantmentDic.Count > 0 
                        && _newItem.type== ItemObject.instance.EnchantingCategoryID && Random.Range(0, 100) < ItemObject.instance.EnchantingSuccessRate)
                    {
                        List<int> _newEnchantments = new List<int>();
                        int _count = Random.Range(Mathf.FloorToInt(ItemObject.instance.EnchantmentNumberRange.x), Mathf.FloorToInt(ItemObject.instance.EnchantmentNumberRange.y));
                        for (int i = 0; i < _count; i++)
                        {
                            int _uid = Random.Range(0, ItemObject.instance.enchantmentDic.Count);
                            if (_enchantmentPool.Count > 0) _uid = _enchantmentPool[Random.Range(0,_enchantmentPool.Count)];
                            if (!_newEnchantments.Contains(_uid) && ItemObject.instance.TryGetEnchantmentById(_uid) != null) _newEnchantments.Add(_uid);
                        }
                        _newItem.enchantments.Clear();
                        for (int i = 0; i < _newEnchantments.Count; i++)
                        {
                            _newItem.AddEnchantment(_newEnchantments[i]);
                        }
                    }
                    if (_randomLevel && ItemObject.instance.EnableEnhancing && _newItem.type == ItemObject.instance.EnhancingCategoryID)
                    {
                        int _level = Random.Range(0, Mathf.Clamp(_maxiumEnhancingLevel, 1, ItemObject.instance.MaxiumEnhancingLevel));
                        if (Random.Range(0, 100) < ItemObject.instance.EnhancingSuccessCurve.Evaluate(_level*1F/ ItemObject.instance.MaxiumEnhancingLevel)) {
                            _newItem.upgradeLevel = _level;
                        }
                    }
                    _dropList.Add(_newItem);
                }
            }

            if (_dropList.Count <= 0)
            {
                Item _newItem = new Item(_itemIdPool[Random.Range(0, _itemIdPool.Count)], _randomEnchantments, true);
                if (_randomLevel && ItemObject.instance.EnableEnhancing && _newItem.type == ItemObject.instance.EnhancingCategoryID)
                {
                    int _level = Random.Range(0, Mathf.Clamp(_maxiumEnhancingLevel, 1, ItemObject.instance.MaxiumEnhancingLevel));
                    if (Random.Range(0, 100) < ItemObject.instance.EnhancingSuccessCurve.Evaluate(_level * 1F / ItemObject.instance.MaxiumEnhancingLevel))
                    {
                        _newItem.upgradeLevel = _level;
                    }
                }
                _dropList.Add(_newItem);
            }

            _dropList.Sort(SortByRandom);
            int _dropCount = Mathf.Min(_dropList.Count, Random.Range(1, _maxiumItemCount + 1));
            _holder.InventorySize = _dropCount;
            for (int i = 0; i < _dropCount; i++)
            {
                _holder.Stacks.Add(new InventoryStack());
            }
            for (int i = 0; i < _dropCount; i++)
            {
                _holder.AddItem(_dropList[i], Random.Range(1, Mathf.Min(_dropList[i].maxiumStack, _maxiumCountEachItem) + 1));
            }
            for (int i = 0; i < Mathf.Min(_currencyMinArray.Length, _currencyMaxArray.Length, ItemObject.instance.currencies.Count); i++)
            {
                _holder.AddCurrency(i, Random.Range(_currencyMinArray[i], _currencyMaxArray[i]));
            }
        }

        private static int SortByRandom(Item a, Item b)
        {
            return Random.Range(0, 100).CompareTo(Random.Range(0, 100));
        }

        private void Update()
        {
            if (!inited) return;
            if (lootWindow != null)
            {
                windowWasOpen = true;
                if (lootWindow.isEmpty()) mData.DestoryWhenPlayerCloseLootWindow = true;
            }
            else
            {
                if (windowWasOpen && mData.DestoryWhenPlayerCloseLootWindow) DestroyPack();
            }
        }
        public void UpdatePrefab()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }


        public void Init(LootPackData _data)
        {
            mData = _data.Copy();
            mHolder=new InventoryData();
            mHolder.Name = mData.uid;
            if (mData.EnchantmentPool == null) mData.EnchantmentPool = new List<int>();
            GetRandomLoot(mHolder, mData.ItemPool, mData.CurrencyMin.ToArray(), mData.CurrencyMax.ToArray(), mData.EnchantmentPool, mData.MaxiumItemCount, mData.DropChanceMultiplier, mData.RandomLevel, mData.MaxiumCountEachItem, mData.MaxiumEnhancingLevel, mData.RandomEnchantment);
            if (mData.VFX != null)
            {
                GameObject _newVFX = Instantiate(mData.VFX,transform);
                _newVFX.transform.localPosition = Vector3.zero;
                _newVFX.transform.localEulerAngles = Vector3.zero;
                _newVFX.transform.localScale = Vector3.one;
                _newVFX.SetActive(true);
            }
            inited = true;
        }
        #endregion



        //Call this to open the interface of this loot pack.
        public void OpenPack()
        {
            if (!inited) return;
            if (lootWindow == null)
            {
                lootWindow = LootUi.ShowLoot(mHolder);
            }
            
        }

        //Destroy this loot pack
        public void DestroyPack()
        {
             Destroy(gameObject);
        }

       

    }
}
