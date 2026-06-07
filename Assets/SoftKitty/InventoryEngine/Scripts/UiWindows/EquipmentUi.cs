using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SoftKitty.InventoryEngine
{
    public class EquipmentUi : UiWindow
    {
        public RectTransform InventoryAnchor;
        public RectTransform HoverInfoAnchorPoint;
        public StatsUi StatsScript;
        public string EquipAction = "equip";
        public BlockByTag[] BlockSlotByTag;
        public InventoryItem[] EquipItems;
        public Text TitleText;
        public Text PlayerNameText;
        public Text PlayerLevelText;
        private Dictionary<string, InventoryItem> EquipSlotsDic = new Dictionary<string, InventoryItem>();
        [SerializeReference]
        private InventoryData mEquipHolder;
        [SerializeReference]
        private InventoryData mInventoryData;
        private bool inited = false;
        private bool wasDragging = false;

        public override void Initialize(InventoryData _inventoryHolder, InventoryData _equipHolder, string _name= "Equipment")
        {
            mEquipHolder = _equipHolder;
            mInventoryData = _inventoryHolder;
            mInventoryData.RegisterItemUseCallback(OnInventoryItemClick);
            StatsScript.Init(_equipHolder.EntityUid);
            TitleText.text = _name.ToUpper();
            if (ItemObject.instance.GetAtttibute(ItemObject.instance.LevelAttributeKey) !=null)
            {
                SetPlayerLevelText("Level . "+  _equipHolder.mEntity.GetAttributeInt(ItemObject.instance.LevelAttributeKey,true).ToString());
            }
            if (ItemObject.instance.GetAtttibute(ItemObject.instance.NameAttributeKey) != null)
            {
                SetPlayerName(_equipHolder.mEntity.GetAttributeString(ItemObject.instance.NameAttributeKey));
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(PlayerNameText.transform.parent.GetComponent<RectTransform>());
            EquipSlotsDic.Clear();
            int _index = 0;
            List<string> Tags = new List<string>();
            foreach (InventoryItem obj in EquipItems) {
                Tags.Add(obj.LimitedByTag);
                obj.Outline.color = ItemObject.instance.ItemSelectedColor;
                obj.Hover.color = ItemObject.instance.ItemHoverColor;
                obj.Fav.GetComponent<Image>().color = ItemObject.instance.FavoriteColor;
                if (!EquipSlotsDic.ContainsKey(obj.LimitedByTag))
                {
                    EquipSlotsDic.Add(obj.LimitedByTag, obj);
                    obj.RegisterClickCallback(_index, OnEquipItemClick);
                    obj.RegisterOnItemChangeCallback(OnItemChange);
                    obj.HoverInfoAnchorPoint = HoverInfoAnchorPoint;
                }
                obj.SetHolder(mEquipHolder);
                _index++;
            }

            _index = 0;
            foreach (InventoryStack item in mEquipHolder.Stacks)
            {
                bool _found = false;
                for (int i=Tags.Count-1;i>=0;i--)
                {
                    if (!_found && item.isTagMatchText(Tags[i])  )
                    {
                        EquipSlotsDic[Tags[i]].Initialize(item);
                        Tags.RemoveAt(i);
                        _found = true;
                    }
                }
                if (!_found && Tags.Count>0)
                {
                    EquipSlotsDic[Tags[0]].Initialize(item);
                    Tags.RemoveAt(0);
                }
            }
            ActiveWindow();
            SlotBlockCheck();
            inited = true;
        }

       

        public void SetPlayerName(string _name)
        {
            PlayerNameText.text = _name;
        }

        public void SetPlayerLevelText(string _text)
        {
            PlayerLevelText.text = _text;
        }

        public void HighLightSlot(List<string> _tags)
        {
            foreach (InventoryItem item in EquipItems)
            {
                bool _match = false;
                foreach (var _tag in _tags)
                {
                    if (item != null && item.LimitedByTag.Replace("#1", "").Replace("#2", "").Replace("#3", "") == _tag) _match = true;
                }
                if (_match && item != null)
                {
                    item.ShowOutline(0.5F);
                }
            }
        }

        public void ToggleInventory()
        {
            if (!WindowsManager.isWindowOpen(mInventoryData))
            {
                UiWindow _inventory = mInventoryData.OpenWindow();
                _inventory.GetComponentInChildren<DragableUi>().DragableRectTransform = GetComponent<RectTransform>();
                _inventory.ChildWindow = true;
                _inventory.GetComponent<UiStyle>().SetHeight(0, 0.5F);
                _inventory.GetComponent<UiStyle>().SetHeight(1, 0.5F);
                ActiveWindow();
            }
            else
            {
                WindowsManager.getOpenedWindow(mInventoryData).Close();
            }
        }

        private void OnDestroy()
        {
            mInventoryData.UnRegisterItemUseCallback(OnInventoryItemClick);
            if (WindowsManager.isWindowOpen(mInventoryData))
            {
                WindowsManager.getOpenedWindow(mInventoryData).GetComponentInChildren<DragableUi>().DragableRectTransform = WindowsManager.getOpenedWindow(mInventoryData).GetComponent<RectTransform>();
                WindowsManager.getOpenedWindow(mInventoryData).ActiveWindow();
            }
        }

        public void OnItemChange(Item _item, int _changedNumber, InventoryItem _itemIcon)
        {
            SlotBlockCheck();
        }

        private void SlotBlockCheck()
        {
            StartCoroutine(SlotBlockCheckCo());
            
        }

        IEnumerator SlotBlockCheckCo()
        {
            yield return 1;
            foreach (var obj in BlockSlotByTag)
            {
                bool _found = false;
                foreach (InventoryItem item in EquipItems)
                {
                    if (!_found && item != null && item.isTagMatchText(obj.tag) && !item.isEmpty())
                    {
                        obj.blockIcon.SetAppearance(item.GetItem().GetIcon(), item.GetItem().GetTypeColor(), item.GetItem().GetQualityColor(), false, true);
                        obj.blockIcon.SetUpgradeLevel(item.GetItem().upgradeLevel);
                        _found = true;
                    }
                }
                obj.blockIcon.gameObject.SetActive(_found);
                obj.blockedItem.Enabled = !_found;
                if (_found && !obj.blockedItem.isEmpty()) UnEquip(obj.blockedItem.LimitedByTag, obj.blockedItem.GetItemId());
            }
        }

        public void OnEquipItemClick(int _id, int _button)
        {
            if (_button == 1 && !EquipItems[_id].isEmpty())
            {
                UnEquip(EquipItems[_id].LimitedByTag, EquipItems[_id].GetItemId());
            }
        }

        public void OnInventoryItemClick(string _action, int _id, int _index)
        {
            if (_index == -1) return;
            if (_action==EquipAction) {
                foreach (InventoryItem item in EquipItems)
                {
                    if (ItemObject.instance.TryGetItem(_id).isTagMatchText(item.LimitedByTag) && item.Enabled)
                    {
                        Equip(item.LimitedByTag, _id,item, _index);
                        return;
                    }
                }
            }
        }



        public void UnEquip(string _tag, int _uid, bool _playSound = true)
        {
            bool _found = false;
            for (int i=mEquipHolder.Stacks.Count-1;i>=0;i--)
            {
                if (!_found
                    && mEquipHolder.Stacks[i].isTagMatchText(_tag)
                    && mEquipHolder.Stacks[i].GetItemId()==_uid)
                {
                    Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
                    _changedItems.Add(mEquipHolder.Stacks[i].Item, -mEquipHolder.Stacks[i].Number);
                    InventoryStack _add;
#if MASTER_INVENTORY_ENGINE
                    if (WindowsManager.isWindowOpen(ItemObject.PlayerInventoryData) || !WindowsManager.isWindowOpen(mInventoryData))
                    {
                        _add = ItemObject.PlayerInventoryData.AddItem(mEquipHolder.Stacks[i].Item.Copy(), mEquipHolder.Stacks[i].Number);
                    }
                    else
                    {
                        _add = mInventoryData.AddItem(mEquipHolder.Stacks[i].Item.Copy(), mEquipHolder.Stacks[i].Number);
                    }

                    if (_add.Number <= 0)
                    {
                        mEquipHolder.Stacks[i].Delete();
                        mEquipHolder.ItemChanged(_changedItems);
                    }
                    _found = true;
#endif
                }
            }
            if(_playSound) SoundManager.Play2D("EquipOff");
        }

        public void Equip(string _tag, int _uid, InventoryItem _equipSlot, int _index)
        {
            UnEquip(_tag, _uid,false);
            Dictionary<Item, int> _changedItems = new Dictionary<Item, int>();
            _changedItems.Add(mInventoryData.Stacks[_index].Item, mInventoryData.Stacks[_index].Number);
            if ( mInventoryData.Stacks[_index].GetItemId() == _uid)
            {
                mInventoryData.Stacks[_index].Set(_equipSlot.StackData.Merge(mInventoryData.Stacks[_index]));
            }
            mEquipHolder.ItemChanged(_changedItems);
            SoundManager.Play2D("EquipOn");
           
        }


        public override void Update()
        {
            if (!inited) return;

            if (WindowsManager.isWindowOpen(mInventoryData))
            {
               if (WindowsManager.getOpenedWindow(mInventoryData).GetComponent<ItemContainer>()) 
                    WindowsManager.getOpenedWindow(mInventoryData).transform.position = InventoryAnchor.position;
            }

            if (ItemDragManager.isDragging)
            {
                if (!ItemDragManager.DraggingSource.transform.IsChildOf(transform) && ItemDragManager.DraggingSource.Type== ItemIcon.IconType.Item)
                {
                    foreach (var obj in EquipItems) {
                        if (ItemDragManager.DraggingSource.isTagMatchText(obj.LimitedByTag))
                        {
                            wasDragging = true;
                            obj.ToggleOutline(true);
                        }
                    }
                }
            }
            else
            {
                if (wasDragging)
                {
                    wasDragging = false;
                    foreach (var obj in EquipItems) obj.ToggleOutline(false);
                }
            }

            base.Update();
        }
    }
}
