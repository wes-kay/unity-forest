using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SoftKitty.InventoryEngine
{
    public class InventoryInspectorModule : IEntityInspectorModule
    {
        public static List<InventoryEngine.InventoryData> CopiedInventory = new List<InventoryEngine.InventoryData>();
        public static List<string> CopiedLootPack = new List<string>();
        private ItemEditorAssets EditorAsset;
        private int _tradeCategoryType = 0;
        private int _tradeItemId = 0;
        private int _tradeType = 0;
        private string _search = "Search item by name";
        private Dictionary<int, Texture2D> itemIconDic = new Dictionary<int, Texture2D>();
        private Dictionary<int, Item> ItemDic = new Dictionary<int, Item>();
        private Dictionary<int, int> ItemIndex = new Dictionary<int, int>();
        private List<string> ItemNames = new List<string>();
        private string[] ItemOptions = new string[0];
        private string[] EnchantOptions = new string[0];
        private string ItemDataHash;
        private int SelLootPackUid = 0;

        private Texture2D GetIcon(int _id)
        {
            if (EditorAsset != null && ItemObject.instance != null)
            {
                if (_id >= 0)
                {
                    Item _item = ItemObject.instance.GetItem(_id);
                    if (_item != null)
                    {
                        Texture2D _icon = null;
                        if (_item.iconLoadMethod == LoadMethod.DirectReference)
                        {
                            _icon= _item.icon;
                        }
                        else 
                        {
                            _icon= (Texture2D)EditorAsset.GetIcon(_id);
                        }
                        if (_item.icon != null)
                        {
                            _icon= _item.icon;
                        }
                        return _icon;
                    }
                }
            }
            return null;
        }
        private bool ItemExist(int _id)
        {
            return ItemIndex.ContainsKey(_id);
        }

        public void DrawRuntimeInspector(EntityComponent myTarget)
        {
            if (ItemObject.instance == null)
            {

                EditorUtils.Warnning("Item Database Object is not assigned, You can create this object via the context menu in any Project folder:" +
                "<color=6688FF>Create ¡ú SoftKitty ¡ú Data Objects ¡ú Item Data</color>" +
                "You can assign the created asset to the database in: <color=6688FF>Project Settings ¡ú SoftKitty ¡ú Data Settings ¡ú Data</color>", 10);

                GUILayout.BeginHorizontal();
                GUI.backgroundColor = EditorUtils._buttonColor;
                if (GUILayout.Button("Assign One"))
                {
                    SGD_SettingsProvider.CurrentSettings.data_expand = true;
                    SettingsService.OpenProjectSettings("Project/SoftKitty/SubData - Items");
                }
                EditorUtils.ResetColor();
                GUILayout.EndHorizontal();
            }
            else
            {
                DisplayItems(myTarget);
            }
        }

        private string CutString(string _text, int _length)
        {
            return _text.Substring(0, Mathf.Min(_text.Length, _length)) + (_text.Length > _length ? ".." : "");
        }

        private void DisplayItems(EntityComponent myTarget)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Inventories: (" + myTarget.mData.GetModule<InventoryModule>().Inventory.Count.ToString() + ")", EditorUtils._boldStyle);
            GUILayout.EndHorizontal();
            for (int i = 0; i < myTarget.mData.GetModule<InventoryModule>().Inventory.Count; i++)
            {
                EditorUtils.TitleButton(new GUIContent(myTarget.mData.GetModule<InventoryModule>().Inventory[i].Name + " (" + myTarget.mData.GetModule<InventoryModule>().Inventory[i].Type + ")", "Click to expand the inventory."),
                    ref myTarget.mData.GetModule<InventoryModule>().Inventory[i].runtimeFold, 10, false, 10, false);
                if (myTarget.mData.GetModule<InventoryModule>().Inventory[i].runtimeFold)
                {

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    for (int u = 0; u < ItemObject.instance.currencies.Count; u++)
                    {
                        if (myTarget.mData.GetModule<InventoryModule>().Inventory[i].Currency.Count > u)
                        {
                            var _currency = ItemObject.instance.currencies[u];
                            if (_currency.icon != null)
                            {
                                GUILayout.Box(new GUIContent(_currency.icon.texture, _currency.name), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20));
                            }
                            else
                            {
                                GUILayout.Box("-", GUILayout.Width(20), GUILayout.Height(20));
                            }
                            GUI.color = _currency.color;
                            GUILayout.Label(myTarget.mData.GetModule<InventoryModule>().Inventory[i].Currency.GetCurrency(u).ToString(), GUILayout.Width(80));
                            EditorUtils.ResetColor();
                        }
                    }
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Separator();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUI.backgroundColor = Color.black;
                    GUILayout.Box("Name", EditorUtils._boxStyle, GUILayout.Width(150), GUILayout.Height(17));
                    GUILayout.Box("Category", EditorUtils._boxStyle, GUILayout.Width(130), GUILayout.Height(17));
                    GUILayout.Box("ID", EditorUtils._boxStyle, GUILayout.Width(70), GUILayout.Height(17));
                    GUILayout.Box("Count", EditorUtils._boxStyle, GUILayout.Width(50), GUILayout.Height(17));
                    GUILayout.Box("Socketing", EditorUtils._boxStyle, GUILayout.Width(70), GUILayout.Height(17));
                    if (ItemObject.instance.EnableEnchanting) GUILayout.Box("Enchantment", EditorUtils._boxStyle, GUILayout.Width(100), GUILayout.Height(17));
                    GUILayout.EndHorizontal();

                    foreach (var _data in myTarget.mData.GetModule<InventoryModule>().Inventory[i].Stacks)
                    {
                        if (!ItemObject.instance.ItemExist(_data.GetItemId()))
                        {
                            continue;
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        bool _empty = _data.isEmpty();
                        string _enchantmentName = "";
                        string _nameHint = _empty ? "" : "[" + _data.Item.name + " ]" + (_data.Item.upgradeLevel > 0 ? "  ( Enhanced Level:" + _data.Item.upgradeLevel + " )" : "");
                        if (!_empty && ItemObject.instance.EnableEnchanting)
                        {
                            for (int u = 0; u < _data.Item.enchantments.Count; u++)
                            {
                                if (ItemObject.instance.TryGetEnchantmentById(_data.Item.enchantments[u]) != null)
                                {
                                    _nameHint += "\n" + ItemObject.instance.TryGetEnchantmentById(_data.Item.enchantments[u]).GetDescription();
                                    _enchantmentName += "(" + _data.Item.enchantments[u] + ")";
                                }
                            }
                        }

                        GUI.backgroundColor = _empty ? EditorUtils._black : ItemObject.instance.TryGetItemQualityById(_data.Item.quality).color;
                        GUILayout.Button(new GUIContent(_empty ? "-" : (CutString(_data.Item.name, 16) + (_data.Item.upgradeLevel > 0 ? "+" + _data.Item.upgradeLevel : "")), _empty ? "" : _nameHint), EditorUtils._boxStyle, GUILayout.Width(150), GUILayout.Height(17));
                        GUI.backgroundColor = _empty ? EditorUtils._black : ItemObject.instance.TryGetItemTypesById(_data.Item.type).color;
                        GUILayout.Box(_empty ? "" : CutString(ItemObject.instance.TryGetItemTypesById(_data.Item.type).name, 12), EditorUtils._boxStyle, GUILayout.Width(130), GUILayout.Height(17));
                        GUI.backgroundColor = _empty ? EditorUtils._black : Color.gray;
                        if (GUILayout.Button(new GUIContent(_empty ? "" : _data.Item.id.ToString(), _empty ? "" : "UID: " + _data.Item.uid), EditorUtils._boxStyle, GUILayout.Width(70), GUILayout.Height(17)) && !_empty)
                        {
                            GUIUtility.systemCopyBuffer = _data.Item.id.ToString();
                            EditorUtils.ShowBoxNotification("id Copied to Clipboard!");
                        }
                        GUI.backgroundColor = _empty ? EditorUtils._black : EditorUtils._titleColor;
                        GUILayout.Button(new GUIContent(_empty ? "" : _data.Number.ToString(), _empty ? "" : "Max Stack: " + _data.Item.maxiumStack.ToString()), EditorUtils._boxStyle, GUILayout.Width(50), GUILayout.Height(17));
                        if (ItemObject.instance.EnableSocketing)
                        {
                            GUI.backgroundColor = _empty ? EditorUtils._black : EditorUtils._gray;
                            string _socketingString = "";
                            string _socketingDetailString = "";
                            if (!_empty)
                            {
                                for (int w = 0; w < _data.Item.socketedItems.Count; w++)
                                {
                                    if (_data.Item.socketedItems[w] == -2)
                                    {
                                        _socketingString += "X";
                                        _socketingDetailString += " [Locked] ";
                                    }
                                    else if (_data.Item.socketedItems[w] == -1)
                                    {
                                        _socketingString += "<color=#888888>O</color>";
                                        _socketingDetailString += " [Available] ";
                                    }
                                    else
                                    {
                                        _socketingString += "<color=#22FF55>O</color>";
                                        InventoryEngine.Item _socketed = ItemObject.instance.TryGetItem(_data.Item.socketedItems[w]);
                                        _socketingDetailString += " [" + _socketed != null ? _socketed.name : "Invalid Item" + "] ";
                                    }
                                }
                            }
                            GUILayout.Button(new GUIContent(_empty ? "" : _socketingString, _empty ? "" : _socketingDetailString), EditorUtils._boxRichStyle, GUILayout.Width(70), GUILayout.Height(17));
                        }
                        else
                        {
                            GUI.backgroundColor = EditorUtils._black;
                            GUILayout.Box("Disabled", EditorUtils._boxStyle, GUILayout.Width(70), GUILayout.Height(17));
                        }
                        EditorUtils.ResetColor();
                        if (ItemObject.instance.EnableEnchanting)
                        {
                            GUI.backgroundColor = _empty ? EditorUtils._black : EditorUtils._gray;
                            GUILayout.Button(new GUIContent(_empty ? "-" : _enchantmentName, _empty ? "" : _nameHint), EditorUtils._boxStyle, GUILayout.Width(100), GUILayout.Height(17));
                        }
                        GUILayout.Space(20);
                        GUILayout.EndHorizontal();

                    }

                    if (myTarget.mData.GetModule<InventoryModule>().Inventory[i].HiddenStacks.Count > 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUI.backgroundColor = EditorUtils._black;
                        GUILayout.Box("Hidden Items", EditorUtils._titleButtonStyle, GUILayout.Width(600));
                        GUILayout.Space(20);
                        EditorUtils.ResetColor();
                        GUILayout.EndHorizontal();

                        foreach (var _data in myTarget.mData.GetModule<InventoryModule>().Inventory[i].HiddenStacks)
                        {
                            if (!ItemObject.instance.ItemExist(_data.GetItemId()))
                            {
                                continue;
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(20);
                            bool _empty = _data.isEmpty();
                            string _nameHint = _empty ? "" : "[" + _data.Item.name + " ]" + (_data.Item.upgradeLevel > 0 ? "  ( Enhanced Level:" + _data.Item.upgradeLevel + " )" : "");
                            string _enchantmentName = "";
                            if (!_empty && ItemObject.instance.EnableEnchanting)
                            {
                                for (int u = 0; u < _data.Item.enchantments.Count; u++)
                                {
                                    if (ItemObject.instance.TryGetEnchantmentById(_data.Item.enchantments[u]) != null)
                                    {
                                        _nameHint += "\n" + ItemObject.instance.TryGetEnchantmentById(_data.Item.enchantments[u]).GetDescription();
                                        _enchantmentName += "(" + _data.Item.enchantments[u] + ")";
                                    }
                                }
                            }
                            GUI.backgroundColor = _empty ? EditorUtils._black : ItemObject.instance.TryGetItemQualityById(_data.Item.quality).color;
                            GUILayout.Button(new GUIContent(_empty ? "-" : (CutString(_data.Item.name, 16) + (_data.Item.upgradeLevel > 0 ? "+" + _data.Item.upgradeLevel : "")), _nameHint), EditorUtils._boxStyle, GUILayout.Width(150), GUILayout.Height(17));
                            GUI.backgroundColor = _empty ? EditorUtils._black : ItemObject.instance.TryGetItemTypesById(_data.Item.type).color;
                            GUILayout.Box(_empty ? "" : CutString(ItemObject.instance.TryGetItemTypesById(_data.Item.type).name, 12), EditorUtils._boxStyle, GUILayout.Width(130), GUILayout.Height(17));
                            GUI.backgroundColor = _empty ? EditorUtils._black : Color.gray;
                            if (GUILayout.Button(new GUIContent(_empty ? "" : _data.Item.id.ToString(), _empty ? "" : "UID: " + _data.Item.uid), EditorUtils._boxStyle, GUILayout.Width(70), GUILayout.Height(17)) && !_empty)
                            {
                                GUIUtility.systemCopyBuffer = _data.Item.id.ToString();
                                EditorUtils.ShowBoxNotification("id Copied to Clipboard!");
                            }
                            GUI.backgroundColor = _empty ? EditorUtils._black : EditorUtils._titleColor;
                            GUILayout.Button(new GUIContent(_empty ? "" : _data.Number.ToString(), _empty ? "" : "Max Stack: " + _data.Item.maxiumStack.ToString()), EditorUtils._boxStyle, GUILayout.Width(50), GUILayout.Height(17));
                            if (ItemObject.instance.EnableSocketing)
                            {
                                GUI.backgroundColor = _empty ? EditorUtils._black : EditorUtils._gray;
                                string _socketingString = "";
                                string _socketingDetailString = "";
                                if (!_empty)
                                {
                                    for (int w = 0; w < _data.Item.socketedItems.Count; w++)
                                    {
                                        if (_data.Item.socketedItems[w] == -2)
                                        {
                                            _socketingString += "X";
                                            _socketingDetailString += " [Locked] ";
                                        }
                                        else if (_data.Item.socketedItems[w] == -1)
                                        {
                                            _socketingString += "<color=#888888>O</color>";
                                            _socketingDetailString += " [Available] ";
                                        }
                                        else
                                        {
                                            _socketingString += "<color=#22FF55>O</color>";
                                            InventoryEngine.Item _socketed = ItemObject.instance.TryGetItem(_data.Item.socketedItems[w]);
                                            _socketingDetailString += " [" + _socketed != null ? _socketed.name : "Invalid Item" + "] ";
                                        }
                                    }
                                }
                                GUILayout.Button(new GUIContent(_empty ? "" : _socketingString, _empty ? "" : _socketingDetailString), EditorUtils._boxRichStyle, GUILayout.Width(70), GUILayout.Height(17));
                            }
                            else
                            {
                                GUI.backgroundColor = EditorUtils._black;
                                GUILayout.Box("Disabled", EditorUtils._boxStyle, GUILayout.Width(70), GUILayout.Height(17));
                            }
                            EditorUtils.ResetColor();
                            if (ItemObject.instance.EnableEnchanting)
                            {
                                GUI.backgroundColor = _empty ? EditorUtils._black : EditorUtils._gray;
                                GUILayout.Button(new GUIContent(_empty ? "-" : _enchantmentName, _empty ? "" : _nameHint), EditorUtils._boxStyle, GUILayout.Width(100), GUILayout.Height(17));
                            }
                            GUILayout.Space(20);
                            GUILayout.EndHorizontal();

                        }
                    }

                    EditorGUILayout.Separator();

                }
            }
        }

        public bool DrawInspector(Entity _entity, EntityManagerObject _myTarget)
        {
            if (EditorAsset == null) EditorAsset = (ItemEditorAssets)AssetDatabase.LoadAssetAtPath(ItemObject_Inspector.InventoryEngineEditorPath + "ItemEditorAssets.asset", typeof(ItemEditorAssets));

            EditorGUILayout.LabelField("Inventory Module:", EditorStyles.boldLabel);
            var _inventoryModule = _entity.GetModule<InventoryModule>();
            if (_inventoryModule == null)
            {
                _inventoryModule = (InventoryModule)System.Activator.CreateInstance(typeof(InventoryModule));
                _entity.AddModule(_inventoryModule);
                foreach (var obj in _entity.Modules) obj.Save();
                EditorUtility.SetDirty(_myTarget);
                AssetDatabase.SaveAssets();
                return true;
            }

            if (ItemObject.instance == null)
            {

                EditorUtils.Warnning("Item Database Object is not assigned, You can create this object via the context menu in any Project folder:" +
                "<color=6688FF>Create ¡ú SoftKitty ¡ú Data Objects ¡ú Item Data</color>" +
                "You can assign the created asset to the database in: <color=6688FF>Project Settings ¡ú SoftKitty ¡ú Data Settings ¡ú Data</color>", 10);
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = EditorUtils._buttonColor;
                if (GUILayout.Button("Assign One"))
                {
                    SGD_SettingsProvider.CurrentSettings.data_expand = true;
                    SettingsService.OpenProjectSettings("Project/SoftKitty/SubData - Items");
                }
                EditorUtils.ResetColor();
                GUILayout.EndHorizontal();
                return false;
            }

            bool _valueChanged = false;
            if (!SGD_Settings.isRuntime && ItemObject.instance != null &&
                (itemIconDic.Count == 0 || ItemNames.Count == 0 || ItemIndex.Count == 0 ||
                ItemDic.Count == 0 || ItemOptions.Length == 0 || ItemDataHash != ItemObject.instance.Hash))
            {
                ItemDataHash = ItemObject.instance.Hash;
                itemIconDic.Clear();
                ItemNames.Clear();
                ItemIndex.Clear();
                ItemDic.Clear();
                ItemOptions = new string[ItemObject.instance.ItemCount + 1];
                EnchantOptions = new string[ItemObject.instance.itemEnchantments.Count];
                ItemOptions[0] = "Empty";
                for (int i = 0; i < ItemObject.instance.ItemCount; i++)
                {
                    if (!itemIconDic.ContainsKey(ItemObject.instance.GetItemByIndex(i).id)) itemIconDic.Add(ItemObject.instance.GetItemByIndex(i).id, GetIcon(ItemObject.instance.GetItemByIndex(i).id));
                    ItemNames.Add(ItemObject.instance.GetItemByIndex(i).name);
                    if (!ItemIndex.ContainsKey(ItemObject.instance.GetItemByIndex(i).id)) ItemIndex.Add(ItemObject.instance.GetItemByIndex(i).id, i);
                    if (!ItemDic.ContainsKey(ItemObject.instance.GetItemByIndex(i).id)) ItemDic.Add(ItemObject.instance.GetItemByIndex(i).id, ItemObject.instance.GetItemByIndex(i));
                    ItemOptions[i + 1] = ItemObject.instance.GetItemByIndex(i).name;
                }
                for (int i = 0; i < ItemObject.instance.itemEnchantments.Count; i++) EnchantOptions[i] = ItemObject.instance.itemEnchantments[i].name;
            }



            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._buttonColor * 0.6F;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));

            if (GUILayout.Button(EditorUtils.TextHint((_inventoryModule.inventoryFold ? "-" : "+") + " Inventory: (" + _inventoryModule.Inventory.Count + ")", "Inventory of this entity."), EditorUtils._titleButtonStyle))
            {
                _inventoryModule.inventoryFold = !_inventoryModule.inventoryFold;
                _valueChanged = true;
            }
            GUI.backgroundColor = EditorUtils._titleColor;
            if (GUILayout.Button(new GUIContent("+", "Add a new inventory."), GUILayout.Width(20)))
            {
                InventoryData _holder = new InventoryData();
                _holder.Name = "New Inventory";
                _holder.EntityUid = _entity.uid;
                _inventoryModule.Inventory.Add(_holder);
                EditorGUI.FocusTextInControl(null);
                _valueChanged = true;
            }
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Help), "Opens the official online documentation, including guides, node reference, and examples."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
            {
                Application.OpenURL("https://www.soft-kitty.com/docs/master-inventory-engine/item-class/item-data");
                EditorGUI.FocusTextInControl(null);
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy the Inventory data."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
            {
                CopiedInventory.Clear();
                for (int i = 0; i < _inventoryModule.Inventory.Count; i++) CopiedInventory.Add(_inventoryModule.Inventory[i].Copy());
                EditorGUI.FocusTextInControl(null);
                EditorUtils.ShowBoxNotification("Inventory Data copied to clipboard!");
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste the Inventory list."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
            {
                if (CopiedInventory.Count > 0)
                {
                    _inventoryModule.Inventory.Clear();
                    for (int i = 0; i < CopiedInventory.Count; i++)
                    {
                        InventoryData _holder = CopiedInventory[i].Copy();
                        _holder.EntityUid = _entity.uid;
                        _inventoryModule.Inventory.Add(_holder);
                    }
                    EditorGUI.FocusTextInControl(null);
                    _valueChanged = true;
                }
            }
            GUILayout.EndHorizontal();
            EditorUtils.End(10);

            if (_inventoryModule.inventoryFold)
            {
                EditorUtils.HelpInfo("Each [Inventory] is a unique container of items. The player should have two containers : Inventory & Equipment. " +
                "Other units that carry items should also have their own [InventoryData], such as Merchant, Crates, or Monsters. ", 30);

                for (int i = 0; i < _inventoryModule.Inventory.Count; i++)
                {
                    _inventoryModule.Inventory[i].EntityUid = _entity.uid;
                    if (EditorUtils.TitleButton(new GUIContent(_inventoryModule.Inventory[i].Name + " [" + _inventoryModule.Inventory[i].Type + "]", "Click to expand"),
                        ref _inventoryModule.Inventory[i].uiFold, 10, true, 10, true))
                    {
                        _inventoryModule.Inventory.RemoveAt(i);
                        EditorGUI.FocusTextInControl(null);
                        _valueChanged = true;
                        break;
                    }
                    if (_inventoryModule.Inventory[i].uiFold)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        GUILayout.Label(new GUIContent("Type:", "Make sure set matched type, this will change behavior of this container."), GUILayout.Width(150));
                        GUI.backgroundColor = EditorUtils._black;
                        _inventoryModule.Inventory[i].Type = (InventoryData.HolderType)EditorGUILayout.EnumPopup(_inventoryModule.Inventory[i].Type);
                        GUILayout.Space(40);
                        GUILayout.EndHorizontal();

                        EditorUtils.TextField(ref _inventoryModule.Inventory[i].Name, new GUIContent("Display Name:", "This will be the name shows as the title of the window."), 40, 150, true);

                        if (_inventoryModule.Inventory[i].Type == InventoryData.HolderType.Merchant)
                        {
                            EditorUtils.TitleButton(new GUIContent(" Merchant Settings", "The settings for Merchant."), ref _inventoryModule.Inventory[i].uiMerchantExpand, 40, false, 40);
                            if (_inventoryModule.Inventory[i].uiMerchantExpand)
                            {
                                EditorUtils.FloatSlider(ref _inventoryModule.Inventory[i].SellPriceMultiplier, 0F, 2F,
                                    new GUIContent("Sell Price Multiplier:", "This multiplier is applied when selling items. For example, a merchant NPC with a SellPriceMultiplier of 1.2 will sell an item priced at 1000 for 1200")
                                   , "x", 50, 150);
                                EditorUtils.FloatSlider(ref _inventoryModule.Inventory[i].BuyPriceMultiplier, 0F, 2F,
                                    new GUIContent("Buy Price Multiplier:", "This multiplier is applied when buying items. For example, a merchant NPC with a BuyPriceMultiplier of 0.8 will buy an item priced at 1000 for 800")
                                   , "x", 50, 150);

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(50);
                                _inventoryModule.Inventory[i].uiPriceFold = EditorGUILayout.Foldout(_inventoryModule.Inventory[i].uiPriceFold, "Specific Price Multiplier (" + _inventoryModule.Inventory[i].SpecificPriceMultiplier.Count + "):");
                                GUILayout.EndHorizontal();
                                if (_inventoryModule.Inventory[i].uiPriceFold)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(60);
                                    GUI.backgroundColor = EditorUtils._gray;
                                    GUILayout.BeginVertical(EditorUtils._groupStyle);

                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(10);
                                    GUI.backgroundColor = EditorUtils._titleColor;
                                    if (GUILayout.Button("Add", GUILayout.Width(100)))
                                    {
                                        _inventoryModule.Inventory[i].SpecificPriceMultiplier.Add(new Vector3(0F, _inventoryModule.Inventory[i].SellPriceMultiplier, _inventoryModule.Inventory[i].BuyPriceMultiplier));
                                        _valueChanged = true;
                                        EditorGUI.FocusTextInControl(null);
                                    }
                                    GUI.backgroundColor = Color.white;
                                    GUILayout.Space(10);
                                    GUILayout.EndHorizontal();

                                    for (int u = 0; u < _inventoryModule.Inventory[i].SpecificPriceMultiplier.Count; u++)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Space(10);
                                        GUI.backgroundColor = EditorUtils._gray;
                                        GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                        int _itemId = Mathf.FloorToInt(_inventoryModule.Inventory[i].SpecificPriceMultiplier[u].x);
                                        float _sell = _inventoryModule.Inventory[i].SpecificPriceMultiplier[u].y;
                                        float _buy = _inventoryModule.Inventory[i].SpecificPriceMultiplier[u].z;
                                        Item _item = ItemDic[_itemId];
                                        EditorUtils.ResetColor();
                                        GUILayout.Box(itemIconDic[_itemId], EditorUtils._toolButtonStyle, GUILayout.Width(24), GUILayout.Height(24));
                                        GUI.backgroundColor = ItemObject.instance.itemTypes[_item.type].color;
                                        int _index = ItemObject.instance.IndexOfItems(_item.uid);
                                        _index = EditorGUILayout.Popup("", _index, ItemNames.ToArray(), GUILayout.Width(100F));
                                        GUI.backgroundColor = EditorUtils._black;
                                        GUILayout.Label("Sell:", GUILayout.Width(30F));
                                        _sell = EditorGUILayout.FloatField(_sell, GUILayout.Width(30F));
                                        GUILayout.Label("x", GUILayout.Width(20F));
                                        GUILayout.Label("Buy:", GUILayout.Width(30F));
                                        _buy = EditorGUILayout.FloatField(_buy, GUILayout.Width(30F));
                                        GUILayout.Label("x", GUILayout.Width(20F));
                                        _inventoryModule.Inventory[i].SpecificPriceMultiplier[u] = new Vector3(_index, _sell, _buy);
                                        GUI.backgroundColor = EditorUtils._black;
                                        GUILayout.FlexibleSpace();
                                        if (GUILayout.Button("X", GUILayout.Width(20)))
                                        {
                                            _inventoryModule.Inventory[i].SpecificPriceMultiplier.RemoveAt(u);
                                            _valueChanged = true;
                                            EditorGUI.FocusTextInControl(null);
                                        }
                                        EditorUtils.ResetColor();
                                        GUILayout.EndHorizontal();
                                        GUILayout.Space(10);
                                        GUILayout.EndHorizontal();
                                    }

                                    GUILayout.EndVertical();
                                    GUILayout.Space(40);
                                    GUILayout.EndHorizontal();
                                }
                                EditorGUILayout.Separator();
                                GUI.backgroundColor = EditorUtils._titleColor;
                                EditorUtils.Toggle(ref _inventoryModule.Inventory[i].TradeAllItems, new GUIContent("Accept all tradeable items.", "When unchecked, this merchant will only accept tradeable items within a list."), 50, 150);
                                EditorUtils.ResetColor();

                                if (!_inventoryModule.Inventory[i].TradeAllItems)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(50);
                                    _inventoryModule.Inventory[i].uiTradeTypeFold = EditorGUILayout.Foldout(_inventoryModule.Inventory[i].uiTradeTypeFold, "Accept items of these categories (" + _inventoryModule.Inventory[i].TradeCategoryList.Count + "):");
                                    GUILayout.EndHorizontal();

                                    if (_inventoryModule.Inventory[i].uiTradeTypeFold)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Space(60);
                                        GUI.backgroundColor = EditorUtils._gray;
                                        GUILayout.BeginVertical(EditorUtils._groupStyle);

                                        GUILayout.BeginHorizontal();
                                        GUILayout.Space(10);
                                        GUILayout.Label("Category:", GUILayout.Width(60));
                                        _tradeCategoryType = Mathf.Clamp(_tradeCategoryType, 0, ItemObject.instance.itemTypes.Count - 1);
                                        GUI.backgroundColor = ItemObject.instance.itemTypes[_tradeCategoryType].color;
                                        _tradeCategoryType = EditorGUILayout.Popup("", _tradeCategoryType, ItemObject.instance.ItemCategoryNames, GUILayout.Width(150));

                                        GUI.backgroundColor = EditorUtils._titleColor;
                                        if (GUILayout.Button("Add", GUILayout.Width(70)))
                                        {
                                            _inventoryModule.Inventory[i].TradeCategoryList.Add(_tradeCategoryType);
                                            _valueChanged = true;
                                            EditorGUI.FocusTextInControl(null);
                                        }
                                        EditorUtils.ResetColor();
                                        GUILayout.Space(10);
                                        GUILayout.EndHorizontal();

                                        for (int u = 0; u < _inventoryModule.Inventory[i].TradeCategoryList.Count; u++)
                                        {
                                            if (_inventoryModule.Inventory[i].TradeCategoryList[u] >= 0 && _inventoryModule.Inventory[i].TradeCategoryList[u] < ItemObject.instance.itemTypes.Count)
                                            {
                                                GUILayout.BeginHorizontal();
                                                GUILayout.Space(10);
                                                GUI.backgroundColor = EditorUtils._gray;
                                                GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                                GUI.backgroundColor = ItemObject.instance.itemTypes[_inventoryModule.Inventory[i].TradeCategoryList[u]].color; ;
                                                GUILayout.Button(ItemObject.instance.itemTypes[_inventoryModule.Inventory[i].TradeCategoryList[u]].name, EditorUtils._titleButtonStyle, GUILayout.Width(150));
                                                GUILayout.FlexibleSpace();
                                                GUI.backgroundColor = EditorUtils._black;
                                                if (GUILayout.Button("X", GUILayout.Width(20)))
                                                {
                                                    _inventoryModule.Inventory[i].TradeCategoryList.RemoveAt(u);
                                                    _valueChanged = true;
                                                    EditorGUI.FocusTextInControl(null);
                                                }
                                                EditorUtils.ResetColor();
                                                GUILayout.EndHorizontal();
                                                GUILayout.Space(10);
                                                GUILayout.EndHorizontal();
                                            }
                                            else
                                            {
                                                _inventoryModule.Inventory[i].TradeCategoryList.RemoveAt(u);
                                            }
                                        }
                                        GUILayout.EndVertical();
                                        GUILayout.Space(40);
                                        GUILayout.EndHorizontal();
                                    }

                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(50);
                                    _inventoryModule.Inventory[i].uiTradeListFold = EditorGUILayout.Foldout(_inventoryModule.Inventory[i].uiTradeListFold, "Accept items within this list: (" + _inventoryModule.Inventory[i].TradeList.Count + "):");
                                    GUILayout.EndHorizontal();
                                    if (_inventoryModule.Inventory[i].uiTradeListFold)
                                    {
                                        if (ItemObject.instance.ItemCount <= 0)
                                        {
                                            EditorUtils.Warnning("You must have at least one item setup in Project Settings/SoftKitty/SubData - Items", 60);
                                        }
                                        else
                                        {
                                            GUILayout.BeginHorizontal();
                                            GUILayout.Space(60);
                                            GUI.backgroundColor = EditorUtils._gray;
                                            GUILayout.BeginVertical(EditorUtils._groupStyle);
                                            GUILayout.BeginHorizontal();
                                            Item _tradeItem = ItemDic[_tradeItemId];
                                            Color _color = ItemObject.instance.itemTypes[_tradeItem.type].color;
                                            EditorUtils.ResetColor();
                                            GUILayout.Box(itemIconDic[_tradeItemId], EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20));
                                            GUI.backgroundColor = _color;
                                            int _itemIndex = ItemObject.instance.IndexOfItems(_tradeItemId);
                                            _itemIndex = EditorGUILayout.Popup("", _itemIndex, ItemNames.ToArray(), GUILayout.Width(190));

                                            GUI.backgroundColor = EditorUtils._titleColor;
                                            if (GUILayout.Button("Add", GUILayout.Width(70)))
                                            {
                                                if (!_inventoryModule.Inventory[i].TradeList.Contains(_tradeItemId)) _inventoryModule.Inventory[i].TradeList.Add(_tradeItemId);
                                                _valueChanged = true;
                                                EditorGUI.FocusTextInControl(null);
                                            }
                                            EditorUtils.ResetColor();
                                            GUILayout.EndHorizontal();

                                            GUILayout.BeginHorizontal();
                                            GUILayout.Label("Category:", GUILayout.Width(60));
                                            _tradeType = Mathf.Clamp(_tradeType, 0, ItemObject.instance.itemTypes.Count - 1);
                                            GUI.backgroundColor = ItemObject.instance.itemTypes[_tradeType].color;
                                            _tradeType = EditorGUILayout.Popup("", _tradeType, ItemObject.instance.ItemCategoryNames, GUILayout.Width(100));

                                            GUI.backgroundColor = EditorUtils._titleColor;
                                            if (GUILayout.Button("Add All", GUILayout.Width(65)))
                                            {
                                                for (int u = 0; u < ItemObject.instance.ItemCount; u++)
                                                {
                                                    if (ItemObject.instance.GetItemByIndex(i).type == _tradeType && !_inventoryModule.Inventory[i].TradeList.Contains(ItemObject.instance.GetItemByIndex(u).id)) _inventoryModule.Inventory[i].TradeList.Add(ItemObject.instance.GetItemByIndex(u).id);
                                                }
                                                _valueChanged = true;
                                                EditorGUI.FocusTextInControl(null);
                                            }
                                            GUI.backgroundColor = EditorUtils._black;
                                            if (GUILayout.Button("Remove All", GUILayout.Width(85)))
                                            {
                                                for (int u = _inventoryModule.Inventory[i].TradeList.Count - 1; u >= 0; u--)
                                                {
                                                    if (ItemObject.instance.GetItemByIndex(_inventoryModule.Inventory[i].TradeList[u]).type == _tradeType) _inventoryModule.Inventory[i].TradeList.RemoveAt(u);
                                                }
                                                _valueChanged = true;
                                                EditorGUI.FocusTextInControl(null);
                                            }
                                            EditorUtils.ResetColor();
                                            GUILayout.EndHorizontal();


                                            for (int u = 0; u < _inventoryModule.Inventory[i].TradeList.Count; u++)
                                            {
                                                if (_inventoryModule.Inventory[i].TradeList[u] >= 0)
                                                {
                                                    Item _listItem = ItemDic[_inventoryModule.Inventory[i].TradeList[u]];
                                                    if (_listItem != null)
                                                    {
                                                        GUILayout.BeginHorizontal();
                                                        GUILayout.Space(15);
                                                        GUI.backgroundColor = EditorUtils._gray;
                                                        GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                                        EditorUtils.ResetColor();
                                                        Color _itemColor = ItemObject.instance.itemTypes[_listItem.type].color;
                                                        GUILayout.Box(itemIconDic[_inventoryModule.Inventory[i].TradeList[u]], EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20));
                                                        GUI.backgroundColor = _itemColor;
                                                        GUILayout.Box(_listItem.name, EditorUtils._boxStyle, GUILayout.Width(120));
                                                        EditorUtils.ResetColor();
                                                        if (!_listItem.tradeable)
                                                        {
                                                            GUILayout.Box(EditorUtils.GetTexture(EditorIcon.Warning), EditorUtils._toolButtonStyle, GUILayout.Width(17), GUILayout.Height(17));
                                                            GUI.color = Color.red;
                                                            GUILayout.Label("NOT tradeable", GUILayout.Width(100));
                                                            GUI.color = Color.white;
                                                        }
                                                        GUILayout.FlexibleSpace();
                                                        GUI.backgroundColor = EditorUtils._black;
                                                        if (GUILayout.Button("X", GUILayout.Width(20)))
                                                        {
                                                            _inventoryModule.Inventory[i].TradeList.RemoveAt(u);
                                                            _valueChanged = true;
                                                            EditorGUI.FocusTextInControl(null);
                                                        }
                                                        GUI.backgroundColor = Color.white;
                                                        GUILayout.EndHorizontal();
                                                        GUILayout.Space(15);
                                                        GUILayout.EndHorizontal();
                                                    }
                                                    else
                                                    {
                                                        _inventoryModule.Inventory[i].TradeList.RemoveAt(u);
                                                        _valueChanged = true;
                                                    }
                                                }
                                                else
                                                {
                                                    _inventoryModule.Inventory[i].TradeList.RemoveAt(u);
                                                    _valueChanged = true;
                                                }
                                            }
                                            GUILayout.EndVertical();
                                            GUILayout.Space(40);
                                            GUILayout.EndHorizontal();
                                        }
                                    }

                                }
                                EditorGUILayout.Separator();
                            }

                        }


                        EditorUtils.TitleButton(new GUIContent(" Currency", "The currencies this unit carries."), ref _inventoryModule.Inventory[i].uiCurrencyExpand, 40, false, 40);
                        if (_inventoryModule.Inventory[i].Currency.Count < ItemObject.instance.currencies.Count)
                        {
                            _inventoryModule.Inventory[i].Currency.Add(0);
                            _valueChanged = true;
                        }
                        if (_inventoryModule.Inventory[i].uiCurrencyExpand)
                        {
                            if (ItemObject.instance.currencies.Count <= 0)
                            {
                                EditorUtils.Warnning("You must have at least one currency setup in Project Settings/SoftKitty/SubData - Items", 50);
                            }
                            else
                            {
                                for (int u = 0; u < ItemObject.instance.currencies.Count; u++)
                                {
                                    if (_inventoryModule.Inventory[i].Currency.Count <= u) _inventoryModule.Inventory[i].Currency.Add(0);
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(50);
                                    GUI.backgroundColor = EditorUtils._gray;
                                    GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                    GUILayout.Label("(ID:" + u.ToString() + ")", GUILayout.Width(40), GUILayout.Height(20));
                                    if (ItemObject.instance.currencies[u].icon != null)
                                    {
                                        GUILayout.Box(ItemObject.instance.currencies[u].icon.texture, EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20));
                                    }
                                    else
                                    {
                                        GUILayout.Box("-", EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20));
                                    }
                                    GUI.color = ItemObject.instance.currencies[u].color;
                                    GUILayout.Label(ItemObject.instance.currencies[u].name + " :", GUILayout.Width(100), GUILayout.Height(20));
                                    GUI.color = Color.white;

                                    GUI.backgroundColor = ItemObject.instance.currencies[u].color;
                                    _inventoryModule.Inventory[i].Currency.Currency[u] = EditorGUILayout.IntField(_inventoryModule.Inventory[i].Currency.Currency[u], GUILayout.Width(100), GUILayout.Height(20));
                                    EditorUtils.ResetColor();
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(40);
                                    GUILayout.EndHorizontal();
                                }
                            }

                            EditorGUILayout.Separator();
                        }


                        EditorUtils.TitleButton(new GUIContent(" Inventory Items", "The items this unit carries."), ref _inventoryModule.Inventory[i].uiItemExpand, 40, false, 40);


                        if (_inventoryModule.Inventory[i].uiItemExpand)
                        {
                            if (ItemObject.instance.ItemCount <= 0)
                            {
                                EditorUtils.Warnning("You must have at least one item setup in Project Settings/SoftKitty/SubData - Items", 50);
                            }
                            else
                            {
                                float _weight = 0F;
                                for (int u = 0; u < _inventoryModule.Inventory[i].Stacks.Count; u++) _weight += _inventoryModule.Inventory[i].Stacks[u].GetWeight();
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(50);
                                GUILayout.Label(new GUIContent("Maxmium CarryWeight:", "The maxmium carry weight of this unit."), GUILayout.Width(150));
                                GUI.backgroundColor = EditorUtils._titleColor;
                                _inventoryModule.Inventory[i].MaxiumCarryWeight = EditorGUILayout.FloatField(_inventoryModule.Inventory[i].MaxiumCarryWeight, GUILayout.Width(70));
                                GUI.color = _weight <= _inventoryModule.Inventory[i].MaxiumCarryWeight ? EditorUtils._buttonColor : EditorUtils._red;
                                GUILayout.Label("  (" + _weight.ToString("0.0") + "/" + _inventoryModule.Inventory[i].MaxiumCarryWeight.ToString("0.0") + ")", GUILayout.Width(100));
                                EditorUtils.ResetColor();
                                GUILayout.Space(40);
                                GUILayout.EndHorizontal();

                                EditorUtils.IntSlider(ref _inventoryModule.Inventory[i].InventorySize, 1, 200, new GUIContent("Inventory Size:", "This will be the total count of the slots in the bag."), "", 50, 150);

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(50);
                                GUI.backgroundColor = EditorUtils._black * 0.7F;
                                GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                GUILayout.Label(" Items", GUILayout.Height(25));
                                GUI.backgroundColor = EditorUtils._black;
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("Expand All", GUILayout.Width(80)))
                                {
                                    for (int u = 0; u < _inventoryModule.Inventory[i].Stacks.Count; u++) _inventoryModule.Inventory[i].Stacks[u].Fold = false;
                                    _valueChanged = true;
                                }
                                if (GUILayout.Button("Shrink All", GUILayout.Width(80)))
                                {
                                    for (int u = 0; u < _inventoryModule.Inventory[i].Stacks.Count; u++) _inventoryModule.Inventory[i].Stacks[u].Fold = true;
                                    _valueChanged = true;
                                }
                                EditorUtils.ResetColor();
                                GUILayout.EndHorizontal();
                                GUILayout.Space(40);
                                GUILayout.EndHorizontal();

                                if (_inventoryModule.Inventory[i].Stacks.Count > _inventoryModule.Inventory[i].InventorySize)
                                {
                                    for (int u = _inventoryModule.Inventory[i].InventorySize; u < _inventoryModule.Inventory[i].Stacks.Count; u++) _inventoryModule.Inventory[i].Stacks.RemoveAt(u);
                                }
                                else if (_inventoryModule.Inventory[i].Stacks.Count < _inventoryModule.Inventory[i].InventorySize)
                                {
                                    for (int u = _inventoryModule.Inventory[i].Stacks.Count; u < _inventoryModule.Inventory[i].InventorySize; u++) _inventoryModule.Inventory[i].Stacks.Add(new InventoryStack());
                                }

                                for (int u = 0; u < _inventoryModule.Inventory[i].Stacks.Count; u++)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(50);
                                    GUI.backgroundColor = (_inventoryModule.Inventory[i].Stacks[u].Item != null && !_inventoryModule.Inventory[i].Stacks[u].Empty) ? EditorUtils._gray : EditorUtils._black;
                                    GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                    int _item = -1;

                                    Color _color = Color.white;
                                    if (GUILayout.Button(_inventoryModule.Inventory[i].Stacks[u].Fold ? "+" : "-", GUILayout.Width(20)))
                                    {
                                        _inventoryModule.Inventory[i].Stacks[u].Fold = !_inventoryModule.Inventory[i].Stacks[u].Fold;
                                        _search = "Search item by name";
                                    }
                                    if (_inventoryModule.Inventory[i].Stacks[u].Item != null && !_inventoryModule.Inventory[i].Stacks[u].Empty)
                                    {

                                        if (!ItemExist(_inventoryModule.Inventory[i].Stacks[u].Item.id))
                                        {
                                            _inventoryModule.Inventory[i].Stacks[u].Delete();
                                            _valueChanged = true;
                                        }
                                        else
                                        {
                                            _inventoryModule.Inventory[i].Stacks[u].Item.type = Mathf.Clamp(_inventoryModule.Inventory[i].Stacks[u].Item.type, 0, ItemObject.instance.itemTypes.Count - 1);
                                            _item = _inventoryModule.Inventory[i].Stacks[u].Item.id;
                                            _color = ItemObject.instance.itemTypes[_inventoryModule.Inventory[i].Stacks[u].Item.type].color;
                                            EditorUtils.ResetColor();
                                            GUILayout.Box(itemIconDic[_inventoryModule.Inventory[i].Stacks[u].Item.id], EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20));
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Box("-", GUILayout.Width(20), GUILayout.Height(20));
                                    }
                                    int _itemIndex = ItemObject.instance.IndexOfItems(_item) + 1;
                                    EditorGUI.BeginChangeCheck();
                                    GUI.backgroundColor = _color;
                                    _itemIndex = EditorGUILayout.Popup("", _itemIndex, ItemOptions, GUILayout.Width(150));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        if (_itemIndex == 0)
                                        {
                                            _inventoryModule.Inventory[i].Stacks[u].Empty = true;
                                            _inventoryModule.Inventory[i].Stacks[u].Item = null;
                                            _inventoryModule.Inventory[i].Stacks[u].Number = 0;
                                            _valueChanged = true;
                                        }
                                        else
                                        {
                                            _inventoryModule.Inventory[i].Stacks[u] = new InventoryStack(ItemObject.instance.GetItemByIndex(_itemIndex - 1), Mathf.Max(1, _inventoryModule.Inventory[i].Stacks[u].Number));
                                            _valueChanged = true;
                                        }
                                    }
                                    GUILayout.Label(" x", GUILayout.Width(20));

                                    int _maxStack = 0;
                                    if (_inventoryModule.Inventory[i].Stacks[u].Item != null && !_inventoryModule.Inventory[i].Stacks[u].Empty)
                                    {
                                        _maxStack = _inventoryModule.Inventory[i].Stacks[u].Item.maxiumStack;
                                    }

                                    GUI.backgroundColor = EditorUtils._titleColor;
                                    _inventoryModule.Inventory[i].Stacks[u].Number = Mathf.FloorToInt(GUILayout.HorizontalSlider(_inventoryModule.Inventory[i].Stacks[u].Number, 0, _maxStack, GUILayout.Width(60)));
                                    if (_item == 0 && _inventoryModule.Inventory[i].Stacks[u].Number != 0)
                                    {
                                        _inventoryModule.Inventory[i].Stacks[u].Number = 0;
                                        _valueChanged = true;
                                    }
                                    GUILayout.Label(_inventoryModule.Inventory[i].Stacks[u].Number.ToString(), GUILayout.Width(25));
                                    EditorUtils.ResetColor();

                                    GUI.color = EditorUtils._red;
                                    if (_inventoryModule.Inventory[i].Stacks[u].Item != null && _inventoryModule.Inventory[i].Stacks[u].Item.upgradeLevel > 0)
                                    {
                                        GUILayout.Label("Lv." + _inventoryModule.Inventory[i].Stacks[u].Item.upgradeLevel.ToString(), GUILayout.Width(40));
                                    }
                                    else
                                    {
                                        GUILayout.Space(40);
                                    }
                                    EditorUtils.ResetColor();
                                    GUILayout.FlexibleSpace();
                                    GUI.backgroundColor = EditorUtils._black;
                                    if (GUILayout.Button("X", GUILayout.Width(20)))
                                    {
                                        _inventoryModule.Inventory[i].Stacks[u].Empty = true;
                                        _inventoryModule.Inventory[i].Stacks[u].Item = null;
                                        _inventoryModule.Inventory[i].Stacks[u].Number = 0;
                                        _valueChanged = true;
                                    }
                                    EditorUtils.ResetColor();
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(40);
                                    GUILayout.EndHorizontal();

                                    if (!_inventoryModule.Inventory[i].Stacks[u].Fold)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Space(70);
                                        GUILayout.Label("Search:", GUILayout.Width(120));
                                        GUI.backgroundColor = EditorUtils._black;
                                        _search = GUILayout.TextField(_search, GUILayout.Width(150), GUILayout.Height(18));
                                        if (GUILayout.Button("X", GUILayout.Width(25)))
                                        {
                                            _search = "";
                                        }
                                        EditorUtils.ResetColor();
                                        GUILayout.Space(40);
                                        GUILayout.EndHorizontal();
                                        if (!string.IsNullOrEmpty(_search) && _search != "Search item by name")
                                        {
                                            for (int w = 0; w < ItemObject.instance.ItemCount; w++)
                                            {
                                                if (ItemNames[w].ToLower().Contains(_search.ToLower()))
                                                {
                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Space(100);
                                                    GUILayout.Box(itemIconDic[ItemObject.instance.GetItemByIndex(w).id], EditorUtils._toolButtonStyle, GUILayout.Width(18), GUILayout.Height(18));
                                                    GUI.backgroundColor = EditorUtils._black;
                                                    if (GUILayout.Button(ItemObject.instance.GetItemByIndex(w).name, GUILayout.Width(170)))
                                                    {
                                                        _inventoryModule.Inventory[i].Stacks[u] = new InventoryStack(ItemObject.instance.GetItemByIndex(w), Mathf.Max(1, _inventoryModule.Inventory[i].Stacks[u].Number));
                                                        _inventoryModule.Inventory[i].Stacks[u].Fold = true;
                                                        _valueChanged = true;
                                                        EditorGUI.FocusTextInControl(null);
                                                    }
                                                    GUILayout.Space(40);
                                                    GUILayout.EndHorizontal();
                                                    EditorUtils.ResetColor();
                                                }
                                            }
                                        }

                                        if (_inventoryModule.Inventory[i].Stacks[u].Item != null)
                                        {
                                            if (ItemObject.instance.EnableEnhancing && _inventoryModule.Inventory[i].Stacks[u].Item.type == ItemObject.instance.EnhancingCategoryID)
                                            {
                                                GUILayout.BeginHorizontal();
                                                GUILayout.Space(70);
                                                GUILayout.Label("Enhancing Level: ", GUILayout.Width(120));
                                                _inventoryModule.Inventory[i].Stacks[u].Item.upgradeLevel = EditorGUILayout.IntSlider(_inventoryModule.Inventory[i].Stacks[u].Item.upgradeLevel, 0, ItemObject.instance.MaxiumEnhancingLevel);
                                                GUILayout.Space(40);
                                                GUILayout.EndHorizontal();
                                            }

                                            if (ItemObject.instance.EnableEnchanting && ItemObject.instance.itemEnchantments.Count > 0 && _inventoryModule.Inventory[i].Stacks[u].Item.type == ItemObject.instance.EnchantingCategoryID)
                                            {
                                                GUILayout.BeginHorizontal();
                                                GUILayout.Space(70);
                                                GUILayout.Label("Enchantments (" + _inventoryModule.Inventory[i].Stacks[u].Item.enchantments.Count + "): ", GUILayout.Width(120));
                                                GUI.backgroundColor = EditorUtils._titleColor;
                                                if (GUILayout.Button("+", GUILayout.Width(25)))
                                                {
                                                    _inventoryModule.Inventory[i].Stacks[u].Item.enchantments.Add(0);
                                                    _valueChanged = true;
                                                }
                                                EditorUtils.ResetColor();
                                                GUILayout.Space(40);
                                                GUILayout.EndHorizontal();

                                                for (int w = _inventoryModule.Inventory[i].Stacks[u].Item.enchantments.Count - 1; w >= 0; w--)
                                                {
                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Space(90);
                                                    int _ent = _inventoryModule.Inventory[i].Stacks[u].Item.enchantments[w];
                                                    EditorGUI.BeginChangeCheck();

                                                    GUI.backgroundColor = EditorUtils._black;
                                                    _ent = EditorGUILayout.Popup("", _ent, EnchantOptions, GUILayout.Width(120));

                                                    if (EditorGUI.EndChangeCheck())
                                                    {
                                                        _inventoryModule.Inventory[i].Stacks[u].Item.enchantments[w] = _ent;
                                                        _valueChanged = true;
                                                    }

                                                    GUILayout.Label(ItemObject.instance.itemEnchantments[_inventoryModule.Inventory[i].Stacks[u].Item.enchantments[w]].GetDescription(), EditorUtils._richLabelStyle, GUILayout.ExpandWidth(true));
                                                    GUILayout.FlexibleSpace();
                                                    GUI.backgroundColor = EditorUtils._black;
                                                    if (GUILayout.Button("X", GUILayout.Width(25)))
                                                    {
                                                        _inventoryModule.Inventory[i].Stacks[u].Item.enchantments.RemoveAt(w);
                                                        _valueChanged = true;
                                                    }
                                                    EditorUtils.ResetColor();
                                                    GUILayout.Space(40);
                                                    GUILayout.EndHorizontal();
                                                }
                                            }

                                            if (ItemObject.instance.EnableSocketing && _inventoryModule.Inventory[i].Stacks[u].Item.type == ItemObject.instance.SocketedCategoryFilter)
                                            {
                                                GUILayout.BeginHorizontal();
                                                GUILayout.Space(70);
                                                GUILayout.Label("Socketing Slots (" + _inventoryModule.Inventory[i].Stacks[u].Item.socketingSlots.ToString() + "):", GUILayout.Width(120));
                                                GUI.backgroundColor = _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count < ItemObject.instance.MaxmiumSocketingSlotsNumber ? EditorUtils._titleColor : Color.gray;
                                                if (GUILayout.Button("+", GUILayout.Width(25)))
                                                {
                                                    if (_inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count < ItemObject.instance.MaxmiumSocketingSlotsNumber)
                                                    {
                                                        _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Add(ItemObject.instance.LockSocketingSlotsByDefault ? -2 : -1);
                                                        _inventoryModule.Inventory[i].Stacks[u].Item.socketingSlots = _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count;
                                                        _valueChanged = true;
                                                    }
                                                }
                                                GUI.backgroundColor = _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count > 0 ? Color.red : Color.gray;
                                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                                {
                                                    if (_inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count > 0)
                                                    {
                                                        _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.RemoveAt(_inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count - 1);
                                                        _inventoryModule.Inventory[i].Stacks[u].Item.socketingSlots = _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count;
                                                        _valueChanged = true;
                                                    }
                                                }
                                                GUILayout.FlexibleSpace();
                                                EditorUtils.ResetColor();
                                                GUILayout.Space(40);
                                                GUILayout.EndHorizontal();

                                                GUILayout.BeginHorizontal();
                                                GUILayout.Space(90);
                                                for (int w = 0; w < _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems.Count; w++)
                                                {
                                                    if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(_inventoryModule.Inventory[i].Stacks[u].Item.socketedItems[w] == -2 ? EditorIcon.Lock : EditorIcon.Socket), _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems[w] == -2 ? "Locked" : "Available"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                                                    {
                                                        if (_inventoryModule.Inventory[i].Stacks[u].Item.socketedItems[w] == -2)
                                                        {
                                                            _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems[w] = -1;
                                                            _valueChanged = true;
                                                        }
                                                        else
                                                        {
                                                            _inventoryModule.Inventory[i].Stacks[u].Item.socketedItems[w] = -2;
                                                            _valueChanged = true;
                                                        }
                                                    }
                                                }

                                                EditorUtils.ResetColor();
                                                GUILayout.Space(40);
                                                GUILayout.EndHorizontal();
                                                EditorGUILayout.Separator();
                                            }
                                        }
                                        EditorGUILayout.Separator();
                                    }
                                }

                            }
                            EditorGUILayout.Separator();
                        }

                        EditorUtils.TitleButton(new GUIContent(" Hidden Items", "Hidden items will not show in the normal inventory window, but shows in the interfaces using scripts that inherit from HiddenContainer.cs"), ref _inventoryModule.Inventory[i].uiHiddenExpand, 40, false, 40);
                        if (_inventoryModule.Inventory[i].uiHiddenExpand)
                        {
                            if (ItemObject.instance.ItemCount <= 0)
                            {
                                EditorUtils.Warnning("You must have at least one item setup in Project Settings/SoftKitty/SubData - Items", 50);
                            }
                            else
                            {

                                GUILayout.BeginHorizontal();
                                GUILayout.Space(50);
                                GUI.backgroundColor = EditorUtils._black * 0.6F;
                                GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                GUI.backgroundColor = EditorUtils._titleColor;
                                if (GUILayout.Button("Add Item", GUILayout.Width(200)))
                                {
                                    _inventoryModule.Inventory[i].HiddenStacks.Add(new InventoryStack());
                                    _valueChanged = true;
                                }
                                GUILayout.FlexibleSpace();
                                GUI.backgroundColor = EditorUtils._black;
                                if (GUILayout.Button("Expand All", GUILayout.Width(80)))
                                {
                                    for (int u = 0; u < _inventoryModule.Inventory[i].HiddenStacks.Count; u++) _inventoryModule.Inventory[i].HiddenStacks[u].Fold = false;
                                    _valueChanged = true;
                                }
                                if (GUILayout.Button("Shrink All", GUILayout.Width(80)))
                                {
                                    for (int u = 0; u < _inventoryModule.Inventory[i].HiddenStacks.Count; u++) _inventoryModule.Inventory[i].HiddenStacks[u].Fold = true;
                                    _valueChanged = true;
                                }
                                EditorUtils.ResetColor();
                                GUILayout.EndHorizontal();
                                GUILayout.Space(40);
                                GUILayout.EndHorizontal();

                                for (int u = 0; u < _inventoryModule.Inventory[i].HiddenStacks.Count; u++)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(50);
                                    GUI.backgroundColor = (_inventoryModule.Inventory[i].HiddenStacks[u].Item != null && !_inventoryModule.Inventory[i].HiddenStacks[u].Empty) ? EditorUtils._gray : EditorUtils._black;
                                    GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));
                                    int _item = -1;
                                    Color _color = Color.white;
                                    if (GUILayout.Button(_inventoryModule.Inventory[i].HiddenStacks[u].Fold ? "+" : "-", GUILayout.Width(20)))
                                    {
                                        _inventoryModule.Inventory[i].HiddenStacks[u].Fold = !_inventoryModule.Inventory[i].HiddenStacks[u].Fold;
                                        _search = "Search item by name";
                                    }
                                    if (_inventoryModule.Inventory[i].HiddenStacks[u].Item != null && !_inventoryModule.Inventory[i].HiddenStacks[u].Empty)
                                    {
                                        if (!ItemExist(_inventoryModule.Inventory[i].HiddenStacks[u].Item.id))
                                        {
                                            _inventoryModule.Inventory[i].HiddenStacks[u].Delete();
                                            _valueChanged = true;
                                        }
                                        else
                                        {
                                            _inventoryModule.Inventory[i].HiddenStacks[u].Item.type = Mathf.Clamp(_inventoryModule.Inventory[i].HiddenStacks[u].Item.type, 0, ItemObject.instance.itemTypes.Count - 1);
                                            _item = _inventoryModule.Inventory[i].HiddenStacks[u].Item.id;
                                            _color = ItemObject.instance.itemTypes[_inventoryModule.Inventory[i].HiddenStacks[u].Item.type].color;
                                            EditorUtils.ResetColor();
                                            GUILayout.Box(itemIconDic[_inventoryModule.Inventory[i].HiddenStacks[u].Item.id], EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20));
                                        }
                                    }
                                    else
                                    {
                                        GUILayout.Box("-", GUILayout.Width(20), GUILayout.Height(20));
                                    }
                                    int _itemIndex = ItemObject.instance.IndexOfItems(_item) + 1;
                                    EditorGUI.BeginChangeCheck();
                                    GUI.backgroundColor = _color;
                                    _itemIndex = EditorGUILayout.Popup("", _itemIndex, ItemOptions, GUILayout.Width(150));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        if (_itemIndex == 0)
                                        {
                                            _inventoryModule.Inventory[i].HiddenStacks[u].Empty = true;
                                            _inventoryModule.Inventory[i].HiddenStacks[u].Item = null;
                                            _inventoryModule.Inventory[i].HiddenStacks[u].Number = 0;
                                            _valueChanged = true;
                                        }
                                        else
                                        {
                                            _inventoryModule.Inventory[i].HiddenStacks[u] = new InventoryStack(ItemObject.instance.GetItemByIndex(_itemIndex - 1), Mathf.Max(1, _inventoryModule.Inventory[i].HiddenStacks[u].Number));
                                            _valueChanged = true;
                                        }
                                    }
                                    GUILayout.Label(" x", GUILayout.Width(20));

                                    int _maxStack = 0;
                                    if (_inventoryModule.Inventory[i].HiddenStacks[u].Item != null && !_inventoryModule.Inventory[i].HiddenStacks[u].Empty)
                                    {
                                        _maxStack = _inventoryModule.Inventory[i].HiddenStacks[u].Item.maxiumStack;
                                    }

                                    GUI.backgroundColor = EditorUtils._titleColor;
                                    _inventoryModule.Inventory[i].HiddenStacks[u].Number = Mathf.FloorToInt(GUILayout.HorizontalSlider(_inventoryModule.Inventory[i].HiddenStacks[u].Number, 0, _maxStack, GUILayout.Width(60)));
                                    if (_item == 0 && _inventoryModule.Inventory[i].HiddenStacks[u].Number != 0)
                                    {
                                        _inventoryModule.Inventory[i].HiddenStacks[u].Number = 0;
                                        _valueChanged = true;
                                    }
                                    GUILayout.Label(_inventoryModule.Inventory[i].HiddenStacks[u].Number.ToString(), GUILayout.Width(25));
                                    EditorUtils.ResetColor();
                                    GUI.color = EditorUtils._red;
                                    if (_inventoryModule.Inventory[i].HiddenStacks[u].Item != null && _inventoryModule.Inventory[i].HiddenStacks[u].Item.upgradeLevel > 0)
                                    {
                                        GUILayout.Label("Lv." + _inventoryModule.Inventory[i].HiddenStacks[u].Item.upgradeLevel.ToString(), GUILayout.Width(40));
                                    }
                                    else
                                    {
                                        GUILayout.Space(40);
                                    }
                                    EditorUtils.ResetColor();
                                    GUILayout.FlexibleSpace();
                                    GUI.backgroundColor = EditorUtils._black;
                                    if (GUILayout.Button("X", GUILayout.Width(20)))
                                    {
                                        _inventoryModule.Inventory[i].HiddenStacks.RemoveAt(u);
                                        _valueChanged = true;
                                    }
                                    EditorUtils.ResetColor();
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(40);
                                    GUILayout.EndHorizontal();

                                    if (!_inventoryModule.Inventory[i].HiddenStacks[u].Fold)
                                    {
                                        GUILayout.BeginHorizontal();
                                        GUILayout.Space(70);
                                        GUILayout.Label("Search:", GUILayout.Width(120));
                                        GUI.backgroundColor = EditorUtils._black;
                                        _search = GUILayout.TextField(_search, GUILayout.Width(150), GUILayout.Height(18));
                                        if (GUILayout.Button("X", GUILayout.Width(25)))
                                        {
                                            _search = "";
                                        }
                                        EditorUtils.ResetColor();
                                        GUILayout.Space(40);
                                        GUILayout.EndHorizontal();

                                        if (string.IsNullOrEmpty(_search) && _search != "Search item by name")
                                        {
                                            for (int w = 0; w < ItemObject.instance.ItemCount; w++)
                                            {
                                                if (ItemNames[w].ToLower().Contains(_search.ToLower()))

                                                {
                                                    GUILayout.BeginHorizontal();
                                                    GUILayout.Space(100);
                                                    GUILayout.Box(itemIconDic[ItemObject.instance.GetItemByIndex(w).id], EditorUtils._toolButtonStyle, GUILayout.Width(18), GUILayout.Height(18));
                                                    GUI.backgroundColor = EditorUtils._black;
                                                    if (GUILayout.Button(ItemObject.instance.GetItemByIndex(w).name, GUILayout.Width(160)))
                                                    {
                                                        _inventoryModule.Inventory[i].HiddenStacks[u] = new InventoryStack(ItemObject.instance.GetItemByIndex(w), Mathf.Max(1, _inventoryModule.Inventory[i].HiddenStacks[u].Number));
                                                        _inventoryModule.Inventory[i].HiddenStacks[u].Fold = true;
                                                        _valueChanged = true;
                                                        EditorGUI.FocusTextInControl(null);
                                                    }
                                                    GUILayout.Space(40);
                                                    GUILayout.EndHorizontal();
                                                    EditorUtils.ResetColor();
                                                }
                                            }
                                        }
                                        GUI.color = Color.white;

                                        EditorGUILayout.Separator();
                                    }
                                }

                            }
                            EditorGUILayout.Separator();
                        }
                        EditorGUILayout.Separator();
                    }

                }

            }




            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._buttonColor * 0.6F;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle, GUILayout.Height(25));

            if (GUILayout.Button(EditorUtils.TextHint((_inventoryModule.inventoryFold ? "-" : "+") + " Loot Packs: (" + _inventoryModule.LootPacks.Count + ")", "Loot packs this entity can drop."), EditorUtils._titleButtonStyle))
            {
                _inventoryModule.lootpackFold = !_inventoryModule.lootpackFold;
                _valueChanged = true;
            }
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Help), "Opens the official online documentation, including guides, node reference, and examples."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
            {
                Application.OpenURL("https://www.soft-kitty.com/docs/master-inventory-engine/item-class/loot-pack");
                EditorGUI.FocusTextInControl(null);
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy the loot pack settings."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
            {
                CopiedLootPack.Clear();
                for (int i = 0; i < _inventoryModule.LootPacks.Count; i++) CopiedLootPack.Add(_inventoryModule.LootPacks[i]);
                EditorGUI.FocusTextInControl(null);
                EditorUtils.ShowBoxNotification("Loot Pack settings copied to clipboard!");
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste the loot pack settings."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
            {
                if (CopiedLootPack.Count > 0)
                {
                    _inventoryModule.LootPacks.Clear();
                    for (int i = 0; i < CopiedLootPack.Count; i++)
                    {
                        _inventoryModule.LootPacks.Add(CopiedLootPack[i]);
                    }
                    EditorGUI.FocusTextInControl(null);
                    _valueChanged = true;
                }
            }
            GUILayout.EndHorizontal();
            EditorUtils.End(10);

            if (_inventoryModule.lootpackFold)
            {
                EditorUtils.HelpInfo("Call <color=#22FFAA>_inventoryModule.DropLootPack()</color> to drop the loot pack, or you can simply call <color=#22FFAA>GameManage.DropLootPack(Vector3 _pos, string _uid)</color> to drop a loot pack.", 30);
                if (ItemObject.instance.lootPacks.Count <= 0)
                {
                    EditorUtils.Warnning("You have not setup loot packs yet, please go to <color=#22AAFF>Project Settings/SoftKitty/SubData - Items</color> to setup the loot packs.", 30);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.backgroundColor = EditorUtils._black;
                    SelLootPackUid = EditorGUILayout.Popup(SelLootPackUid, ItemObject.instance.LootPackUids.ToArray(), GUILayout.Width(150));
                    GUI.backgroundColor = EditorUtils._titleColor;
                    if (GUILayout.Button("Add", GUILayout.Width(100)))
                    {
                        _inventoryModule.LootPacks.Add(ItemObject.instance.LootPackUids[SelLootPackUid]);
                        _valueChanged = true;
                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();

                    GUI.backgroundColor = EditorUtils._black;
                    for (int i = 0; i < _inventoryModule.LootPacks.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUILayout.Box(_inventoryModule.LootPacks[i], GUILayout.Width(200));
                        if (GUILayout.Button(new GUIContent("X", "Remove this loot pack."), GUILayout.Width(20), GUILayout.Height(17)))
                        {
                            _inventoryModule.LootPacks.RemoveAt(i);
                            _valueChanged = true;
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorUtils.ResetColor();
                }
            }


            if (GUI.changed) _valueChanged = true;
            return _valueChanged;

        }


    }

    [InitializeOnLoad]
    public static class InventoryInspectorRegister
    {
        static InventoryInspectorRegister()
        {
            EntityInspectorRegistry.Register(new InventoryInspectorModule());
        }
    }
}
