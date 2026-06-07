using UnityEngine;
using UnityEditor;
using SoftKitty.InventoryEngine;
using System.Collections.Generic;
using System.IO;

namespace SoftKitty
{
    [CustomEditor(typeof(ItemObject))]
    public class ItemObject_Inspector : UnityEditor.Editor
    {

        private string[] SearchOptions = new string[5] { "uid", "name", "type", "quality", "tag" };
        private static List<string> ClipboardStringList = new List<string>();
        private static List<Vector2> ClipboardVector2List = new List<Vector2>();
        private static List<AttributeData> ClipboardAttributeList = new List<AttributeData>();
        private Item ClipboardItem = null;
        private LootPackData ClipboardLootPack = null;
        private EditorAssetData ClipboardAsset = null;
        private ItemEditorAssets EditorAsset;
        private static int _newAttSel = 0;


        private static void ApplyAndSave(ItemObject scriptableObject, ItemEditorAssets EditorAsset)
        {
            scriptableObject.GenerateUniqueHash();
            EditorUtility.SetDirty(scriptableObject);
            EditorUtility.SetDirty(EditorAsset);
        }

        private bool isMatching(Item _item, ItemObject _target)
        {
            if (string.IsNullOrEmpty(_target.SearchText)) return true;
            if (_target.SearchType == 0)
            {
                return _item.uid.ToLower().Contains(_target.SearchText.ToLower());
            }
            else if (_target.SearchType == 1)
            {
                return _item.name.ToLower().Contains(_target.SearchText.ToLower());
            }
            else if (_target.SearchType == 2)
            {
                return _target.itemTypes[_item.type].name.ToLower().Contains(_target.SearchText.ToLower());
            }
            else if (_target.SearchType == 3)
            {
                return _target.itemQuality[_item.quality].name.ToLower().Contains(_target.SearchText.ToLower());
            }
            else
            {
                return _item.isTagContainText(_target.SearchText.ToLower(), false);
            }
        }

        private static string mPath = "";
        public static string InventoryEngineEditorPath
        {
            get
            {
                if (string.IsNullOrEmpty(mPath))
                {
                    var guids = AssetDatabase.FindAssets("t:MonoScript ItemObject_Inspector");
                    if (guids.Length > 0)
                    {
                        mPath = AssetDatabase.GUIDToAssetPath(guids[0]).Replace("ItemObject_Inspector.cs", "");
                    }
                }
                return mPath;
            }
        }

        public override void OnInspectorGUI()
        {

            GUI.changed = false;
            bool _valueChanged = false;
            ItemObject myTarget = (ItemObject)target;
            var script = MonoScript.FromScriptableObject(this);
            string _thePath = AssetDatabase.GetAssetPath(script);
            string _PackagePath = _thePath.Replace("Editor/ItemObject_Inspector.cs", "");
            _thePath = _thePath.Replace("ItemObject_Inspector.cs", "");
            string _basePath = Path.Combine(Application.dataPath, "..", _PackagePath).Replace(@"\", "/") + "Textures/UiStyles/";
            string _targetPath = Path.Combine(Application.dataPath, "..", _PackagePath).Replace(@"\", "/") + "Textures/Sprites/Main.png";

            EditorUtils.TitleLogo(EditorUtils.GetTexture(EditorIcon.DataLogo), 0);

            EditorUtils.PreBox(EditorUtils.TextHint("Data Hash:", "The data hash is a unique string representing the current state of the data. It changes automatically whenever any data is modified."), 6, Color.black, 70);
            GUI.backgroundColor = EditorUtils._black;
            GUILayout.Box(myTarget.Hash, EditorUtils._boxLeftStyle);

            if (GUILayout.Button("Generate", GUILayout.Width(60)))
            {
                myTarget.GenerateUniqueHash();
            }
            EditorUtils.ResetColor();
            EditorUtils.End(0);

            EditorUtils.Document("master-inventory-engine/item-class/item-object");
            EditorGUILayout.Separator();

            EditorUtils.Save();

            if (GameManager.AttributeData == null)
            {
                EditorUtils.Warnning("Attribute Setting Object is not assigned, You can create this object via the context menu in any Project folder:" +
                "<color=6688FF>Create ¡ú SoftKitty ¡ú Data Objects ¡ú Attribute Data</color>" +
                "You can assign the created asset to the database in: <color=6688FF>Project Settings ¡ú SoftKitty ¡ú Data Settings ¡ú Data</color>", 10);
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = EditorUtils._buttonColor;
                if (GUILayout.Button("Assign One"))
                {
                    SGD_SettingsProvider.CurrentSettings.data_expand = true;
                    SettingsService.OpenProjectSettings("Project/SoftKitty/SubData - Attributes");
                }
                EditorUtils.ResetColor();
                GUILayout.EndHorizontal();
                return;
            }

            if (EditorAsset == null) EditorAsset = (ItemEditorAssets)AssetDatabase.LoadAssetAtPath(_thePath + "ItemEditorAssets.asset", typeof(ItemEditorAssets));
            if (myTarget.itemTypes.Count <= 0)
            {
                StringColorData _newType = new StringColorData();
                _newType.name = "Default Category" + myTarget.itemTypes.Count.ToString();
                _newType.color = Color.white;
                _newType.visible = true;
                myTarget.itemTypes.Add(_newType);
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            if (myTarget.itemQuality.Count <= 0)
            {
                StringColorData _newType = new StringColorData();
                _newType.name = "Default Quality" + myTarget.itemQuality.Count.ToString();
                _newType.color = Color.white;
                _newType.visible = true;
                myTarget.itemQuality.Add(_newType);
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            if (myTarget.currencies.Count <= 0)
            {
                Currency _newCurrency = new Currency();
                _newCurrency.name = "Default Currency";
                _newCurrency.color = Color.white;
                _newCurrency.icon = null;
                _newCurrency.ExchangeRate = new List<Vector3>();
                _newCurrency.fold = true;
                myTarget.currencies.Add(_newCurrency);
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            if (myTarget.ItemCount <= 0)
            {
                Item _newItem = new Item();
                _newItem.name = "Default Item";
                _newItem.description = "No description yet.";
                _newItem.type = 0;
                _newItem.quality = 0;
                _newItem.icon = null;
                _newItem.maxiumStack = 99;
                _newItem.price = 0;
                _newItem.upgradeLevel = 0;
                _newItem.useable = false;
                _newItem.consumable = false;
                _newItem.tradeable = true;
                _newItem.deletable = true;
                _newItem.attributes.Clear();
                _newItem.enchantments.Clear();
                _newItem.actions.Clear();
                _newItem.craftMaterials.Clear();
                _newItem.tags.Clear();
                _newItem.uid = "Item" + myTarget.ItemCount.ToString();
                _newItem.fold = true;
                if (myTarget.DataMode == ItemDataMode.Unified)
                {
                    myTarget.items.Add(_newItem);
                }
                else
                {
                    ItemScriptableObject _newObj = (ItemScriptableObject)ScriptableObject.CreateInstance(typeof(ItemScriptableObject));
                    _newObj.mItem = _newItem;
                    string _basepath = EditorUtils.DataPath + "Items/" + _newItem.uid;
                    string _path = _basepath + ".asset";
                    if (!AssetDatabase.IsValidFolder(EditorUtils.DataPath + "Items/"))
                    {
                        AssetDatabase.CreateFolder(EditorUtils.DataPath.Replace("Data/", "Data"), "Items");
                        AssetDatabase.Refresh();
                    }
                    AssetDatabase.CreateAsset(_newObj, _path);
                    AssetDatabase.Refresh();
                    EditorUtility.SetDirty(_newObj);
                    myTarget.itemScriptableObjects.Add(_newObj);
                }
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            string[] _typeOptions = new string[myTarget.itemTypes.Count];
            for (int i = 0; i < myTarget.itemTypes.Count; i++) _typeOptions[i] = myTarget.itemTypes[i].name;

            string[] _qualityOptions = new string[myTarget.itemQuality.Count];
            for (int i = 0; i < myTarget.itemQuality.Count; i++) _qualityOptions[i] = myTarget.itemQuality[i].name;

            string[] _itemOption = myTarget.ItemNames;

            string[] _currencyOption = new string[myTarget.currencies.Count];
            for (int i = 0; i < myTarget.currencies.Count; i++) _currencyOption[i] = myTarget.currencies[i].name;
            List<string> _allAtt = new List<string>(GameManager.AttributeData.AttributesUidArray);




            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = EditorUtils._black;
            if (GUILayout.Button(EditorUtils.TextHint("Import Settings From Lite Version"), EditorUtils._titleButtonStyle, GUILayout.Height(20)))
            {
                string _path = EditorUtility.OpenFilePanel("Load the settings", Application.dataPath, "txt");
                if (!string.IsNullOrEmpty(_path))
                {
                    string _json = File.ReadAllText(_path, System.Text.Encoding.UTF8);
                    SettingsExport _import = (SettingsExport)JsonUtility.FromJson(_json, typeof(SettingsExport));
                    myTarget.itemTypes = new List<StringColorData>();
                    myTarget.itemTypes.AddRange(_import.itemTypes);

                    myTarget.itemQuality = new List<StringColorData>();
                    myTarget.itemQuality.AddRange(_import.itemQuality);

                    myTarget.clickSettings = new List<ClickSetting>();
                    myTarget.clickSettings.AddRange(_import.clickSettings);

                    GameManager.AttributeData.AttributeList = new List<Attribute>();
                    GameManager.AttributeData.AttributeList.AddRange(_import.itemAttributes);
                    for (int i = 0; i < GameManager.AttributeData.AttributeList.Count; i++)
                    {
                        GameManager.AttributeData.AttributeList[i].uid = _import.AttributeKeys[i];
                    }

                    myTarget.itemEnchantments = new List<Enchantment>();
                    myTarget.itemEnchantments.AddRange(_import.itemEnchantments);

                    myTarget.currencies = new List<Currency>();
                    myTarget.currencies.AddRange(_import.currencies);
                    for (int i = 0; i < _import.currencyIcons.Count; i++)
                    {
                        myTarget.currencies[i].icon = (Sprite)AssetDatabase.LoadAssetByGUID(new GUID(_import.currencyIcons[i]), typeof(Sprite));
                    }

                    myTarget.items = new List<Item>();
                    myTarget.IdManager.Clear();
                    myTarget.items.AddRange(_import.items);
                    for (int i = 0; i < _import.itemIcons.Count; i++)
                    {
                        if (myTarget.items[i].iconLoadMethod == LoadMethod.DirectReference)
                        {
                            myTarget.items[i].uid = myTarget.items[i].name.ToLower().Replace(" ", "_").Replace("'s", "").Replace("'", "");
                            myTarget.items[i].icon = (Texture2D)AssetDatabase.LoadAssetByGUID(new GUID(_import.itemIcons[i]), typeof(Texture2D));
                        }
                        else
                        {
                            EditorAsset.ItemAssets[i].icon = (Texture2D)AssetDatabase.LoadAssetByGUID(new GUID(_import.itemIcons[i]), typeof(Texture2D));
                        }
                    }

                    myTarget.NameAttributeKey = _import.NameAttributeKey;
                    myTarget.LevelAttributeKey = _import.LevelAttributeKey;
                    myTarget.XpAttributeKey = _import.XpAttributeKey;
                    myTarget.MaxXpAttributeKey = _import.MaxXpAttributeKey;
                    myTarget.CoolDownAttributeKey = _import.CoolDownAttributeKey;
                    myTarget.SharedGlobalCoolDown = _import.SharedGlobalCoolDown;
                    myTarget.AttributeNameColor = _import.AttributeNameColor;
                    myTarget.UseQualityColorForItemName = _import.UseQualityColorForItemName;
                    myTarget.MerchantStyle = _import.MerchantStyle;
                    myTarget.HighlightEquipmentSlotWhenHoverItem = _import.HighlightEquipmentSlotWhenHoverItem;
                    myTarget.AllowDropItem = _import.AllowDropItem;
                    myTarget.CanvasTag = _import.CanvasTag;
                    myTarget.EnableCrafting = _import.EnableCrafting;
                    myTarget.CraftingMaterialCategoryID = _import.CraftingMaterialCategoryID;
                    myTarget.CraftingBlueprintTag = _import.CraftingBlueprintTag;
                    myTarget.PlayerName = _import.PlayerName;
                    myTarget.CraftingTime = _import.CraftingTime;
                    myTarget.EnhancingMaterials = _import.EnhancingMaterials;
                    myTarget.EnhancingCurrencyType = _import.EnhancingCurrencyType;
                    myTarget.EnhancingCurrencyNeed = _import.EnhancingCurrencyNeed;
                    myTarget.EnableEnhancing = _import.EnableEnhancing;
                    myTarget.DestroyEquipmentWhenFail = _import.DestroyEquipmentWhenFail;
                    myTarget.DestroyEquipmentWhenFailLevel = _import.DestroyEquipmentWhenFailLevel;
                    myTarget.MaxiumEnhancingLevel = _import.MaxiumEnhancingLevel;
                    myTarget.EnhancingSuccessCurve = _import.EnhancingSuccessCurve;
                    myTarget.EnhancingCategoryID = _import.EnhancingCategoryID;
                    myTarget.EnhancingTime = _import.EnhancingTime;
                    myTarget.EnableEnhancingGlow = _import.EnableEnhancingGlow;
                    myTarget.EnhancingGlowCurve = _import.EnhancingGlowCurve;
                    myTarget.EnchantingMaterial = _import.EnchantingMaterial;
                    myTarget.EnchantingCurrencyType = _import.EnchantingCurrencyType;
                    myTarget.EnchantingCurrencyNeed = _import.EnchantingCurrencyNeed;
                    myTarget.EnchantmentNumberRange = _import.EnchantmentNumberRange;
                    myTarget.EnableEnchanting = _import.EnableEnchanting;
                    myTarget.RandomEnchantmentsForNewItem = _import.RandomEnchantmentsForNewItem;
                    myTarget.EnchantingSuccessRate = _import.EnchantingSuccessRate;
                    myTarget.EnchantingCategoryID = _import.EnchantingCategoryID;
                    myTarget.EnchantingTime = _import.EnchantingTime;
                    myTarget.EnchantingPrefixesColor = _import.EnchantingPrefixesColor;
                    myTarget.EnchantingSuffxesColor = _import.EnchantingSuffxesColor;
                    myTarget.EnchantingNameColor = _import.EnchantingNameColor;
                    myTarget.EnableSocketing = _import.EnableSocketing;
                    myTarget.SocketingTagFilter = _import.SocketingTagFilter;
                    myTarget.SocketingCategoryFilter = _import.SocketingCategoryFilter;
                    myTarget.SocketedCategoryFilter = _import.SocketedCategoryFilter;
                    myTarget.EnableRemoveSocketing = _import.EnableRemoveSocketing;
                    myTarget.RemoveSocketingPrice = _import.RemoveSocketingPrice;
                    myTarget.RemoveSocketingCurrency = _import.RemoveSocketingCurrency;
                    myTarget.DestroySocketItemWhenRemove = _import.DestroySocketItemWhenRemove;
                    myTarget.RandomSocketingSlotsNummber = _import.RandomSocketingSlotsNummber;
                    myTarget.MinimalSocketingSlotsNumber = _import.MinimalSocketingSlotsNumber;
                    myTarget.MaxmiumSocketingSlotsNumber = _import.MaxmiumSocketingSlotsNumber;
                    myTarget.LockSocketingSlotsByDefault = _import.LockSocketingSlotsByDefault;
                    myTarget.RandomChanceToLockSocketingSlots = _import.RandomChanceToLockSocketingSlots;
                    myTarget.UnlockSocketingSlotsPrice = _import.UnlockSocketingSlotsPrice;
                    myTarget.UnlockSocketingSlotsCurrency = _import.UnlockSocketingSlotsCurrency;
                    myTarget.msgBagFull = _import.msgBagFull;
                    myTarget.msgItemUseRestricted = _import.msgItemUseRestricted;
                    myTarget.msgItemAssign = _import.msgItemAssign;
                    myTarget.msgEnhancingFail = _import.msgEnhancingFail;

                    GameManager.AttributeData.GenerateUniqueHash();
                    EditorUtility.SetDirty(GameManager.AttributeData);
                    ApplyAndSave(myTarget, EditorAsset);
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Success!", "Settings loaded from: " + _path, "OK");
                    GUILayout.BeginHorizontal();
                }
                EditorGUI.FocusTextInControl(null);
            }
            GUI.backgroundColor = new Color(1F, 0.8F, 0.2F, 1F);
            if (GUILayout.Button(new GUIContent("?", "Open online guide page."), GUILayout.Width(18), GUILayout.Height(18)))
            {
                Application.OpenURL("https://www.soft-kitty.com/docs/master-inventory-engine/getting-started/upgrade");
                EditorGUI.FocusTextInControl(null);
            }

            GUILayout.FlexibleSpace();
            GUI.backgroundColor = EditorUtils._black;
            if (GUILayout.Button("Clear Editor Game Save"))
            {
                DirectoryInfo _dir = new DirectoryInfo(Application.dataPath + "/../SaveData/");
                foreach (var obj in _dir.GetFiles())
                {
                    File.Delete(obj.FullName);
                }
            }
            EditorUtils.ResetColor();
            GUILayout.Space(17);
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._skinExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._skinExpand ? "-" : "+") + " Interface Skin", "Skin Settings of the inventory interfaces."), EditorUtils._titleButtonStyle))
            {
                myTarget._skinExpand = !myTarget._skinExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._skinExpand)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Overall UI Style:", GUILayout.Width(150));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUI.backgroundColor = myTarget.UiStyle == 1 ? EditorUtils._activeColor : EditorUtils._disableColor;
                if (GUILayout.Button("Immersive", GUILayout.Width(75)))
                {
                    myTarget.UiStyle = 1;
                    File.Copy(_basePath + "Style1.png", _targetPath, true);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(_PackagePath + "Textures/Sprites/Main.png");
                    importer.filterMode = FilterMode.Bilinear;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    AssetDatabase.Refresh();
                    myTarget.UpdatePrefab();
                }
                GUI.backgroundColor = myTarget.UiStyle == 2 ? EditorUtils._activeColor : EditorUtils._disableColor;
                if (GUILayout.Button("Simple", GUILayout.Width(75)))
                {
                    myTarget.UiStyle = 2;
                    File.Copy(_basePath + "Style2.png", _targetPath, true);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(_PackagePath + "Textures/Sprites/Main.png");
                    importer.filterMode = FilterMode.Bilinear;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    AssetDatabase.Refresh();
                    myTarget.UpdatePrefab();
                }
                GUI.backgroundColor = myTarget.UiStyle == 3 ? EditorUtils._activeColor : EditorUtils._disableColor;
                if (GUILayout.Button("Flat", GUILayout.Width(75)))
                {
                    myTarget.UiStyle = 3;
                    File.Copy(_basePath + "Style3.png", _targetPath, true);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(_PackagePath + "Textures/Sprites/Main.png");
                    importer.filterMode = FilterMode.Bilinear;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    AssetDatabase.Refresh();
                    myTarget.UpdatePrefab();
                }
                GUI.backgroundColor = myTarget.UiStyle == 4 ? EditorUtils._activeColor : EditorUtils._disableColor;
                if (GUILayout.Button("Pixel", GUILayout.Width(75)))
                {
                    myTarget.UiStyle = 4;
                    File.Copy(_basePath + "Style4.png", _targetPath, true);
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(_PackagePath + "Textures/Sprites/Main.png");
                    importer.filterMode = FilterMode.Point;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    AssetDatabase.Refresh();
                    myTarget.UpdatePrefab();
                }
                EditorUtils.ResetColor();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                EditorGUILayout.Separator();

                float _oldScale = myTarget.UiScale;
                EditorUtils.FloatSlider(ref myTarget.UiScale, 1F, 2F, new GUIContent("Ui Window Scale:"), "x", 20, 150, true, 1F);
                if (_oldScale != myTarget.UiScale && !Application.isPlaying)
                {

                    AnimationClip _clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(_PackagePath + "Animations/WindowOpen.anim", typeof(AnimationClip));
                    EditorCurveBinding[] _bindings = AnimationUtility.GetCurveBindings(_clip);
                    foreach (EditorCurveBinding obj in _bindings)
                    {
                        AnimationCurve _curve = AnimationUtility.GetEditorCurve(_clip, obj);
                        Keyframe[] _keys = new Keyframe[_curve.keys.Length];
                        for (int i = 0; i < _keys.Length; i++)
                        {
                            _keys[i].time = _curve.keys[i].time;
                            _keys[i].value = _curve.keys[i].value;
                            _keys[i].inTangent = _curve.keys[i].inTangent;
                            _keys[i].outTangent = _curve.keys[i].outTangent;
                            _keys[i].inWeight = _curve.keys[i].inWeight;
                            _keys[i].outWeight = _curve.keys[i].outWeight;
                        }
                        _keys[_curve.keys.Length - 1].value = myTarget.UiScale;
                        _curve.keys = _keys;
                        AnimationUtility.SetEditorCurve(_clip, obj, _curve);
                    }
                    myTarget.UpdatePrefab();
                }

                EditorUtils.FloatSlider(ref myTarget.InventorySlotScale, 1F, 2F, new GUIContent("Item Slots Scale:"), "x", 20, 150, true, 1F);
                EditorUtils.ColorField(ref myTarget.EmptyItemBackColor, new GUIContent("Empty Item Slot:"), 20, 150,
                    true, new Color(0.33F, 0.33F, 0.33F, 1F));
                EditorUtils.ColorField(ref myTarget.ItemSelectedColor, new GUIContent("Selected Item Outline:"), 20, 150,
                    true, new Color(1F, 0.28F, 0F, 0.4F));
                EditorUtils.ColorField(ref myTarget.ItemHoverColor, new GUIContent("Mouse Hover Item Effect:"), 20, 150,
                    true, new Color(1F, 0.54F, 0F, 0.1F));
                EditorUtils.ColorField(ref myTarget.FavoriteColor, new GUIContent("Favorite Item Indicator:"), 20, 150,
                    true, new Color(1F, 0.54F, 0F, 1F));
                EditorGUILayout.Separator();
            }


            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._generalExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._generalExpand ? "-" : "+") + " General Settings", "General Settings of the inventory system."), EditorUtils._titleButtonStyle))
            {
                myTarget._generalExpand = !myTarget._generalExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._generalExpand)
            {

                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.TextField(ref myTarget.CanvasTag, EditorUtils.TextHint("Canvas GameObject Tag:", "[Optional] Specify the tag of your main Canvas GameObject. This ensures the system finds the correct canvas, especially in scenes with multiple canvases."), 20, 150, true);
                EditorUtils.HelpInfo("[Optional] Specify the tag of your main Canvas GameObject. This ensures the system finds the correct canvas, especially in scenes with multiple canvases.", 20);

                EditorGUILayout.Separator();

                EditorUtils.Toggle(ref myTarget.AllowDropItem, EditorUtils.TextHint("Allow player drop item by dragging out of the window."), 20, 350);

                string[] _styleNames = new string[2] { "Simple Style", "DOS Style" };
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.PopUp(ref myTarget.MerchantStyle, _styleNames,
                           EditorUtils.TextHint("Merchant interface style:"), 20, 150, 150);

                EditorUtils.Toggle(ref myTarget.HighlightEquipmentSlotWhenHoverItem, EditorUtils.TextHint("Highlight associated slot when hover over equipment item."), 20, 350);
                EditorUtils.Toggle(ref myTarget.AllowDropItem, EditorUtils.TextHint("Allow player drop item by dragging out of the window."), 20, 350);

                string[] _filterNames = new string[2] { "Dim Non-Matching", "Hide Non-Matching" };

                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.PopUp(ref myTarget.SearchFilterMode, _filterNames,
                           EditorUtils.TextHint("Search Filter Mode:", "Determines how item slots behave when using the search field.\nDim Non - Matching: Non - matching items remain visible but semi - transparent.\nHide Non - Matching: Non - matching items are fully hidden and the layout collapses to show only matching results."), 20, 150, 150);
                EditorUtils.Toggle(ref myTarget.MergeStacksOnSort, EditorUtils.TextHint("Combines partial stacks into larger stacks during sorting.", "If enabled, compatible item stacks will automatically merge up to their maximum stack size when sorting the inventory."), 20, 350);


                EditorGUILayout.Separator();


                EditorUtils.Pre(EditorUtils.TextHint("Character Name Attribute:", "Select the attribute you've created to represent the character's name. Ensure it's a string value, with no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData."), 20, 150);
                int _keySel = 0;
                for (int i = 0; i < GameManager.AttributeData.AttributesUidArray.Length; i++)
                {
                    if (GameManager.AttributeData.AttributesUidArray[i] == myTarget.NameAttributeKey) _keySel = i;
                }
                GUI.backgroundColor = EditorUtils._titleColor;
                EditorGUI.BeginChangeCheck();
                _keySel = EditorGUILayout.Popup(_keySel, GameManager.AttributeData.AttributesNames, GUILayout.Width(100F));
                if (EditorGUI.EndChangeCheck()) myTarget.NameAttributeKey = GameManager.AttributeData.AttributesUidArray[_keySel];
                EditorUtils.ResetColor();
                EditorUtils.End(20);
                EditorUtils.HelpInfo("Select the attribute you've created to represent the character's name. Ensure it's a string value, with no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData.", 20);

                EditorGUILayout.Separator();

                EditorUtils.Pre(EditorUtils.TextHint("Character Level Attribute:", "Select the attribute you've created to represent the character's level. Ensure no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData."), 20, 150);
                _keySel = 0;
                for (int i = 0; i < GameManager.AttributeData.AttributesUidArray.Length; i++)
                {
                    if (GameManager.AttributeData.AttributesUidArray[i] == myTarget.LevelAttributeKey) _keySel = i;
                }
                GUI.backgroundColor = EditorUtils._titleColor;
                EditorGUI.BeginChangeCheck();
                _keySel = EditorGUILayout.Popup(_keySel, GameManager.AttributeData.AttributesNames, GUILayout.Width(100F));
                if (EditorGUI.EndChangeCheck()) myTarget.LevelAttributeKey = GameManager.AttributeData.AttributesUidArray[_keySel];
                EditorUtils.ResetColor();
                EditorUtils.End(20);
                EditorUtils.HelpInfo("Select the attribute you've created to represent the character's level. Ensure no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData.", 20);

                EditorGUILayout.Separator();

                EditorUtils.Pre(EditorUtils.TextHint("Character XP Attribute:", "Select the attribute you've created to represent the character's XP points. Ensure no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData."), 20, 150);
                _keySel = 0;
                for (int i = 0; i < GameManager.AttributeData.AttributesUidArray.Length; i++)
                {
                    if (GameManager.AttributeData.AttributesUidArray[i] == myTarget.XpAttributeKey) _keySel = i;
                }
                GUI.backgroundColor = EditorUtils._titleColor;
                EditorGUI.BeginChangeCheck();
                _keySel = EditorGUILayout.Popup(_keySel, GameManager.AttributeData.AttributesNames, GUILayout.Width(100F));
                if (EditorGUI.EndChangeCheck()) myTarget.XpAttributeKey = GameManager.AttributeData.AttributesUidArray[_keySel];
                EditorUtils.ResetColor();
                EditorUtils.End(20);
                EditorUtils.HelpInfo("Select the attribute you've created to represent the character's XP points. Ensure no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData.", 20);

                EditorGUILayout.Separator();

                EditorUtils.Pre(EditorUtils.TextHint("Character Max XP Attribute:", "Select the attribute you've created to represent the character's Max XP points. Ensure no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData."), 20, 150);
                _keySel = 0;
                for (int i = 0; i < GameManager.AttributeData.AttributesUidArray.Length; i++)
                {
                    if (GameManager.AttributeData.AttributesUidArray[i] == myTarget.MaxXpAttributeKey) _keySel = i;
                }
                GUI.backgroundColor = EditorUtils._titleColor;
                EditorGUI.BeginChangeCheck();
                _keySel = EditorGUILayout.Popup(_keySel, GameManager.AttributeData.AttributesNames, GUILayout.Width(100F));
                if (EditorGUI.EndChangeCheck()) myTarget.MaxXpAttributeKey = GameManager.AttributeData.AttributesUidArray[_keySel];
                EditorUtils.ResetColor();
                EditorUtils.End(20);
                EditorUtils.HelpInfo("Select the attribute you've created to represent the character's Max XP points. Ensure no visibility options enabled, and added to the Player/NPC's Base Stats in the InventoryData.", 20);

                EditorGUILayout.Separator();

                EditorUtils.Pre(EditorUtils.TextHint("Item Cool Down Attribute:", "Select the numerical attribute you've created to represent cool down time. Ensure it's added to the items with cool down functionality."), 20, 150);
                _keySel = 0;
                for (int i = 0; i < GameManager.AttributeData.AttributesUidArray.Length; i++)
                {
                    if (GameManager.AttributeData.AttributesUidArray[i] == myTarget.CoolDownAttributeKey) _keySel = i;
                }
                GUI.backgroundColor = EditorUtils._titleColor;
                EditorGUI.BeginChangeCheck();
                _keySel = EditorGUILayout.Popup(_keySel, GameManager.AttributeData.AttributesNames, GUILayout.Width(100F));
                if (EditorGUI.EndChangeCheck()) myTarget.CoolDownAttributeKey = GameManager.AttributeData.AttributesUidArray[_keySel];
                EditorUtils.ResetColor();
                EditorUtils.End(20);
                EditorUtils.HelpInfo("Select the numerical attribute you've created to represent cool down time. Ensure it's added to the items with cool down functionality.", 20);

                EditorGUILayout.Separator();

                EditorUtils.FloatSlider(ref myTarget.SharedGlobalCoolDown, 0F, 2F, new GUIContent("Shared Global Cool Down Time:", "Sets a global cool down. When any item/skill is used, all usable items/skills enter this shared cool down period."), "sec", 20, 150);
                EditorUtils.HelpInfo("Sets a global cool down. When any item/skill is used, all usable items/skills enter this shared cool down period.", 20);

                EditorGUILayout.Separator();


                if (myTarget.clickSettings.Count < 4)
                {
                    if (EditorUtils.TitleBox(EditorUtils.TextHint("Input Binding For Inventory Items:"), EditorUtils._titleColor, 20, true, "Add", 50))
                    {
                        if (GUILayout.Button("Add", GUILayout.Width(50)))
                        {
                            ClickSetting _newSetting = new ClickSetting();
                            _newSetting.key = AlterKeys.None;
                            _newSetting.mouseButton = MouseButtons.LeftClick;
                            _newSetting.function = ClickFunctions.Use;
                            myTarget.clickSettings.Add(_newSetting);
                            EditorGUI.FocusTextInControl(null);
                            _valueChanged = true;
                        }
                    }
                }
                else
                {
                    EditorUtils.TitleBox(EditorUtils.TextHint("Input Binding For Inventory Items:"), EditorUtils._titleColor, 20);
                }


                for (int i = 0; i < myTarget.clickSettings.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    GUI.backgroundColor = EditorUtils._black;
                    AlterKeys _key = myTarget.clickSettings[i].key;
                    EditorGUI.BeginChangeCheck();
                    _key = (AlterKeys)EditorGUILayout.EnumPopup(_key, GUILayout.Width(100));
                    if (EditorGUI.EndChangeCheck()) myTarget.clickSettings[i].key = _key;

                    GUILayout.Label("+", GUILayout.Width(15));

                    MouseButtons _mouse = myTarget.clickSettings[i].mouseButton;
                    EditorGUI.BeginChangeCheck();
                    _mouse = (MouseButtons)EditorGUILayout.EnumPopup(_mouse, GUILayout.Width(100));
                    if (EditorGUI.EndChangeCheck()) myTarget.clickSettings[i].mouseButton = _mouse;

                    GUILayout.Label("=", GUILayout.Width(15));

                    ClickFunctions _function = myTarget.clickSettings[i].function;
                    EditorGUI.BeginChangeCheck();
                    _function = (ClickFunctions)EditorGUILayout.EnumPopup(_function, GUILayout.Width(100));
                    if (EditorGUI.EndChangeCheck()) myTarget.clickSettings[i].function = _function;
                    GUILayout.Space(10);
                    GUI.backgroundColor = EditorUtils._black;
                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        myTarget.clickSettings.RemoveAt(i);
                        EditorGUI.FocusTextInControl(null);
                        _valueChanged = true;
                    }
                    GUI.backgroundColor = Color.white;
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();


                EditorUtils.TitleBox(EditorUtils.TextHint("Window UI Prefabs:"), EditorUtils._titleColor, 20);
                EditorUtils.TextField(ref myTarget.PlayerInventoryWindowName, EditorUtils.TextHint("Player Inventory:"), 40, 150, true);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.PlayerInventoryWindowName + ".prefab</color>"), 40);
                EditorGUILayout.Separator();
                EditorUtils.TextField(ref myTarget.PlayerEquipWindowName, EditorUtils.TextHint("Player Equipment:"), 40, 150, true);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.PlayerEquipWindowName + ".prefab</color>"), 40);
                EditorGUILayout.Separator();
                EditorUtils.TextField(ref myTarget.MerchantWindowName, EditorUtils.TextHint("Merchant:"), 40, 150, true);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.MerchantWindowName + ".prefab</color> [Simple Style]"), 40);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.MerchantWindowName + "1.prefab</color> [DOS Style]"), 40);
                EditorGUILayout.Separator();
                EditorUtils.TextField(ref myTarget.NpcInventoryWindowName, EditorUtils.TextHint("Npc Inventory:"), 40, 150, true);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.NpcInventoryWindowName + ".prefab</color>"), 40);
                EditorGUILayout.Separator();
                EditorUtils.TextField(ref myTarget.NpcEquipmentWindowName, EditorUtils.TextHint("Npc Equipment:"), 40, 150, true);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.NpcEquipmentWindowName + ".prefab</color>"), 40);
                EditorGUILayout.Separator();
                EditorUtils.TextField(ref myTarget.StorageWindowName, EditorUtils.TextHint("Storage/Crate:"), 40, 150, true);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.StorageWindowName + ".prefab</color>"), 40);
                EditorGUILayout.Separator();
                EditorUtils.TextField(ref myTarget.ForgeWindowName, EditorUtils.TextHint("Forge:"), 40, 150, true);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.BoxInfo(new GUIContent("<color=#5588BB>Resources/InventoryEngine/WindowsManager/" + myTarget.ForgeWindowName + ".prefab</color>"), 40);
                EditorGUILayout.Separator();


                EditorGUILayout.Separator();
                EditorGUILayout.Separator();


                EditorUtils.TitleBox(EditorUtils.TextHint("Dynamic Messages:"), EditorUtils._titleColor, 20);

                EditorUtils.Label(EditorUtils.TextHint("Adding item when bag is full:"), 40);
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.TextField(ref myTarget.msgBagFull, EditorUtils.TextHint("-"), 40, 20);
                EditorGUILayout.Separator();
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.Label(EditorUtils.TextHint("Can not use item because of the restriction setting:"), 40);
                EditorUtils.TextField(ref myTarget.msgItemUseRestricted, EditorUtils.TextHint("-"), 40, 20);
                EditorUtils.HelpInfo("{name} = restriction attribute name; {value} = restriction attribute value required;", 60);
                EditorGUILayout.Separator();
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.Label(EditorUtils.TextHint("Can not assign item to a slot:"), 40);
                EditorUtils.TextField(ref myTarget.msgItemAssign, EditorUtils.TextHint("-"), 40, 20);
                EditorGUILayout.Separator();
                GUI.backgroundColor = EditorUtils._black;
                EditorUtils.Label(EditorUtils.TextHint("Failed when enhancing an item:"), 40);
                EditorUtils.TextField(ref myTarget.msgEnhancingFail, EditorUtils.TextHint("-"), 40, 20);
                EditorUtils.HelpInfo("{name} = the item name;", 60);
                EditorGUILayout.Separator();

            }

            //////////Item Types
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._typeExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._typeExpand ? "-" : "+") + " Item Category Settings (" + myTarget.itemTypes.Count.ToString() + ")",
                    "You can create as many categories for your items as you like (though it¡¯s recommended to keep it to fewer than 5 visible categories due to UI space limitations)."), EditorUtils._titleButtonStyle))
            {
                myTarget._typeExpand = !myTarget._typeExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._typeExpand)
            {
                EditorUtils.HelpInfo("You can create as many categories for your items as you like (though it¡¯s recommended to keep it to fewer than 5 visible categories due to UI space limitations).", 20);
                if (EditorUtils.TitleBox(EditorUtils.TextHint("Categories (" + myTarget.itemTypes.Count.ToString() + ") :"), EditorUtils._titleColor, 20, true, "Add New [Category]", 150))
                {
                    StringColorData _newType = new StringColorData();
                    _newType.name = "New Category" + myTarget.itemTypes.Count.ToString();
                    _newType.color = Color.white;
                    _newType.visible = true;
                    myTarget.itemTypes.Add(_newType);
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }

                for (int i = 0; i < myTarget.itemTypes.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.backgroundColor = EditorUtils._black;
                    GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                    GUI.color = EditorUtils._titleColor;
                    GUILayout.Box(new GUIContent("(ID: " + i.ToString() + ")", "When accessing this catergory in your script, you will need to use this ID."), EditorUtils._boxStyle, GUILayout.Width(60));
                    EditorUtils.ResetColor();
                    GUILayout.Label(new GUIContent("Name:", " This name will be displayed as tab cards at the top of the inventory UI."), GUILayout.Width(40));
                    GUI.backgroundColor = EditorUtils._black;
                    myTarget.itemTypes[i].name = GUILayout.TextField(myTarget.itemTypes[i].name, GUILayout.Width(120));
                    EditorUtils.ResetColor();
                    myTarget.itemTypes[i].color = EditorGUILayout.ColorField(myTarget.itemTypes[i].color, GUILayout.Width(50));
                    myTarget.itemTypes[i].visible = GUILayout.Toggle(myTarget.itemTypes[i].visible, new GUIContent("visible", "Determines if this category shows in inventory interface."), GUILayout.Width(60));
                    GUILayout.FlexibleSpace();
                    if (i > 0)
                    {
                        GUI.backgroundColor = EditorUtils._black;
                        if (GUILayout.Button(new GUIContent("X", "Delete this catergory."), GUILayout.Width(25)))
                        {
                            if (myTarget.DataMode == ItemDataMode.Unified)
                            {
                                for (int w = 0; w < myTarget.items.Count; w++)
                                {
                                    if (myTarget.items[w].type > i)
                                    {
                                        myTarget.items[w].type -= 1;
                                    }
                                }
                            }
                            else
                            {
                                for (int w = 0; w < myTarget.itemScriptableObjects.Count; w++)
                                {
                                    if (myTarget.itemScriptableObjects[w].mItem.type > i)
                                    {
                                        myTarget.itemScriptableObjects[w].mItem.type -= 1;
                                    }
                                }
                            }
                            if (myTarget.SocketedCategoryFilter > i) myTarget.SocketedCategoryFilter -= 1;
                            if (myTarget.SocketingCategoryFilter > i) myTarget.SocketingCategoryFilter -= 1;
                            if (myTarget.EnhancingCategoryID > i) myTarget.EnhancingCategoryID -= 1;
                            if (myTarget.EnchantingCategoryID > i) myTarget.EnchantingCategoryID -= 1;
                            if (myTarget.CraftingMaterialCategoryID > i) myTarget.CraftingMaterialCategoryID -= 1;
                            myTarget.itemTypes.RemoveAt(i);
                            _valueChanged = true;
                            EditorGUI.FocusTextInControl(null);
                            ApplyAndSave(myTarget, EditorAsset);
                            return;
                        }

                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.Separator();
            }



            /// Quality
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._qualityExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._qualityExpand ? "-" : "+") + " Item Quality Settings (" + myTarget.itemQuality.Count.ToString() + ")",
                    "You can create as many quality levels as needed. Each quality will have an index displayed as blue text at the front. When your script accesses the quality attribute of an item, the value will correspond to the index of the quality levels you set here."), EditorUtils._titleButtonStyle))
            {
                myTarget._qualityExpand = !myTarget._qualityExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._qualityExpand)
            {
                EditorUtils.HelpInfo("You can create as many quality levels as needed. Each quality will have an index displayed as blue text at the front. When your script accesses the quality attribute of an item, the value will correspond to the index of the quality levels you set here.", 20);
                EditorUtils.Toggle(ref myTarget.UseQualityColorForItemName, EditorUtils.TextHint("Use [Quality] Color for item name text.", "When enabled, the item name text will use the same color as the quality level of this item."), 20, 220);

                if (EditorUtils.TitleBox(EditorUtils.TextHint("Quality Levels (" + myTarget.itemQuality.Count.ToString() + ") :"), EditorUtils._titleColor, 20, true, "Add New [Quality]", 150))
                {
                    StringColorData _newType = new StringColorData();
                    _newType.name = "New Quality" + myTarget.itemQuality.Count.ToString();
                    _newType.color = Color.white;
                    _newType.visible = true;
                    myTarget.itemQuality.Add(_newType);
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }

                for (int i = 0; i < myTarget.itemQuality.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.backgroundColor = EditorUtils._black;
                    GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                    GUI.backgroundColor = Color.black;
                    GUI.color = EditorUtils._titleColor;
                    GUILayout.Box(new GUIContent("(ID: " + i.ToString() + ")", "When accessing this quality level from your script, you will need to use this ID."), EditorUtils._boxStyle, GUILayout.Width(60));
                    EditorUtils.ResetColor();
                    GUILayout.Label(new GUIContent("Name:", "This name will be shown in the detailed information panel of items."), GUILayout.Width(40));
                    GUI.backgroundColor = EditorUtils._black;
                    myTarget.itemQuality[i].name = GUILayout.TextField(myTarget.itemQuality[i].name, GUILayout.Width(150));
                    EditorUtils.ResetColor();
                    myTarget.itemQuality[i].color = EditorGUILayout.ColorField(myTarget.itemQuality[i].color, GUILayout.Width(50));
                    GUILayout.FlexibleSpace();
                    if (i > 0)
                    {
                        GUI.backgroundColor = EditorUtils._black;
                        if (GUILayout.Button(new GUIContent("X", "Delete this quality level."), GUILayout.Width(25)))
                        {
                            if (myTarget.DataMode == ItemDataMode.Unified)
                            {
                                for (int w = 0; w < myTarget.items.Count; w++)
                                {
                                    if (myTarget.items[w].quality > i)
                                    {
                                        myTarget.items[w].quality -= 1;
                                    }
                                }
                            }
                            else
                            {
                                for (int w = 0; w < myTarget.itemScriptableObjects.Count; w++)
                                {
                                    if (myTarget.itemScriptableObjects[w].mItem.quality > i)
                                    {
                                        myTarget.itemScriptableObjects[w].mItem.quality -= 1;
                                    }
                                }
                            }

                            myTarget.itemQuality.RemoveAt(i);
                            _valueChanged = true;
                            EditorGUI.FocusTextInControl(null);
                            ApplyAndSave(myTarget, EditorAsset);
                            return;
                        }

                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.Separator();
            }



            ///Currency
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._currencyExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._currencyExpand ? "-" : "+") + " Currency Settings (" + myTarget.currencies.Count.ToString() + ")",
                    "You can create as many currency types as needed. Each currency will have an index displayed as blue text at the front. When your script accesses the currency value of an <InventoryData> component, you¡¯ll need to call <InventoryData>().GetCurrency(int _type) using the index number as the parameter."), EditorUtils._titleButtonStyle))
            {
                myTarget._currencyExpand = !myTarget._currencyExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._currencyExpand)
            {
                EditorUtils.HelpInfo("You can create as many currency types as needed. Each currency will have an index displayed as blue text at the front. When your script accesses the currency value of an <InventoryData> class, you¡¯ll need to call <InventoryData>().GetCurrency(int _type) using the index number as the parameter. ", 20);
                if (EditorUtils.TitleBox(EditorUtils.TextHint("Currencies (" + myTarget.currencies.Count.ToString() + ") :"), EditorUtils._titleColor, 20, true, "Add New [Currency]", 150))
                {
                    Currency _newCurrency = new Currency();
                    _newCurrency.name = "Currency" + myTarget.currencies.Count.ToString();
                    _newCurrency.color = Color.white;
                    _newCurrency.icon = null;
                    _newCurrency.ExchangeRate = new List<Vector3>();
                    _newCurrency.fold = true;
                    myTarget.currencies.Add(_newCurrency);
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }

                for (int i = 0; i < myTarget.currencies.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUI.backgroundColor = EditorUtils._black;
                    GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                    GUI.backgroundColor = myTarget.currencies[i].fold ? EditorUtils._disableColor2 : EditorUtils._activeColor2;
                    if (GUILayout.Button(myTarget.currencies[i].fold ? "+" : "-", GUILayout.Width(20)))
                    {
                        myTarget.currencies[i].fold = !myTarget.currencies[i].fold;
                        EditorGUI.FocusTextInControl(null);
                    }
                    EditorUtils.ResetColor();
                    GUI.color = EditorUtils._titleColor;
                    GUI.backgroundColor = Color.black;
                    GUILayout.Box(new GUIContent("(ID: " + i.ToString() + ")", "When accessing this currency from you script, you will need to use this ID."), EditorUtils._boxStyle, GUILayout.Width(60));
                    EditorUtils.ResetColor();
                    GUI.backgroundColor = EditorUtils._black;
                    myTarget.currencies[i].name = GUILayout.TextField(myTarget.currencies[i].name, GUILayout.Width(100));
                    EditorUtils.ResetColor();
                    myTarget.currencies[i].color = EditorGUILayout.ColorField(myTarget.currencies[i].color, GUILayout.Width(50));
                    myTarget.currencies[i].icon = (Sprite)EditorGUILayout.ObjectField(myTarget.currencies[i].icon, typeof(Sprite), false, GUILayout.Width(100));
                    GUI.backgroundColor = EditorUtils._black;
                    GUILayout.FlexibleSpace();
                    if (i > 0)
                    {
                        if (GUILayout.Button(new GUIContent("X", "Delete this currency"), GUILayout.Width(25)))
                        {
                            if (myTarget.DataMode == ItemDataMode.Unified)
                            {
                                for (int w = 0; w < myTarget.items.Count; w++)
                                {
                                    if (myTarget.items[w].currency > i)
                                    {
                                        myTarget.items[w].currency -= 1;
                                    }
                                }
                            }
                            else
                            {
                                for (int w = 0; w < myTarget.itemScriptableObjects.Count; w++)
                                {
                                    if (myTarget.itemScriptableObjects[w].mItem.currency > i)
                                    {
                                        myTarget.itemScriptableObjects[w].mItem.currency -= 1;
                                    }
                                }
                            }
                            if (myTarget.RemoveSocketingCurrency > i) myTarget.RemoveSocketingCurrency -= 1;
                            if (myTarget.UnlockSocketingSlotsCurrency > i) myTarget.UnlockSocketingSlotsCurrency -= 1;
                            if (myTarget.EnchantingCurrencyType > i) myTarget.EnchantingCurrencyType -= 1;
                            if (myTarget.EnhancingCurrencyType > i) myTarget.EnhancingCurrencyType -= 1;
                            myTarget.currencies.RemoveAt(i);
                            for (int u = 0; u < myTarget.currencies.Count; u++)
                            {
                                for (int y = 0; y < myTarget.currencies[u].ExchangeRate.Count; y++)
                                {
                                    if (Mathf.FloorToInt(myTarget.currencies[u].ExchangeRate[y].x) == i)
                                    {
                                        myTarget.currencies[u].ExchangeRate.RemoveAt(y);
                                    }
                                }
                            }
                            _valueChanged = true;
                            EditorGUI.FocusTextInControl(null);
                            ApplyAndSave(myTarget, EditorAsset);
                            return;
                        }
                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(30);
                    GUILayout.EndHorizontal();

                    if (!myTarget.currencies[i].fold)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(40);
                        GUILayout.BeginVertical(EditorUtils._groupStyle);

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUI.backgroundColor = EditorUtils._buttonColor;
                        if (GUILayout.Button("Add Exchange Rate", GUILayout.Width(130)))
                        {
                            EditorGUI.FocusTextInControl(null);
                            myTarget.currencies[i].ExchangeRate.Add(new Vector3(0F, 1F, 1F));
                            _valueChanged = true;
                        }
                        EditorUtils.ResetColor();
                        GUILayout.EndHorizontal();

                        for (int u = 0; u < myTarget.currencies[i].ExchangeRate.Count; u++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            GUILayout.Label("1x = ", GUILayout.Width(30));
                            int _selCurrency = Mathf.FloorToInt(myTarget.currencies[i].ExchangeRate[u].x);
                            int _num = Mathf.FloorToInt(myTarget.currencies[i].ExchangeRate[u].y);
                            bool _autoExchange = (myTarget.currencies[i].ExchangeRate[u].z > 0F);
                            GUI.backgroundColor = EditorUtils._black;
                            EditorGUI.BeginChangeCheck();
                            _selCurrency = EditorGUILayout.Popup(_selCurrency, _currencyOption, GUILayout.Width(80));
                            GUILayout.Label(" x", GUILayout.Width(20));
                            _num = EditorGUILayout.IntField(_num, GUILayout.Width(30));
                            GUILayout.Label("-", GUILayout.Width(10));
                            _autoExchange = GUILayout.Toggle(_autoExchange, "Auto Exchange", GUILayout.Width(115));
                            if (EditorGUI.EndChangeCheck())
                            {
                                myTarget.currencies[i].ExchangeRate[u] = new Vector3(_selCurrency, (float)_num, _autoExchange ? 1F : 0F);
                            }
                            GUI.backgroundColor = EditorUtils._black;
                            if (GUILayout.Button("X", GUILayout.Width(25)))
                            {
                                myTarget.currencies[i].ExchangeRate.RemoveAt(u);
                                EditorGUI.FocusTextInControl(null);
                                _valueChanged = true;
                                ApplyAndSave(myTarget, EditorAsset);
                                return;
                            }
                            EditorUtils.ResetColor();
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndVertical();
                        GUILayout.Space(40);
                        GUILayout.EndHorizontal();
                    }
                }


                EditorGUILayout.Separator();
            }




            ///Craft
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._craftExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._craftExpand ? "-" : "+") + " Crafting Settings",
                    "Players can craft items using other items as materials. They must also have the corresponding blueprint in their inventory as hidden items. For detailed setup instructions, refer to the \"Craft Materials\" section."), EditorUtils._titleButtonStyle))
            {
                myTarget._craftExpand = !myTarget._craftExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._craftExpand)
            {
                if (myTarget.itemTypes.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item category.", 20);
                }
                else
                {
                    EditorUtils.Toggle(ref myTarget.EnableCrafting, EditorUtils.TextHint("Enable Crafting", "If you don¡¯t plan to include a crafting system in your game, uncheck this to disable the feature."), 20, 150);
                    EditorUtils.HelpInfo("Players can CRAFT items using other items as materials. They must also have the corresponding blueprint in their inventory as hidden items. For detailed setup instructions, refer to the Craft Materials section.", 20);
                    if (myTarget.EnableCrafting)
                    {
                        EditorGUILayout.Separator();
                        myTarget.CraftingMaterialCategoryID = Mathf.Clamp(myTarget.CraftingMaterialCategoryID, 0, myTarget.itemTypes.Count - 1);
                        GUI.backgroundColor = EditorUtils._titleColor;
                        EditorUtils.PopUp(ref myTarget.CraftingMaterialCategoryID, _typeOptions,
                           new GUIContent("Material Category:", "Items used as crafting materials must belong to this category."), 20, 150, 200);

                        GUI.backgroundColor = EditorUtils._black;
                        EditorUtils.TextField(ref myTarget.CraftingBlueprintTag, new GUIContent("Blueprint Tag:", "Items used as blueprints must have this tag, refer to the Tags setting section."), 20, 150);
                        EditorUtils.ResetColor();
                        EditorUtils.FloatSlider(ref myTarget.CraftingTime, 0.2F, 30F,
                            new GUIContent("Crafting Time:", "When crafting, a progress bar will fill from 1-100%. This setting determines how long the process will take until the item is created.")
                            , "sec", 20, 150);

                    }
                }
                EditorGUILayout.Separator();
            }



            ///Enchantment
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._entExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._entExpand ? "-" : "+") + " Enchantment Setting (" + myTarget.itemEnchantments.Count.ToString() + ")",
                    "Players can enchant items to grant them additional attributes, which can be either positive or negative. Enchanting requires a specific type of currency and a particular item (such as a \"Magic Scroll\")."), EditorUtils._titleButtonStyle))
            {
                myTarget._entExpand = !myTarget._entExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._entExpand)
            {
                if (GameManager.AttributeData == null || GameManager.AttributeData.AttributeList.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one attribute.", 20);
                }
                else if (myTarget.itemTypes.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item category.", 20);
                }
                else if (myTarget.ItemCount <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item.", 20);
                }
                else
                {
                    EditorUtils.Document("master-inventory-engine/item-class/enchantment");
                    EditorUtils.Toggle(ref myTarget.EnableEnchanting, EditorUtils.TextHint("Enable Enchanting", "Uncheck this if you don't plan to include an enchanting system in your game to disable the feature."), 20, 150);
                    EditorUtils.HelpInfo("Players can ENCHANT items to grant them additional attributes, which can be either positive or negative. Enchanting requires a specific type of currency and a particular item (such as a Magic Scroll).", 20);
                    if (myTarget.EnableEnchanting)
                    {
                        EditorGUILayout.Separator();
                        GUI.backgroundColor = EditorUtils._titleColor;
                        EditorUtils.PopUp(ref myTarget.EnchantingCategoryID, _typeOptions,
                            new GUIContent("Enchanting Category:", "Only items in this category can be enchanted."), 20, 160, 200);

                        EditorGUILayout.Separator();

                        EditorUtils.Label(new GUIContent("Random number of enchantments player can get:", "When enchanting, player will randomly get enchantments by the number of this range."), 20);
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(60);
                        EditorGUILayout.MinMaxSlider(ref myTarget.EnchantmentNumberRange.x, ref myTarget.EnchantmentNumberRange.y, 0, 3);
                        myTarget.EnchantmentNumberRange.x = Mathf.FloorToInt(myTarget.EnchantmentNumberRange.x);
                        myTarget.EnchantmentNumberRange.y = Mathf.FloorToInt(myTarget.EnchantmentNumberRange.y);
                        GUILayout.Label("(" + Mathf.FloorToInt(myTarget.EnchantmentNumberRange.x).ToString() + "~" + Mathf.FloorToInt(myTarget.EnchantmentNumberRange.y).ToString() + ")", GUILayout.Width(70));
                        GUILayout.EndHorizontal();

                        EditorUtils.IntSlider(ref myTarget.EnchantingSuccessRate, 1, 100, new GUIContent("Enchanting Success Rate:", "The percentage chance of success when a player enchants an item."), "%", 20, 160);
                        myTarget.EnchantingCurrencyType = Mathf.Clamp(myTarget.EnchantingCurrencyType, 0, myTarget.currencies.Count - 1);
                        EditorUtils.PopUpIntField(ref myTarget.EnchantingCurrencyType, ref myTarget.EnchantingCurrencyNeed, _currencyOption,
                            new GUIContent("Required Currency:", "Set the currency type and the cost required to enchant an item."), 20,
                            EditorUtils._black, EditorUtils._titleColor, 160, 100, 80);

                        EditorUtils.Pre(new GUIContent("Required Item as Material:", "Select the material item and quantity needed for enchanting."), 20, 160);
                        myTarget.EnchantingMaterial.x = Mathf.Max(myTarget.EnchantingMaterial.x, 0);
                        int _entMatId = Mathf.FloorToInt(myTarget.EnchantingMaterial.x);
                        int _entMat = myTarget.IndexOfItems(_entMatId);
                        ItemIcon(EditorAsset, _entMatId);
                        EditorGUI.BeginChangeCheck();
                        _entMat = EditorGUILayout.Popup("", _entMat, _itemOption, GUILayout.Width(100));
                        if (EditorGUI.EndChangeCheck()) myTarget.EnchantingMaterial = new Vector2(myTarget.GetItemByIndex(_entMat).id, myTarget.EnchantingMaterial.y);
                        GUILayout.Label("x", GUILayout.Width(10));
                        myTarget.EnchantingMaterial.y = Mathf.Clamp(EditorGUILayout.IntField(Mathf.FloorToInt(myTarget.EnchantingMaterial.y), GUILayout.Width(40)), 1, 99);
                        EditorUtils.End(20);

                        EditorUtils.FloatSlider(ref myTarget.EnchantingTime, 0.2F, 3F,
                            new GUIContent("Enchanting Time:", "When enchanting, a progress bar will fill from 1-100%. This setting determines how long the process will take to complete the enchantment."),
                            "sec", 20, 160);

                        EditorGUILayout.Separator();

                        EditorUtils.Toggle(ref myTarget.RandomEnchantmentsForNewItem, new GUIContent("Random Enchantments For New Item", "When using \"new Item(int _uid)\", the enchantments of the item will be randomized."),
                            20, 220);
                        EditorGUILayout.Separator();
                        EditorUtils.ColorField(ref myTarget.EnchantingNameColor, new GUIContent("Enchantment Name Color:", "The color of the enchantment name on the interface."), 20, 160);
                        EditorUtils.ColorField(ref myTarget.EnchantingPrefixesColor, new GUIContent("Enchantment Prefixes Color:", "The prefixes string will be displayed in the front of the item name with this color."), 20, 160);
                        EditorUtils.ColorField(ref myTarget.EnchantingSuffxesColor, new GUIContent("Enchantment Suffxes Color:", "The suffxes string will be displayed in the end of the item name with this color."), 20, 160);
                        EditorGUILayout.Separator();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUI.backgroundColor = EditorUtils._titleColor * 0.7F;
                        GUILayout.BeginHorizontal(EditorUtils._titleButtonStyle);
                        GUILayout.Label(new GUIContent("Enchantments List (" + myTarget.itemEnchantments.Count.ToString() + "):", "Create a list of enchantments, each of which can have multiple attributes."));
                        GUI.backgroundColor = EditorUtils._black;
                        if (GUILayout.Button("Export to Json", GUILayout.Width(110)))
                        {
                            string _path = EditorUtility.SaveFilePanel("Export to Json", Application.dataPath, "EnchantmentSetting.txt", "txt");
                            if (_path.Length != 0)
                            {
                                myTarget.ExportEnchantmentDataToJson(_path);
                            }
                        }
                        if (GUILayout.Button("Import from Json", GUILayout.Width(110)))
                        {
                            string _path = EditorUtility.OpenFilePanel("Import from Json", Application.dataPath, "txt");
                            if (_path.Length != 0)
                            {
                                myTarget.ImportEnchantmentDataFromJson(_path);
                                GUILayout.EndHorizontal();
                                EditorGUI.FocusTextInControl(null);
                                ApplyAndSave(myTarget, EditorAsset);
                                return;
                            }
                        }
                        GUILayout.EndHorizontal();
                        EditorUtils.End(20);

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUI.backgroundColor = EditorUtils._titleColor;
                        if (GUILayout.Button(new GUIContent("Add New [Enchantment]", "Create a new enchantment to the pool."), GUILayout.Width(170)))
                        {
                            Enchantment _newAtt = new Enchantment();
                            _newAtt.name = "New Enchantment" + myTarget.itemEnchantments.Count.ToString();
                            if (myTarget.itemEnchantments.Count > 0)
                            {
                                _newAtt.uid = myTarget.itemEnchantments[myTarget.itemEnchantments.Count - 1].uid + 1;
                            }
                            else
                            {
                                _newAtt.uid = myTarget.itemEnchantments.Count;
                            }
                            _newAtt.fold = true;
                            _newAtt.attributes.Clear();
                            myTarget.itemEnchantments.Add(_newAtt);
                            _valueChanged = true;
                        }
                        GUI.backgroundColor = EditorUtils._black;
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Expand All", GUILayout.Width(70)))
                        {
                            for (int i = 0; i < myTarget.itemEnchantments.Count; i++) myTarget.itemEnchantments[i].fold = true;
                        }

                        if (GUILayout.Button("Fold All", GUILayout.Width(70)))
                        {
                            for (int i = 0; i < myTarget.itemEnchantments.Count; i++) myTarget.itemEnchantments[i].fold = false;
                        }
                        EditorUtils.ResetColor();
                        EditorUtils.End(20);

                        for (int i = 0; i < myTarget.itemEnchantments.Count; i++)
                        {
                            if (EditorUtils.TitleButton(new GUIContent(myTarget.itemEnchantments[i].name + " (UID:" + myTarget.itemEnchantments[i].uid.ToString() + ")", "Click to expand."),
                                ref myTarget.itemEnchantments[i].fold, 20, true, 20, true))
                            {
                                myTarget.itemEnchantments.RemoveAt(i);
                                EditorGUI.FocusTextInControl(null);
                                ApplyAndSave(myTarget, EditorAsset);
                                return;
                            }

                            if (myTarget.itemEnchantments[i].fold)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(45);
                                GUILayout.BeginVertical(EditorUtils._groupStyle);
                                EditorUtils.TextField(ref myTarget.itemEnchantments[i].name, new GUIContent("Display Name:", "The name will be displayed in the floating panel of item detailed information."), 10, 130);
                                EditorUtils.TextField(ref myTarget.itemEnchantments[i].prefixes, new GUIContent("Prefixes:", "The prefixes string will be displayed in the front of the item name."), 10, 130);
                                EditorUtils.IntSlider(ref myTarget.itemEnchantments[i].prefixesPriority, 0, 100, new GUIContent("Prefixes Priority:", "Only one prefixes string with highest priority will be displayed if an item has multiple enchantments."), " ", 10, 130);
                                EditorUtils.TextField(ref myTarget.itemEnchantments[i].suffixes, new GUIContent("Suffixes:", "The suffixes string will be displayed in the end of the item name."), 10, 130);
                                EditorUtils.IntSlider(ref myTarget.itemEnchantments[i].suffixesPriority, 0, 100, new GUIContent("Suffixes Priority:", "Only one suffixes string with highest priority will be displayed if an item has multiple enchantments."), " ", 10, 130);

                                if (EditorUtils.TitleBox(new GUIContent("Attributes: (" + myTarget.itemEnchantments[i].attributes.Count + ")", "Items with this enchantment will add these attributes to their base attributes."),
                                    EditorUtils._titleColor, 10, true, "+", 30))
                                {
                                    myTarget.itemEnchantments[i].attributes.Add(new AttributeData(GameManager.AttributeData.AttributeList[0].uid, 0F, ""));
                                    _valueChanged = true;
                                }

                                for (int u = 0; u < myTarget.itemEnchantments[i].attributes.Count; u++)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(20);
                                    int _sel = 0;
                                    for (int x = 0; x < GameManager.AttributeData.AttributeList.Count; x++)
                                    {
                                        if (GameManager.AttributeData.AttributeList[x].uid == myTarget.itemEnchantments[i].attributes[u].uid)
                                        {
                                            _sel = x;
                                            break;
                                        }
                                    }
                                    GUI.backgroundColor = EditorUtils._titleColor;
                                    GUILayout.Label(">", GUILayout.Width(15));
                                    EditorGUI.BeginChangeCheck();
                                    _sel = EditorGUILayout.Popup(new GUIContent("", "Click to select the attribute type."), _sel, GameManager.AttributeData.AttributesNames, GUILayout.Width(120));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        string _stringvalue = myTarget.itemEnchantments[i].attributes[u].stringValue;
                                        float _floatvalue = myTarget.itemEnchantments[i].attributes[u].floatValue;
                                        myTarget.itemEnchantments[i].attributes[u].uid = GameManager.AttributeData.AttributeList[_sel].uid;
                                        myTarget.itemEnchantments[i].attributes[u].stringValue = _stringvalue;
                                        myTarget.itemEnchantments[i].attributes[u].floatValue = _floatvalue;
                                    }

                                    GUILayout.Label(" :", GUILayout.Width(15));
                                    GUI.backgroundColor = EditorUtils._black;
                                    myTarget.itemEnchantments[i].attributes[u].floatValue = EditorGUILayout.FloatField(myTarget.itemEnchantments[i].attributes[u].floatValue, GUILayout.Width(60));
                                    GUI.backgroundColor = EditorUtils._black;
                                    GUI.color = Color.white;
                                    if (GUILayout.Button(new GUIContent("X", "Delete this attribute."), GUILayout.Width(25)))
                                    {
                                        myTarget.itemEnchantments[i].attributes.RemoveAt(u);
                                        ApplyAndSave(myTarget, EditorAsset);
                                        return;
                                    }
                                    EditorUtils.ResetColor();

                                    GUILayout.EndHorizontal();
                                }
                                GUILayout.EndVertical();
                                GUILayout.Space(25);
                                GUILayout.EndHorizontal();
                                EditorGUILayout.Separator();
                            }

                        }
                    }
                }
                EditorGUILayout.Separator();
            }



            ///Enhancement
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._upgradeExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._upgradeExpand ? "-" : "+") + " Enhancement Settings",
                    "Player can upgrade an item with the enhancement system. The enhancing will cost one type of currency and two specific items. Each upgrade level will make the attributes of this items increased by the amount set by \"Upgrade Increment\" property in attributes setting"), EditorUtils._titleButtonStyle))
            {
                myTarget._upgradeExpand = !myTarget._upgradeExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._upgradeExpand)
            {
                if (myTarget.ItemCount <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item.", 20);
                }
                else if (myTarget.itemTypes.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item category.", 20);
                }
                else
                {
                    EditorUtils.Toggle(ref myTarget.EnableEnhancing, new GUIContent("Enable Enhancing", " If you plan to not include Enhancing system in your game, uncheck this to disable this feature."), 20, 150);
                    EditorUtils.HelpInfo("Player can upgrade an item with the ENHANCEMENT system. The enhancing will cost one type of currency and two specific items. Each upgrade level will make the attributes of this items increased by the amount set by Upgrade Increment property in attributes setting.", 20);
                    if (myTarget.EnableEnhancing)
                    {
                        myTarget.EnhancingCategoryID = Mathf.Clamp(myTarget.EnhancingCategoryID, 0, myTarget.itemTypes.Count - 1);
                        GUI.backgroundColor = EditorUtils._titleColor;
                        EditorUtils.PopUp(ref myTarget.EnhancingCategoryID, _typeOptions,
                            new GUIContent("Enhancing Category:", "Only items belong to this category can be enhanced."), 20, 160, 200);
                        EditorUtils.IntSlider(ref myTarget.MaxiumEnhancingLevel, 1, 30, new GUIContent("Maximum Enhancing Level:", "The maximum level an item can be enhanced."), " ", 20, 160);
                        EditorUtils.Curve(ref myTarget.EnhancingSuccessCurve, new GUIContent("Enhancing Success Curve:", "Success rate in percentage when player enhance an item. The rate will change by the curve from level.0~ maximum enhancing Level."), 20, 180);
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        myTarget.DestroyEquipmentWhenFail = GUILayout.Toggle(myTarget.DestroyEquipmentWhenFail, new GUIContent("Destroy item when fail ( Level>", "When check this option, the item will be destroyed when fail by upgrading to level x+ "), GUILayout.Width(200));
                        myTarget.DestroyEquipmentWhenFailLevel = EditorGUILayout.IntSlider(myTarget.DestroyEquipmentWhenFailLevel, 0, myTarget.MaxiumEnhancingLevel, GUILayout.Width(150));
                        GUILayout.Label(")", GUILayout.Width(10));
                        GUILayout.Space(20);
                        GUILayout.EndHorizontal();
                        EditorUtils.FloatSlider(ref myTarget.EnhancingTime, 0.2F, 3F, new GUIContent("Enhancing Time:", "When enhancing, there will be a progress bar move from 1-100%, when it¡¯s done, the item will be enhanced, this is how long the process will take."), "sec", 20, 160);
                        myTarget.EnhancingCurrencyType = Mathf.Clamp(myTarget.EnhancingCurrencyType, 0, myTarget.currencies.Count - 1);
                        GUI.backgroundColor = EditorUtils._titleColor;
                        EditorUtils.PopUpIntField(ref myTarget.EnhancingCurrencyType, ref myTarget.EnhancingCurrencyNeed, _currencyOption,
                           new GUIContent("Required Currency:", "Set the currency type and cost when player enhance an item."),
                           20, EditorUtils._black, EditorUtils._titleColor, 160, 100, 80);

                        EditorUtils.TitleBox(new GUIContent("Required Item as Material:", "Select the material item and number of cost when player enhance an item."), EditorUtils._titleColor, 20);
                        if (myTarget.EnhancingMaterials.Length < 2) myTarget.EnhancingMaterials = new Vector2[2] { Vector2.one, Vector2.one };
                        for (int i = 0; i < myTarget.EnhancingMaterials.Length; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(40);
                            myTarget.EnhancingMaterials[i].x = Mathf.Max(myTarget.EnhancingMaterials[i].x, 0);
                            int _enhancingMatId = Mathf.FloorToInt(myTarget.EnhancingMaterials[i].x);
                            int _enhancingMat = myTarget.IndexOfItems(_enhancingMatId);
                            ItemIcon(EditorAsset, _enhancingMatId);
                            GUI.backgroundColor = EditorUtils._buttonColor;
                            EditorGUI.BeginChangeCheck();
                            _enhancingMat = EditorGUILayout.Popup("", _enhancingMat, _itemOption, GUILayout.Width(100));
                            if (EditorGUI.EndChangeCheck()) myTarget.EnhancingMaterials[i] = new Vector2(myTarget.GetItemByIndex(_enhancingMat).id, myTarget.EnhancingMaterials[i].y);
                            GUI.backgroundColor = EditorUtils._black;
                            GUILayout.Label("x", GUILayout.Width(10));
                            myTarget.EnhancingMaterials[i].y = Mathf.Clamp(EditorGUILayout.IntField(Mathf.FloorToInt(myTarget.EnhancingMaterials[i].y), GUILayout.Width(65)), 1, 99);
                            EditorUtils.ResetColor();
                            GUILayout.EndHorizontal();
                        }
                        EditorGUILayout.Separator();
                        EditorUtils.Toggle(ref myTarget.EnableEnhancingGlow, new GUIContent("Enable Glowing Effect", "check this to make the icon glow by the enhancing Level."), 20, 160);
                        if (myTarget.EnableEnhancingGlow)
                        {
                            EditorUtils.Curve(ref myTarget.EnhancingGlowCurve, new GUIContent("Glow Intensity Curve:", "The icon will glow by the curve from level.0~ maximum enhancing Level."), 20, 160, 180);
                        }
                    }
                }
                EditorGUILayout.Separator();
            }


            ///Socketing
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._socketingExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._socketingExpand ? "-" : "+") + " Socketing Settings",
                    "Players can socketing items with specified category and tag to other items to grant them additional attributes."), EditorUtils._titleButtonStyle))
            {
                myTarget._socketingExpand = !myTarget._socketingExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._socketingExpand)
            {
                if (GameManager.AttributeData == null || GameManager.AttributeData.AttributeList.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item attribute.", 20);
                }
                else if (myTarget.itemTypes.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item category.", 20);
                }
                else if (myTarget.ItemCount <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item.", 20);
                }
                else
                {
                    EditorUtils.Toggle(ref myTarget.EnableSocketing, new GUIContent("Enable Socketing", "Toggle this option to enable or disable the socketing system in your game. If unchecked, the feature will not be available."), 20, 150);
                    EditorUtils.HelpInfo("The SOCKETING system allows players to insert (socket) items of specified categories and tags into another item to boost its attributes. Follow the configuration options below to tailor the system to your game's needs.", 20);
                    if (myTarget.EnableSocketing)
                    {
                        myTarget.SocketedCategoryFilter = Mathf.Clamp(myTarget.SocketedCategoryFilter, 0, myTarget.itemTypes.Count - 1);
                        EditorUtils.PopUp(ref myTarget.SocketedCategoryFilter, _typeOptions,
                            new GUIContent("Socketed Items (Receiver) Category:", "Defines the category of items that can have socketing slots (e.g., weapons, armor). Only items in this category can receive socketing items."),
                            20, 220, 150);
                        myTarget.SocketingCategoryFilter = Mathf.Clamp(myTarget.SocketingCategoryFilter, 0, myTarget.itemTypes.Count - 1);
                        EditorUtils.PopUp(ref myTarget.SocketingCategoryFilter, _typeOptions,
                            new GUIContent("Socketing Items (Plug-in) Category:", "Specifies the category of items that can be inserted (socketed) into socketed items (e.g., gems, runes)."),
                            20, 220, 150);
                        EditorUtils.TextField(ref myTarget.SocketingTagFilter, new GUIContent("Socketing Items (Plug-in) Tag:", " Filters socketing items further by requiring them to have a specific tag (e.g., \"FireGem\" or \"MagicRune\"). Leave blank if no tag filtering is needed."), 20, 220);
                        EditorGUILayout.Separator();
                        EditorUtils.Toggle(ref myTarget.RandomSocketingSlotsNummber, new GUIContent("Random Skocketing Slots Number for New Item.", "Enable this to randomize the number of socketing slots when a new item is created using new Item(int _uid)."), 20, 300);
                        if (myTarget.RandomSocketingSlotsNummber)
                        {
                            EditorUtils.IntSlider(ref myTarget.MinimalSocketingSlotsNumber, 0, myTarget.MaxmiumSocketingSlotsNumber,
                               new GUIContent("Minimal Socketing Slots Count:", "Sets the minimum count of socketing slots for newly created items when randomization is enabled."),
                                " ", 40, 210);
                            EditorUtils.IntSlider(ref myTarget.MaxmiumSocketingSlotsNumber, myTarget.MaxmiumSocketingSlotsNumber, 5,
                              new GUIContent("Maximum Socketing Slots Count:", " Sets the maximum count of socketing slots for newly created items when randomization is enabled."),
                               " ", 40, 210);

                        }
                        EditorGUILayout.Separator();
                        EditorUtils.Toggle(ref myTarget.LockSocketingSlotsByDefault, new GUIContent("Lock Socketing Slots Number By Default.", "Enable this to lock socketing slots by default when a new item is created."), 20, 300);
                        if (myTarget.LockSocketingSlotsByDefault)
                        {
                            EditorUtils.IntSlider(ref myTarget.RandomChanceToLockSocketingSlots, 0, 100,
                              new GUIContent("Random Chance to Lock Slots:", "Specify the probability (as a percentage) of socketing slots being locked by default for newly created items."),
                               "%", 40, 210);
                            myTarget.UnlockSocketingSlotsCurrency = Mathf.Clamp(myTarget.UnlockSocketingSlotsCurrency, 0, myTarget.currencies.Count - 1);
                            EditorUtils.PopUpIntField(ref myTarget.UnlockSocketingSlotsCurrency, ref myTarget.UnlockSocketingSlotsPrice, _currencyOption,
                                new GUIContent("Unlock Socketing Slot Cost:", "The cost for the player to unlock a locked socketing slot. This cost can represent in-game currency or another resource.")
                                , 40, EditorUtils._black, EditorUtils._titleColor, 210, 120, 50);
                        }
                        EditorGUILayout.Separator();

                        EditorUtils.Toggle(ref myTarget.EnableRemoveSocketing, new GUIContent("Allow Remove Socketing Items.", "Enable this to allow players to remove socketed items from their slots."), 20, 300);
                        if (myTarget.EnableRemoveSocketing)
                        {
                            myTarget.RemoveSocketingCurrency = Mathf.Clamp(myTarget.RemoveSocketingCurrency, 0, myTarget.currencies.Count - 1);
                            EditorUtils.PopUpIntField(ref myTarget.RemoveSocketingCurrency, ref myTarget.RemoveSocketingPrice, _currencyOption,
                                new GUIContent("Remove Socketing Item Cost:", "The cost for the player to remove a socketed item.")
                                , 40, EditorUtils._black, EditorUtils._titleColor, 210, 120, 50);
                            EditorUtils.Toggle(ref myTarget.DestroySocketItemWhenRemove,
                                new GUIContent("Destroy Socketed Item When Remove", "Enable this option to destroy the socketed item when it is removed. Disable it if you want the item to return to the player's inventory instead."),
                                40, 280);
                        }


                    }
                }
                EditorGUILayout.Separator();
            }


            ///LootPack
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._lootPackExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._lootPackExpand ? "-" : "+") + " Loot Pack Settings",
                    "Manage loot packs, you can assign loot packs to Entity in EntityManager, then call _entity.DropLootPack(), or you can simplely call GameManage.DropLootPack(Vector3 _pos, string _uid) to drop a loot pack. "), EditorUtils._titleButtonStyle))
            {
                myTarget._lootPackExpand = !myTarget._lootPackExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._lootPackExpand)
            {
                EditorUtils.Document("master-inventory-engine/item-class/loot-pack");
                EditorUtils.HelpInfo("You can assign loot packs to <color=#22AAFF>Entity</color> in <color=#22AAFF>EntityManager</color>, then call <color=#22FFAA>_entity.DropLootPack()</color>, or you can simply call <color=#22FFAA>GameManage.DropLootPack(Vector3 _pos, string _uid)</color> to drop a loot pack.", 20);

                bool _ready = true;
                if (myTarget.ItemCount <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item.", 20);
                    _ready = false;
                }

                if (_ready)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(25);
                    GUI.backgroundColor = EditorUtils._titleColor;
                    if (GUILayout.Button(new GUIContent("Add [Loot Pack]", "Create a new Loot Pack."), GUILayout.Width(100)))
                    {
                        LootPackData _newLoot = new LootPackData();
                        if (myTarget.lootPacks.Count < 1)
                        {
                            _newLoot.uid = "LootPack1";
                            _newLoot.ItemPool = new List<int>();
                            _newLoot.CurrencyMin = new List<int>();
                            _newLoot.CurrencyMax = new List<int>();
                            _newLoot.MaxiumItemCount = 5;
                            _newLoot.MaxiumCountEachItem = 3;
                            _newLoot.DropChanceMultiplier = 1F;
                            _newLoot.RandomLevel = false;
                            _newLoot.DestoryWhenPlayerCloseLootWindow = true;
                            _newLoot.RandomEnchantment = false;
                            _newLoot.EnchantmentPool = new List<int>();
                            _newLoot.MaxiumEnhancingLevel = 10;
                        }
                        else
                        {
                            _newLoot = myTarget.lootPacks[myTarget.lootPacks.Count - 1].Copy();
                        }
                        string _base = "LootPack";
                        int _maxId = 0;
                        foreach (var obj in myTarget.lootPacks)
                        {
                            if (obj.uid.Contains(_base))
                            {
                                int _testId = -1;
                                if (int.TryParse(obj.uid.Replace(_base, ""), out _testId)) _maxId = Mathf.Max(_maxId, _testId);
                            }
                        }
                        _newLoot.uid = _base + (_maxId + 1).ToString();
                        _newLoot.fold = true;


                        myTarget.lootPacks.Add(_newLoot);
                        _valueChanged = true;
                        ApplyAndSave(myTarget, EditorAsset);
                        EditorGUI.FocusTextInControl(null);
                    }
                    GUI.backgroundColor = EditorUtils._black;
                    if (GUILayout.Button("Fold All", GUILayout.Width(50)))
                    {
                        for (int i = 0; i < myTarget.lootPacks.Count; i++) myTarget.lootPacks[i].fold = false;
                    }
                    EditorUtils.ResetColor();
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }
                List<string> _uids = new List<string>();
#if MASTER_INVENTORY_ENGINE
                for (int i = 0; i < myTarget.lootPacks.Count; i++)
                {
                    bool _duplicateUid = _uids.Contains(myTarget.lootPacks[i].uid);
                    if (!_duplicateUid) _uids.Add(myTarget.lootPacks[i].uid);
                    GUILayout.BeginHorizontal();
                    GUI.backgroundColor = myTarget.lootPacks[i].fold ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
                    GUILayout.Label(myTarget.lootPacks[i].fold ? "[-]" : "[+]", GUILayout.Width(20));
                    if (_duplicateUid)
                    {
                        GUI.backgroundColor = Color.white;
                        GUILayout.Box(EditorUtils.GetTexture(EditorIcon.Warning), EditorUtils._iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                        GUI.backgroundColor = Color.red;
                    }

                    string _key = myTarget.lootPacks[i].uid;
                    GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                    GUILayout.Space(5);
                    if (GUILayout.Button(new GUIContent(myTarget.lootPacks[i].uid, "Click to expand"), EditorUtils._titleButtonStyle))
                    {
                        myTarget.lootPacks[i].fold = !myTarget.lootPacks[i].fold;
                        EditorGUI.FocusTextInControl(null);
                    }
                    EditorUtils.ResetColor();
                    if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy this loot pack."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                    {
                        ClipboardLootPack = myTarget.lootPacks[i].Copy();
                        EditorGUI.FocusTextInControl(null);
                    }
                    EditorUtils.ResetColor();
                    if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste data of the copied loot pack to replace this one."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                    {
                        if (ClipboardLootPack != null)
                        {
                            string _olduid = myTarget.lootPacks[i].uid;
                            myTarget.lootPacks[i] = ClipboardLootPack.Copy();
                            myTarget.lootPacks[i].uid = _olduid;
                            _valueChanged = true;
                            EditorGUI.FocusTextInControl(null);
                            ApplyAndSave(myTarget, EditorAsset);
                        }
                    }
                    EditorUtils.ResetColor();
                    GUI.color = i > 0 ? Color.white : Color.gray;
                    if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Up), "Move this loot pack up."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                    {
                        if (i > 0)
                        {
                            LootPackData _mLootPack = myTarget.lootPacks[i].Copy();
                            _mLootPack.fold = false;
                            myTarget.lootPacks.RemoveAt(i);
                            myTarget.lootPacks.Insert(i - 1, _mLootPack);
                            EditorGUI.FocusTextInControl(null);
                            ApplyAndSave(myTarget, EditorAsset);
                            return;
                        }
                    }
                    GUI.color = i < myTarget.lootPacks.Count - 1 ? Color.white : Color.gray;
                    if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Down), "Move this loot pack down."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                    {
                        if (i < myTarget.lootPacks.Count - 1)
                        {
                            LootPackData _mLootPack = myTarget.lootPacks[i].Copy();
                            _mLootPack.fold = false;
                            myTarget.lootPacks.RemoveAt(i);
                            myTarget.lootPacks.Insert(i + 1, _mLootPack);
                            EditorGUI.FocusTextInControl(null);
                            ApplyAndSave(myTarget, EditorAsset);
                            return;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;


                    GUI.backgroundColor = EditorUtils._black;
                    if (GUILayout.Button(new GUIContent("X", "Delete this loot pack."), GUILayout.Width(20)))
                    {
                        for (int w = 0; w < GameManager.EntityManagerData.EntityList.Count; w++)
                        {
                            if (GameManager.EntityManagerData.EntityList[w].GetModule<InventoryModule>().LootPacks.Contains(myTarget.lootPacks[i].uid))
                            {
                                GameManager.EntityManagerData.EntityList[w].GetModule<InventoryModule>().LootPacks.RemoveAt(w);
                            }
                        }
                        myTarget.lootPacks.RemoveAt(i);
                        GameManager.EntityManagerData.GenerateUniqueHash();
                        EditorUtility.SetDirty(GameManager.EntityManagerData);
                        EditorGUI.FocusTextInControl(null);
                        ApplyAndSave(myTarget, EditorAsset);
                        AssetDatabase.SaveAssets();
                        return;
                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();

                    if (myTarget.lootPacks[i].fold)
                    {
                        EditorUtils.TextField(ref myTarget.lootPacks[i].uid, new GUIContent("UID:", "The unique string id this loot pack."), 50, 100, true);
                        myTarget.lootPacks[i].uid = myTarget.lootPacks[i].uid.Replace(" ", "");

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        GUILayout.Label(new GUIContent("VFX:", "The visual effect prefab for this loot pack."), GUILayout.Width(100));
                        myTarget.lootPacks[i].VFX = (GameObject)EditorGUILayout.ObjectField(myTarget.lootPacks[i].VFX, typeof(GameObject), false, GUILayout.Width(150));
                        GUILayout.Space(30);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        GUI.backgroundColor = myTarget.lootPacks[i].currencyExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
                        GUI.color = (myTarget.lootPacks[i].CurrencyMin.Count <= 0 || myTarget.lootPacks[i].CurrencyMax.Count <= 0) ? Color.red : Color.white;
                        if (GUILayout.Button(new GUIContent(" Currency", "The currencies this loot pack contains."), EditorUtils._titleButtonStyle))
                        {
                            myTarget.lootPacks[i].currencyExpand = !myTarget.lootPacks[i].currencyExpand;
                            EditorGUI.FocusTextInControl(null);
                        }
                        EditorUtils.ResetColor();
                        if (ItemObject.instance.currencies.Count <= 0)
                        {
                            GUILayout.Box(EditorUtils.GetTexture(EditorIcon.Warning), GUIStyle.none, GUILayout.Width(20));
                        }
                        else
                        {
                            bool _noMatch = false;
                            if (myTarget.lootPacks[i].CurrencyMin.Count < ItemObject.instance.currencies.Count)
                            {
                                myTarget.lootPacks[i].CurrencyMin.Add(0);
                                _noMatch = true;
                            }
                            else if (myTarget.lootPacks[i].CurrencyMin.Count > ItemObject.instance.currencies.Count)
                            {
                                myTarget.lootPacks[i].CurrencyMin.RemoveAt(0);
                                _noMatch = true;
                            }
                            if (myTarget.lootPacks[i].CurrencyMax.Count < ItemObject.instance.currencies.Count)
                            {
                                myTarget.lootPacks[i].CurrencyMax.Add(0);
                                _noMatch = true;
                            }
                            else if (myTarget.lootPacks[i].CurrencyMax.Count > ItemObject.instance.currencies.Count)
                            {
                                myTarget.lootPacks[i].CurrencyMax.RemoveAt(0);
                                _noMatch = true;
                            }
                            if (_noMatch) return;
                        }
                        GUILayout.Space(30);
                        GUILayout.EndHorizontal();

                        if (myTarget.lootPacks[i].currencyExpand)
                        {
                            if (ItemObject.instance.currencies.Count <= 0)
                            {
                                EditorUtils.Warnning("You must have at least one currency setup in Project Settings/SoftKitty/SubData - Items", 70);
                            }
                            else
                            {

                                //===Currency List

                                for (int u = 0; u < ItemObject.instance.currencies.Count; u++)
                                {
                                    if (myTarget.lootPacks[i].CurrencyMin.Count <= u) myTarget.lootPacks[i].CurrencyMin.Add(0);
                                    if (myTarget.lootPacks[i].CurrencyMax.Count <= u) myTarget.lootPacks[i].CurrencyMax.Add(0);
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    GUILayout.Label("(ID:" + u.ToString() + ")", GUILayout.Width(40), GUILayout.Height(20));
                                    if (ItemObject.instance.currencies[u].icon.texture != null)
                                    {
                                        GUILayout.Box(ItemObject.instance.currencies[u].icon.texture, GUILayout.Width(20), GUILayout.Height(20));
                                    }
                                    else
                                    {
                                        GUILayout.Box("-", GUILayout.Width(20), GUILayout.Height(20));
                                    }
                                    GUI.color = ItemObject.instance.currencies[u].color;
                                    GUILayout.Label(ItemObject.instance.currencies[u].name + " :", GUILayout.Width(100), GUILayout.Height(20));
                                    GUI.color = Color.white;

                                    GUI.backgroundColor = ItemObject.instance.currencies[u].color;
                                    myTarget.lootPacks[i].CurrencyMin[u] = EditorGUILayout.IntField(myTarget.lootPacks[i].CurrencyMin[u], GUILayout.Width(100), GUILayout.Height(20));
                                    GUILayout.Label("~", GUILayout.Width(15), GUILayout.Height(20));
                                    myTarget.lootPacks[i].CurrencyMax[u] = EditorGUILayout.IntField(myTarget.lootPacks[i].CurrencyMax[u], GUILayout.Width(100), GUILayout.Height(20));
                                    EditorUtils.ResetColor();
                                    GUILayout.Space(30);
                                    GUILayout.EndHorizontal();
                                }
                                //===Currency List
                            }
                        }

                        GUILayout.BeginHorizontal();
                        GUI.backgroundColor = myTarget.lootPacks[i].itemExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
                        GUI.color = (myTarget.ItemCount <= 0) ? Color.red : Color.white;
                        GUILayout.Space(50);
                        if (GUILayout.Button(new GUIContent(" Drop Items Pool (" + myTarget.lootPacks[i].ItemPool.Count.ToString() + ")", "When this loot pack drops, it will random pick items from this pool."), EditorUtils._titleButtonStyle))
                        {
                            myTarget.lootPacks[i].itemExpand = !myTarget.lootPacks[i].itemExpand;
                            EditorGUI.FocusTextInControl(null);
                        }
                        EditorUtils.ResetColor();
                        if (myTarget.ItemCount <= 0)
                        {
                            GUILayout.Box(EditorUtils.GetTexture(EditorIcon.Warning), GUIStyle.none, GUILayout.Width(20));
                        }
                        GUILayout.Space(30);
                        GUILayout.EndHorizontal();


                        if (myTarget.lootPacks[i].itemExpand)
                        {
                            if (myTarget.ItemCount <= 0)
                            {
                                EditorUtils.Warnning("You must have at least one item setup in Project Settings/SoftKitty/SubData - Items", 70);
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(70);
                                GUI.backgroundColor = EditorUtils._titleColor;
                                if (GUILayout.Button(new GUIContent("Add new [Item]", "Add an new item to the pool."), GUILayout.Width(150)))
                                {
                                    myTarget.lootPacks[i].ItemPool.Add(0);
                                    _valueChanged = true;
                                }
                                GUILayout.Space(30);
                                GUILayout.EndHorizontal();
                                for (int u = 0; u < myTarget.lootPacks[i].ItemPool.Count; u++)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    GUILayout.Box(GetIcon(EditorAsset, myTarget.lootPacks[i].ItemPool[u]), GUILayout.Width(20), GUILayout.Height(20));
                                    int _itemId = myTarget.lootPacks[i].ItemPool[u];
                                    int _index = myTarget.IndexOfItems(_itemId);
                                    if (_index >= 0)
                                    {
                                        EditorGUI.BeginChangeCheck();
                                        GUI.backgroundColor = myTarget.itemTypes[myTarget.GetItemByIndex(_index).type].color;
                                        _index = EditorGUILayout.Popup("", _index, _itemOption, GUILayout.Width(180));
                                        if (EditorGUI.EndChangeCheck()) myTarget.lootPacks[i].ItemPool[u] = myTarget.items[_index].id;

                                        GUI.backgroundColor = EditorUtils._black;
                                        if (GUILayout.Button("X", GUILayout.Width(20)))
                                        {
                                            myTarget.lootPacks[i].ItemPool.RemoveAt(u);
                                        }
                                    }
                                    else
                                    {
                                        myTarget.lootPacks[i].ItemPool.RemoveAt(u);
                                    }
                                    EditorUtils.ResetColor();
                                    GUILayout.Space(30);
                                    GUILayout.EndHorizontal();
                                }
                            }

                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        GUILayout.Label(new GUIContent("Maxmium Item Count:", "When random pick items from the pool, this number will be the maxmium item count it could be"), GUILayout.Width(180));
                        myTarget.lootPacks[i].MaxiumItemCount = EditorGUILayout.IntSlider(myTarget.lootPacks[i].MaxiumItemCount, Mathf.Min(myTarget.lootPacks[i].ItemPool.Count, 1), myTarget.lootPacks[i].ItemPool.Count);
                        GUILayout.Space(30);
                        GUILayout.EndHorizontal();

                        if (myTarget.lootPacks[i].ItemPool.Count <= 0)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Box(EditorUtils.GetTexture(EditorIcon.Warning), GUIStyle.none, GUILayout.Width(20));
                            GUI.color = Color.yellow;
                            GUILayout.Label("<color=#22AAFF>Maxmium Item Count</color> can not be larger than the <color=#22AAFF>Drop Items Pool</color> count, please add at least one item to the <color=#22AAFF>pool</color>.");
                            GUI.color = Color.white;
                            GUILayout.Space(30);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        GUILayout.Label(new GUIContent("Maxmium Count Each Item:", "The count of each item stack will be random from 1 to this number."), GUILayout.Width(180));
                        myTarget.lootPacks[i].MaxiumCountEachItem = EditorGUILayout.IntSlider(myTarget.lootPacks[i].MaxiumCountEachItem, 1, 99);
                        GUILayout.Space(30);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        GUILayout.Label(new GUIContent("Drop Chance Multiplier:", "The drop chance of each item will be the drop chance in the [ItemObject.instance] setting multiply this number."), GUILayout.Width(180));
                        myTarget.lootPacks[i].DropChanceMultiplier = EditorGUILayout.Slider(myTarget.lootPacks[i].DropChanceMultiplier, 0.5F, 5F);
                        GUILayout.Label("x", GUILayout.Width(15));
                        GUILayout.Space(30);
                        GUILayout.EndHorizontal();

                        if (ItemObject.instance.EnableEnhancing)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Label(new GUIContent("Random Enhancing Level:", "Will the items have random Enhancing Level?"), GUILayout.Width(180));
                            myTarget.lootPacks[i].RandomLevel = EditorGUILayout.Toggle(myTarget.lootPacks[i].RandomLevel);
                            GUILayout.Space(30);
                            GUILayout.EndHorizontal();

                            if (myTarget.lootPacks[i].RandomLevel)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(50);
                                GUILayout.Label(new GUIContent("Maximum Enhancing Level:", "Maximum Enhancing Level the random result can be."), GUILayout.Width(180));
                                myTarget.lootPacks[i].MaxiumEnhancingLevel = EditorGUILayout.IntSlider(myTarget.lootPacks[i].MaxiumEnhancingLevel, 1, ItemObject.instance.MaxiumEnhancingLevel);
                                GUILayout.Space(30);
                                GUILayout.EndHorizontal();
                            }
                        }

                        if (ItemObject.instance.EnableEnchanting)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(50);
                            GUILayout.Label(new GUIContent("Random Enchantments: (" + (myTarget.lootPacks[i].EnchantmentPool.Count <= 0 ? "All" : myTarget.lootPacks[i].EnchantmentPool.Count.ToString()) + ")",
                                "Will the items have random Enchantments?"), GUILayout.Width(180));
                            myTarget.lootPacks[i].RandomEnchantment = EditorGUILayout.Toggle(myTarget.lootPacks[i].RandomEnchantment, GUILayout.Width(50));
                            if (myTarget.lootPacks[i].RandomEnchantment && ItemObject.instance.itemEnchantments.Count > 0)
                            {
                                GUI.backgroundColor = EditorUtils._titleColor;
                                if (GUILayout.Button("Add", GUILayout.Width(50)))
                                {
                                    myTarget.lootPacks[i].EnchantmentPool.Add(0);
                                    _valueChanged = true;
                                    EditorGUI.FocusTextInControl(null);
                                }
                                GUI.backgroundColor = EditorUtils._titleColor;
                                if (GUILayout.Button("Add All", GUILayout.Width(80)))
                                {
                                    myTarget.lootPacks[i].EnchantmentPool.Clear();
                                    for (int u = 0; u < ItemObject.instance.itemEnchantments.Count; u++) myTarget.lootPacks[i].EnchantmentPool.Add(u);
                                    _valueChanged = true;
                                    EditorGUI.FocusTextInControl(null);
                                }
                                GUI.backgroundColor = EditorUtils._black;
                                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                                {
                                    myTarget.lootPacks[i].EnchantmentPool.Clear();
                                    _valueChanged = true;
                                    EditorGUI.FocusTextInControl(null);
                                }
                                GUI.backgroundColor = Color.white;
                            }
                            GUILayout.Space(30);
                            GUILayout.EndHorizontal();

                            if (myTarget.lootPacks[i].RandomEnchantment)
                            {
                                string[] _enchantmentOptions = new string[ItemObject.instance.itemEnchantments.Count];
                                for (int u = 0; u < _enchantmentOptions.Length; u++) _enchantmentOptions[u] = ItemObject.instance.itemEnchantments[u].name;
                                for (int u = 0; u < myTarget.lootPacks[i].EnchantmentPool.Count; u++)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(70);
                                    GUI.backgroundColor = EditorUtils._black;
                                    myTarget.lootPacks[i].EnchantmentPool[u] = EditorGUILayout.Popup(myTarget.lootPacks[i].EnchantmentPool[u], _enchantmentOptions, GUILayout.Width(120));
                                    GUI.backgroundColor = EditorUtils._black;
                                    if (GUILayout.Button("X", GUILayout.Width(18)))
                                    {
                                        myTarget.lootPacks[i].EnchantmentPool.RemoveAt(u);
                                        _valueChanged = true;
                                        EditorGUI.FocusTextInControl(null);
                                    }
                                    GUI.backgroundColor = Color.white;
                                    GUILayout.Space(30);
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }


                        GUILayout.BeginHorizontal();
                        GUILayout.Space(50);
                        GUILayout.Label(new GUIContent("Destroy when player close loot window:", "Will the loot pack be destroyed when player close the loot window?"), GUILayout.Width(250));
                        myTarget.lootPacks[i].DestoryWhenPlayerCloseLootWindow = EditorGUILayout.Toggle(myTarget.lootPacks[i].DestoryWhenPlayerCloseLootWindow);
                        GUILayout.Space(30);
                        GUILayout.EndHorizontal();
                    }
                }
#endif
            }

            ///Items
            GUILayout.BeginHorizontal();
            GUILayout.Space(6);
            GUI.backgroundColor = myTarget._itemExpand ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
            if (GUILayout.Button(EditorUtils.TextHint((myTarget._itemExpand ? "-" : "+") + " Item Settings (" + myTarget.ItemCount.ToString() + ")",
                    "Setup items database."), EditorUtils._titleButtonStyle))
            {
                myTarget._itemExpand = !myTarget._itemExpand;
                EditorGUI.FocusTextInControl(null);
            }
            EditorUtils.ResetColor();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (myTarget._itemExpand)
            {
                EditorUtils.Document("master-inventory-engine/item-class/item");
                EditorGUILayout.Separator();

                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.Label("Data Mode:", GUILayout.Width(155));
                GUI.backgroundColor = EditorUtils._black;
                var _oriMode = myTarget.DataMode;
                EditorGUI.BeginChangeCheck();
                myTarget.DataMode = (ItemDataMode)EditorGUILayout.EnumPopup(myTarget.DataMode, GUILayout.Width(150));
                if (EditorGUI.EndChangeCheck())
                {
                    bool _accept = EditorUtility.DisplayDialog("Switching Data Mode", "Please make sure to clone data before switching to another mode, otherwise you will lost all data in current mode and it will break the connection for your inventory and other item related settings.", "I'm Aware", "Cancel");
                    if (_accept)
                    {
                        if (_oriMode == ItemDataMode.Unified)
                        {
                            myTarget.items.Clear();
                        }
                        else
                        {
                            myTarget.itemScriptableObjects.Clear();
                        }
                        myTarget.IdManager.idToKey.Clear();
                        ApplyAndSave(myTarget, EditorAsset);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        myTarget.DataMode = _oriMode;
                    }
                }
                if (myTarget.DataMode == ItemDataMode.Unified)
                {
                    if (GUILayout.Button(new GUIContent("Clone to 'ScriptableObject' mode", "Clone the 'Unified' mode data to 'ScriptableObject' mode data")))
                    {
                        if (!AssetDatabase.IsValidFolder(EditorUtils.DataPath + "Items/"))
                        {
                            AssetDatabase.CreateFolder(EditorUtils.DataPath.Replace("Data/", "Data"), "Items");
                            AssetDatabase.Refresh();
                        }
                        myTarget.itemScriptableObjects.Clear();
                        for (int i = 0; i < myTarget.items.Count; i++)
                        {
                            myTarget.items[i].fold = false;
                            ItemScriptableObject _newObj = (ItemScriptableObject)ScriptableObject.CreateInstance(typeof(ItemScriptableObject));
                            _newObj.mItem = myTarget.items[i].Copy();
                            _newObj.mItem.fold = false;
                            if (!AssetDatabase.IsValidFolder(EditorUtils.DataPath + "Items/"))
                            {
                                AssetDatabase.CreateFolder(EditorUtils.DataPath.Replace("Data/", "Data"), "Items");
                                AssetDatabase.Refresh();
                            }
                            string _basepath = EditorUtils.DataPath + "Items/" + _newObj.mItem.uid;
                            string _path = _basepath + ".asset";
                            AssetDatabase.CreateAsset(_newObj, _path);
                            AssetDatabase.Refresh();
                            EditorUtility.SetDirty(_newObj);
                            myTarget.itemScriptableObjects.Add(_newObj);
                        }
                        bool _clear = EditorUtility.DisplayDialog("Success", "All items copied to 'ScriptableObject' mode, do you want to clear the data for 'Unified' mode?", "Yes", "No");
                        if (_clear) myTarget.items.Clear();
                        myTarget.DataMode = ItemDataMode.ScriptableObject;
                        myTarget.IdManager.idToKey.Clear();
                        ApplyAndSave(myTarget, EditorAsset);
                        AssetDatabase.SaveAssets();
                        Object folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(EditorUtils.DataPath + "Items");
                        if (folderAsset != null)
                        {
                            EditorGUIUtility.PingObject(folderAsset);
                            Selection.activeObject = folderAsset;
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button(new GUIContent("Clone to 'Unified' mode", "Clone the 'ScriptableObject' mode data to 'Unified' mode data, please note that all item ScriptableObjects will be deleted after clone.")))
                    {
                        myTarget.items.Clear();
                        for (int i = 0; i < myTarget.itemScriptableObjects.Count; i++)
                        {
                            myTarget.itemScriptableObjects[i].mItem.fold = false;
                            Item _newItem = myTarget.itemScriptableObjects[i].mItem.Copy();
                            _newItem.fold = false;
                            myTarget.items.Add(_newItem);
                        }
                        bool _clear = EditorUtility.DisplayDialog("Success", "All items copied to 'Unified' mode, do you want to clear the data and delete all item assets for 'ScriptableObject' mode?", "Yes", "No");
                        if (_clear)
                        {
                            for (int i = 0; i < myTarget.itemScriptableObjects.Count; i++)
                            {
                                string _itemPath = AssetDatabase.GetAssetPath(myTarget.itemScriptableObjects[i]);
                                AssetDatabase.DeleteAsset(_itemPath);
                            }
                            myTarget.itemScriptableObjects.Clear();
                        }
                        myTarget.DataMode = ItemDataMode.Unified;
                        myTarget.IdManager.idToKey.Clear();
                        ApplyAndSave(myTarget, EditorAsset);
                        AssetDatabase.SaveAssets();
                    }
                }
                EditorUtils.ResetColor();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                EditorUtils.HelpInfo(
                    "[<color=#3399FF>Unified</color>] - Managing all items data all together, clean structure.\n" +
                    "[<color=#3399FF>ScriptableObject</color>] - Each item will store in individual ScriptableObject, good for version control.\n" +
                    "Please do not mix using both mode.", 30);

                EditorGUILayout.Separator();

                bool _ready = true;
                if (myTarget.itemTypes.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item type.", 20);
                    _ready = false;
                }
                if (myTarget.itemQuality.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one item quality.", 20);
                    _ready = false;
                }
                if (GameManager.AttributeData == null || GameManager.AttributeData.AttributeList.Count <= 0)
                {
                    EditorUtils.Warnning("You must have at least one attribute.", 20);
                    _ready = false;
                }


                if (_ready)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("Search:", EditorUtils._boxLeftStyle, GUILayout.Width(50));
                    GUI.backgroundColor = EditorUtils._titleColor;
                    myTarget.SearchType = EditorGUILayout.Popup(myTarget.SearchType, SearchOptions, GUILayout.Width(100));
                    GUI.backgroundColor = string.IsNullOrEmpty(myTarget.SearchText) ? EditorUtils._gray : new Color(1F, 1F, 0F);
                    if (!string.IsNullOrEmpty(myTarget.SearchText)) GUI.color = new Color(1F, 0.5F, 0F);
                    myTarget.SearchText = EditorGUILayout.TextField(myTarget.SearchText);
                    EditorUtils.ResetColor();
                    GUI.backgroundColor = EditorUtils._black;
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        myTarget.SearchText = "";
                        EditorGUI.FocusTextInControl(null);
                        ApplyAndSave(myTarget, EditorAsset);
                    }
                    EditorUtils.ResetColor();
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Separator();

                    ////Item List

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(25);
                    GUI.backgroundColor = EditorUtils._titleColor;
                    if (GUILayout.Button(new GUIContent("Add new [Item]", "Create a new item."), GUILayout.Width(100)))
                    {
                        Item _newItem = new Item();
                        if ((myTarget.ItemCount < 1))
                        {
                            if (myTarget.DataMode == ItemDataMode.Unified)
                            {
                                _newItem.name = "New Item" + myTarget.items.Count.ToString();
                            }
                            else
                            {
                                _newItem.name = "New Item" + myTarget.itemScriptableObjects.Count.ToString();
                            }
                            _newItem.description = "No description yet.";
                            _newItem.type = 0;
                            _newItem.quality = 0;
                            _newItem.icon = null;
                            _newItem.maxiumStack = 99;
                            _newItem.price = 0;
                            _newItem.upgradeLevel = 0;
                            _newItem.useable = false;
                            _newItem.consumable = false;
                            _newItem.tradeable = true;
                            _newItem.deletable = true;
                            _newItem.attributes.Clear();
                            _newItem.enchantments.Clear();
                            _newItem.actions.Clear();
                            _newItem.craftMaterials.Clear();
                            _newItem.tags.Clear();
                            _newItem.customData.Clear();
                        }
                        else
                        {
                            if (myTarget.DataMode == ItemDataMode.Unified)
                            {
                                _newItem = myTarget.items[myTarget.items.Count - 1].Copy();
                                _newItem.name = "New Item" + myTarget.items.Count.ToString();
                            }
                            else
                            {
                                _newItem = myTarget.itemScriptableObjects[myTarget.itemScriptableObjects.Count - 1].mItem.Copy();
                                _newItem.name = "New Item" + myTarget.itemScriptableObjects.Count.ToString();
                            }
                        }
                        string _base = "Item";
                        int _maxId = 0;
                        if (myTarget.DataMode == ItemDataMode.Unified)
                        {
                            foreach (var obj in myTarget.items)
                            {
                                if (obj.uid.Contains(_base))
                                {
                                    int _testId = -1;
                                    if (int.TryParse(obj.uid.Replace(_base, ""), out _testId)) _maxId = Mathf.Max(_maxId, _testId);
                                }
                            }
                        }
                        else
                        {
                            foreach (var obj in myTarget.itemScriptableObjects)
                            {
                                if (obj.mItem.uid.Contains(_base))
                                {
                                    int _testId = -1;
                                    if (int.TryParse(obj.mItem.uid.Replace(_base, ""), out _testId)) _maxId = Mathf.Max(_maxId, _testId);
                                }
                            }
                        }
                        _newItem.uid = _base + (_maxId + 1).ToString();
                        _newItem.fold = true;

                        if (myTarget.DataMode == ItemDataMode.Unified)
                        {
                            for (int u = 0; u < myTarget.items.Count; u++)
                            {
                                myTarget.items[u].fold = false;
                            }
                            myTarget.items.Add(_newItem);
                        }
                        else
                        {
                            for (int u = 0; u < myTarget.itemScriptableObjects.Count; u++)
                            {
                                myTarget.itemScriptableObjects[u].mItem.fold = false;
                            }
                            if (!AssetDatabase.IsValidFolder(EditorUtils.DataPath + "Items/"))
                            {
                                AssetDatabase.CreateFolder(EditorUtils.DataPath.Replace("Data/", "Data"), "Items");
                                AssetDatabase.Refresh();
                            }
                            ItemScriptableObject _newObj = (ItemScriptableObject)ScriptableObject.CreateInstance(typeof(ItemScriptableObject));
                            _newObj.mItem = _newItem;
                            string _basepath = EditorUtils.DataPath + "Items/" + _newItem.uid;
                            string _path = _basepath + ".asset";
                            AssetDatabase.CreateAsset(_newObj, _path);
                            AssetDatabase.Refresh();
                            EditorUtility.SetDirty(_newObj);
                            myTarget.itemScriptableObjects.Add(_newObj);
                        }
                        _valueChanged = true;
                        ApplyAndSave(myTarget, EditorAsset);
                        EditorGUI.FocusTextInControl(null);
                    }
                    GUI.backgroundColor = EditorUtils._black;
                    if (GUILayout.Button("Fold All", GUILayout.Width(50)))
                    {
                        if (myTarget.DataMode == ItemDataMode.Unified)
                        {
                            for (int i = 0; i < myTarget.items.Count; i++) myTarget.items[i].fold = false;
                        }
                        else
                        {
                            for (int i = 0; i < myTarget.itemScriptableObjects.Count; i++) myTarget.itemScriptableObjects[i].mItem.fold = false;
                        }
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Export to Json", GUILayout.Width(100)))
                    {
                        string _path = EditorUtility.SaveFilePanel("Export to Json", Application.dataPath, "ItemSetting.txt", "txt");
                        if (_path.Length != 0)
                        {
                            myTarget.ExportItemDataToJson(_path);
                        }
                    }
                    if (GUILayout.Button("Import from Json", GUILayout.Width(110)))
                    {
                        string _path = EditorUtility.OpenFilePanel("Import from Json", Application.dataPath, "txt");
                        if (_path.Length != 0)
                        {
                            myTarget.ImportItemDataFromJson(_path);
                            GUILayout.EndHorizontal();
                            EditorGUI.FocusTextInControl(null);
                            ApplyAndSave(myTarget, EditorAsset);
                            return;
                        }
                    }

                    EditorUtils.ResetColor();
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();

                    if (!string.IsNullOrEmpty(myTarget.SearchText))
                    {
                        EditorUtils.Label(EditorUtils.TextHint("Search results of [" + SearchOptions[myTarget.SearchType] + "] matching: '<color=#FF7733>" + myTarget.SearchText + "</color>' ..."), 20);
                    }

                    if (myTarget.DataMode == ItemDataMode.Unified)
                    {
                        List<string> _uids = new List<string>();
                        for (int i = 0; i < myTarget.items.Count; i++)
                        {
                            bool _duplicateUid = _uids.Contains(myTarget.items[i].uid);
                            if (!_duplicateUid) _uids.Add(myTarget.items[i].uid);
                            if (EditorAsset != null)
                            {
                                if (EditorAsset.ItemAssets.Count <= i)
                                {
                                    EditorAssetData newData = new EditorAssetData();
                                    newData.uid = myTarget.items[i].id;
                                    newData.icon = myTarget.items[i].icon;
                                    EditorAsset.ItemAssets.Add(newData);
                                }
                                else
                                {
                                    if (EditorAsset.ItemAssets[i].icon != myTarget.items[i].icon && myTarget.items[i].iconLoadMethod == LoadMethod.DirectReference)
                                        EditorAsset.ItemAssets[i].icon = myTarget.items[i].icon;
                                    EditorAsset.ItemAssets[i].uid = myTarget.items[i].id;
                                }
                            }

                            if (isMatching(myTarget.items[i], myTarget))
                            {
                                int _id = myTarget.items[i].id;
                                GUILayout.BeginHorizontal();
                                GUI.backgroundColor = myTarget.items[i].fold ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
                                GUILayout.Label(myTarget.items[i].fold ? "[-]" : "[+]", GUILayout.Width(20));
                                if (_duplicateUid)
                                {
                                    GUI.backgroundColor = Color.white;
                                    GUILayout.Box(EditorUtils.GetTexture(EditorIcon.Warning), EditorUtils._iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                                    GUI.backgroundColor = Color.red;
                                }

                                string _key = myTarget.items[i].uid;
                                GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                                GUILayout.Space(5);
                                GUILayout.Box(EditorAsset.ItemAssets[i].icon, EditorUtils._toolButtonStyle, GUILayout.Width(17), GUILayout.Height(17));
                                if (GUILayout.Button(new GUIContent(myTarget.items[i].name + " ( UID: " + _key + " , ID: " + _id + " )", "Click to expand"), EditorUtils._titleButtonStyle))
                                {
                                    myTarget.items[i].fold = !myTarget.items[i].fold;
                                    if (myTarget.items[i].fold)
                                    {
                                        for (int u = 0; u < myTarget.items.Count; u++)
                                        {
                                            if (u != i) myTarget.items[u].fold = false;
                                        }
                                    }
                                    EditorGUI.FocusTextInControl(null);
                                }
                                EditorUtils.ResetColor();
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy this item."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    ClipboardItem = myTarget.items[i].Copy();
                                    ClipboardAsset = EditorAsset.ItemAssets[i].Copy();
                                    EditorGUI.FocusTextInControl(null);
                                }
                                EditorUtils.ResetColor();
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste data of the copied item to replace this item."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    if (ClipboardItem != null)
                                    {
                                        string _olduid = myTarget.items[i].uid;
                                        myTarget.items[i] = ClipboardItem.Copy();
                                        EditorAsset.ItemAssets[i] = ClipboardAsset.Copy();
                                        EditorAsset.ItemAssets[i].uid = i;
                                        myTarget.items[i].uid = _olduid;
                                        _valueChanged = true;
                                        EditorGUI.FocusTextInControl(null);
                                        ApplyAndSave(myTarget, EditorAsset);
                                    }
                                }
                                EditorUtils.ResetColor();
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of this item"), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    string _olduid = myTarget.items[i].uid;
                                    myTarget.items[i] = new Item();
                                    EditorAsset.ItemAssets[i].icon = null;
                                    myTarget.items[i].uid = _olduid;
                                    _valueChanged = true;
                                    EditorGUI.FocusTextInControl(null);
                                }
                                EditorUtils.ResetColor();
                                GUI.color = i > 0 ? Color.white : Color.gray;
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Up), "Move this item up."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    if (i > 0)
                                    {
                                        Item _mItem = myTarget.items[i].Copy();
                                        _mItem.fold = false;
                                        myTarget.items.RemoveAt(i);
                                        myTarget.items.Insert(i - 1, _mItem);
                                        EditorGUI.FocusTextInControl(null);
                                        ApplyAndSave(myTarget, EditorAsset);
                                        return;
                                    }
                                }
                                GUI.color = i < myTarget.items.Count - 1 ? Color.white : Color.gray;
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Down), "Move this item down."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    if (i < myTarget.items.Count - 1)
                                    {
                                        Item _mItem = myTarget.items[i].Copy();
                                        _mItem.fold = false;
                                        myTarget.items.RemoveAt(i);
                                        myTarget.items.Insert(i + 1, _mItem);
                                        EditorGUI.FocusTextInControl(null);
                                        ApplyAndSave(myTarget, EditorAsset);
                                        return;
                                    }
                                }
                                GUILayout.EndHorizontal();
                                GUI.color = Color.white;

                                GUI.backgroundColor = EditorUtils._black;
                                if (GUILayout.Button(new GUIContent("X", "Delete this item."), GUILayout.Width(20)))
                                {
                                    for (int w = 0; w < myTarget.items.Count; w++)
                                    {
                                        for (int v = 0; v < myTarget.items[w].craftMaterials.Count; v++)
                                        {
                                            if (Mathf.FloorToInt(myTarget.items[w].craftMaterials[v].x) == i)
                                            {
                                                myTarget.items[w].craftMaterials.RemoveAt(v);
                                            }
                                            else if (myTarget.items[w].craftMaterials[v].x > i)
                                            {
                                                myTarget.items[w].craftMaterials[v] = new Vector2(myTarget.items[w].craftMaterials[v].x - 1, myTarget.items[w].craftMaterials[v].y);
                                            }
                                        }
                                    }
                                    for (int w = 0; w < myTarget.EnhancingMaterials.Length; w++)
                                    {
                                        if (myTarget.EnhancingMaterials[w].x > i)
                                        {
                                            myTarget.EnhancingMaterials[w] = new Vector2(myTarget.EnhancingMaterials[w].x - 1, myTarget.EnhancingMaterials[w].y);
                                        }
                                    }
                                    if (myTarget.EnchantingMaterial.x > i)
                                    {
                                        myTarget.EnchantingMaterial = new Vector2(myTarget.EnchantingMaterial.x - 1, myTarget.EnchantingMaterial.y);
                                    }
                                    myTarget.IdManager.RemoveKey(_id);
                                    EditorAsset.ItemAssets.RemoveAt(i);
                                    myTarget.items.RemoveAt(i);
                                    EditorGUI.FocusTextInControl(null);
                                    ApplyAndSave(myTarget, EditorAsset);
                                    return;
                                }
                                EditorUtils.ResetColor();
                                GUILayout.EndHorizontal();

                                if (myTarget.items[i].fold)
                                {
                                    if (ItemInspector(myTarget.items[i], EditorAsset, _duplicateUid, _itemOption, _typeOptions, _qualityOptions, _currencyOption, _allAtt)) _valueChanged = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        List<string> _uids = new List<string>();
                        for (int i = 0; i < myTarget.itemScriptableObjects.Count; i++)
                        {
                            bool _duplicateUid = _uids.Contains(myTarget.itemScriptableObjects[i].mItem.uid);
                            if (!_duplicateUid) _uids.Add(myTarget.itemScriptableObjects[i].mItem.uid);
                            if (EditorAsset != null)
                            {
                                if (EditorAsset.ItemAssets.Count <= i)
                                {
                                    EditorAssetData newData = new EditorAssetData();
                                    newData.uid = myTarget.itemScriptableObjects[i].mItem.id;
                                    newData.icon = myTarget.itemScriptableObjects[i].mItem.icon;
                                    EditorAsset.ItemAssets.Add(newData);
                                }
                                else
                                {
                                    if (EditorAsset.ItemAssets[i].icon != myTarget.itemScriptableObjects[i].mItem.icon && myTarget.itemScriptableObjects[i].mItem.iconLoadMethod == LoadMethod.DirectReference)
                                        EditorAsset.ItemAssets[i].icon = myTarget.itemScriptableObjects[i].mItem.icon;
                                    EditorAsset.ItemAssets[i].uid = myTarget.itemScriptableObjects[i].mItem.id;
                                }
                            }

                            if (isMatching(myTarget.itemScriptableObjects[i].mItem, myTarget))
                            {
                                int _id = myTarget.itemScriptableObjects[i].mItem.id;
                                GUILayout.BeginHorizontal();
                                GUI.backgroundColor = myTarget.itemScriptableObjects[i].mItem.fold ? EditorUtils._activeColor2 : EditorUtils._disableColor2;
                                GUILayout.Label(myTarget.itemScriptableObjects[i].mItem.fold ? "[-]" : "[+]", GUILayout.Width(20));
                                if (_duplicateUid)
                                {
                                    GUI.backgroundColor = Color.white;
                                    GUILayout.Box(EditorUtils.GetTexture(EditorIcon.Warning), EditorUtils._iconStyle, GUILayout.Width(16), GUILayout.Height(16));
                                    GUI.backgroundColor = Color.red;
                                }

                                string _key = myTarget.itemScriptableObjects[i].mItem.uid;
                                GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                                GUILayout.Space(5);
                                GUILayout.Box(EditorAsset.ItemAssets[i].icon, EditorUtils._toolButtonStyle, GUILayout.Width(17), GUILayout.Height(17));
                                if (GUILayout.Button(new GUIContent(myTarget.itemScriptableObjects[i].mItem.name + " ( UID: " + _key + " , ID: " + _id + " )", "Click to expand"), EditorUtils._titleButtonStyle))
                                {
                                    myTarget.itemScriptableObjects[i].mItem.fold = !myTarget.itemScriptableObjects[i].mItem.fold;
                                    if (myTarget.itemScriptableObjects[i].mItem.fold)
                                    {
                                        for (int u = 0; u < myTarget.itemScriptableObjects.Count; u++)
                                        {
                                            if (u != i) myTarget.itemScriptableObjects[u].mItem.fold = false;
                                        }
                                    }
                                    EditorGUI.FocusTextInControl(null);
                                }
                                EditorUtils.ResetColor();
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy this item."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    ClipboardItem = myTarget.itemScriptableObjects[i].mItem.Copy();
                                    ClipboardAsset = EditorAsset.ItemAssets[i].Copy();
                                    EditorGUI.FocusTextInControl(null);
                                }
                                EditorUtils.ResetColor();
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste data of the copied item to replace this item."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    if (ClipboardItem != null)
                                    {
                                        string _olduid = myTarget.itemScriptableObjects[i].mItem.uid;
                                        myTarget.itemScriptableObjects[i].mItem = ClipboardItem.Copy();
                                        EditorAsset.ItemAssets[i] = ClipboardAsset.Copy();
                                        EditorAsset.ItemAssets[i].uid = i;
                                        myTarget.itemScriptableObjects[i].mItem.uid = _olduid;
                                        _valueChanged = true;
                                        EditorGUI.FocusTextInControl(null);
                                        ApplyAndSave(myTarget, EditorAsset);
                                    }
                                }
                                EditorUtils.ResetColor();
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of this item"), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    string _olduid = myTarget.itemScriptableObjects[i].mItem.uid;
                                    myTarget.itemScriptableObjects[i].mItem = new Item();
                                    EditorAsset.ItemAssets[i].icon = null;
                                    myTarget.itemScriptableObjects[i].mItem.uid = _olduid;
                                    _valueChanged = true;
                                    EditorGUI.FocusTextInControl(null);
                                }
                                EditorUtils.ResetColor();
                                GUI.color = i > 0 ? Color.white : Color.gray;
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Up), "Move this item up."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    if (i > 0)
                                    {
                                        ItemScriptableObject _mItem = myTarget.itemScriptableObjects[i];
                                        _mItem.mItem.fold = false;
                                        myTarget.itemScriptableObjects.RemoveAt(i);
                                        myTarget.itemScriptableObjects.Insert(i - 1, _mItem);
                                        EditorGUI.FocusTextInControl(null);
                                        ApplyAndSave(myTarget, EditorAsset);
                                        return;
                                    }
                                }
                                GUI.color = i < myTarget.itemScriptableObjects.Count - 1 ? Color.white : Color.gray;
                                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Down), "Move this item down."), EditorUtils._toolButtonStyle, GUILayout.Width(15)))
                                {
                                    if (i < myTarget.itemScriptableObjects.Count - 1)
                                    {
                                        ItemScriptableObject _mItem = myTarget.itemScriptableObjects[i];
                                        _mItem.mItem.fold = false;
                                        myTarget.itemScriptableObjects.RemoveAt(i);
                                        myTarget.itemScriptableObjects.Insert(i + 1, _mItem);
                                        EditorGUI.FocusTextInControl(null);
                                        ApplyAndSave(myTarget, EditorAsset);
                                        return;
                                    }
                                }
                                GUILayout.EndHorizontal();
                                GUI.color = Color.white;

                                GUI.backgroundColor = EditorUtils._black;
                                if (GUILayout.Button(new GUIContent("X", "Delete this item."), GUILayout.Width(20)))
                                {
                                    bool _deleteFile = EditorUtility.DisplayDialog("Remove Item", "Do you want to delete the ScriptableObject asset as well?", "Yes", "No");
                                    for (int w = 0; w < myTarget.itemScriptableObjects.Count; w++)
                                    {
                                        for (int v = 0; v < myTarget.itemScriptableObjects[w].mItem.craftMaterials.Count; v++)
                                        {
                                            if (Mathf.FloorToInt(myTarget.itemScriptableObjects[w].mItem.craftMaterials[v].x) == i)
                                            {
                                                myTarget.itemScriptableObjects[w].mItem.craftMaterials.RemoveAt(v);
                                            }
                                            else if (myTarget.itemScriptableObjects[w].mItem.craftMaterials[v].x > i)
                                            {
                                                myTarget.itemScriptableObjects[w].mItem.craftMaterials[v] = new Vector2(myTarget.itemScriptableObjects[w].mItem.craftMaterials[v].x - 1, myTarget.itemScriptableObjects[w].mItem.craftMaterials[v].y);
                                            }
                                        }
                                    }
                                    for (int w = 0; w < myTarget.EnhancingMaterials.Length; w++)
                                    {
                                        if (myTarget.EnhancingMaterials[w].x > i)
                                        {
                                            myTarget.EnhancingMaterials[w] = new Vector2(myTarget.EnhancingMaterials[w].x - 1, myTarget.EnhancingMaterials[w].y);
                                        }
                                    }
                                    if (myTarget.EnchantingMaterial.x > i)
                                    {
                                        myTarget.EnchantingMaterial = new Vector2(myTarget.EnchantingMaterial.x - 1, myTarget.EnchantingMaterial.y);
                                    }
                                    if (_deleteFile)
                                    {
                                        string _itemFilePath = AssetDatabase.GetAssetPath(myTarget.itemScriptableObjects[i]);
                                        AssetDatabase.DeleteAsset(_itemFilePath);
                                    }
                                    myTarget.IdManager.RemoveKey(_id);
                                    EditorAsset.ItemAssets.RemoveAt(i);
                                    myTarget.itemScriptableObjects.RemoveAt(i);
                                    EditorGUI.FocusTextInControl(null);
                                    ApplyAndSave(myTarget, EditorAsset);
                                    return;
                                }
                                EditorUtils.ResetColor();
                                GUILayout.EndHorizontal();

                                if (myTarget.itemScriptableObjects[i].mItem.fold)
                                {
                                    var _oriFile = myTarget.itemScriptableObjects[i];
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    GUILayout.Label("ScriptableObject File:", GUILayout.Width(150));
                                    GUI.backgroundColor = EditorUtils._black;
                                    EditorGUI.BeginChangeCheck();
                                    myTarget.itemScriptableObjects[i] = (ItemScriptableObject)EditorGUILayout.ObjectField(myTarget.itemScriptableObjects[i], typeof(ItemScriptableObject), true);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        if (myTarget.itemScriptableObjects[i] == null)
                                        {
                                            myTarget.itemScriptableObjects[i] = _oriFile;
                                        }
                                        else
                                        {
                                            myTarget.itemScriptableObjects[i].mItem.fold = true;
                                            _oriFile.mItem.fold = false;
                                        }
                                        _valueChanged = true;
                                    }
                                    if (GUILayout.Button("Rename as UID"))
                                    {
                                        string _itemFilePath = AssetDatabase.GetAssetPath(myTarget.itemScriptableObjects[i]);
                                        AssetDatabase.RenameAsset(_itemFilePath, myTarget.itemScriptableObjects[i].mItem.uid);
                                        AssetDatabase.Refresh();
                                    }
                                    EditorUtils.ResetColor();
                                    GUILayout.Space(25);
                                    GUILayout.EndHorizontal();
                                    if (ItemInspector(myTarget.itemScriptableObjects[i].mItem, EditorAsset, _duplicateUid, _itemOption, _typeOptions, _qualityOptions, _currencyOption, _allAtt))
                                    {
                                        EditorUtility.SetDirty(myTarget.itemScriptableObjects[i]);
                                    }

                                }
                            }
                        }
                    }
                }

                EditorGUILayout.Separator();
            }

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUI.backgroundColor = myTarget.IdManager.uiFold ? EditorUtils._titleColor : EditorUtils._gray * 0.6F;
            if (GUILayout.Button(EditorUtils.TextHint("ID List Count: " + myTarget.IdManager.idToKey.Count, "An auto-generated list of unique integer IDs. Using integer IDs improves performance."), GUILayout.Width(120)))
            {
                myTarget.IdManager.uiFold = !myTarget.IdManager.uiFold;
            }
            GUI.backgroundColor = EditorUtils._black;
            if (GUILayout.Button("Rebuild", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Warning!", "Rebuilding the ID list will break the mapping between string UIDs and integer IDs. Any code accessing items by integer ID will stop working.", "I'm Aware", "Cancel"))
                {
                    myTarget.IdManager.idToKey.Clear();
                }
            }
            EditorUtils.ResetColor();
            GUILayout.EndHorizontal();
            if (myTarget.IdManager.uiFold)
            {
                foreach (var obj in myTarget.IdManager.idToKey)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    GUILayout.Box(" " + obj.key, EditorUtils._boxLeftStyle, GUILayout.Width(100));
                    GUILayout.Box(" " + obj.id.ToString(), EditorUtils._boxLeftStyle, GUILayout.Width(80));
                    GUILayout.EndHorizontal();
                }
            }


            if ((_valueChanged || GUI.changed) && !Application.isPlaying)
            {
                myTarget.GenerateUniqueHash();
                ApplyAndSave(myTarget, EditorAsset);
            }

            //base.OnInspectorGUI();
        }

        public static bool ItemInspector(Item _itemData, ItemEditorAssets _editorAsset, bool _duplicateUid, string[] _itemOption, string[] _typeOptions, string[] _qualityOptions, string[] _currencyOption, List<string> _allAtt)
        {
            bool _valueChanged = false;
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.BeginVertical(EditorUtils._groupStyle);

            if (_duplicateUid) EditorUtils.Warnning("Duplicated UID! Please make every UID unique.", 10);

            GUI.backgroundColor = _duplicateUid ? Color.red : EditorUtils._titleColor;
            string _uid = _itemData.uid;
            int _id = _itemData.id;
            EditorUtils.TextField(ref _itemData.uid, new GUIContent("UID:", "The unique string id of this item."), 10, 150, true);
            EditorUtils.ResetColor();
            if (_uid != _itemData.uid)
            {
                ItemObject.instance.IdManager.ReplaceKey(_id, _itemData.uid);
            }
            GUI.backgroundColor = EditorUtils._titleColor * 0.6F;
            EditorUtils.BoxInfo(new GUIContent("ID: " + _itemData.id.ToString(), "The unique integer id of this item. This id is auto generated,"), 10, true);
            EditorGUILayout.Separator();
            EditorUtils.TextField(ref _itemData.name, new GUIContent("Display Name:", "The name of the item."), 10, 150, true);
            EditorUtils.TextField(ref _itemData.description, new GUIContent("Description:", "The text displayed in the detailed information panel. Keep it as short as possible"), 10, 150);
            EditorUtils.HelpInfo("Use <br> for new line, use {attribute uid} for dynamic value of an attribute, for example: {atk}", 10, false);

            _itemData.type = Mathf.Clamp(_itemData.type, 0, ItemObject.instance.itemTypes.Count - 1);
            GUI.backgroundColor = EditorUtils._black;
            EditorUtils.PopUp(ref _itemData.type, _typeOptions, new GUIContent("Category: ", "The category to which this item belongs."), 10, 150, 150);

            _itemData.quality = Mathf.Clamp(_itemData.quality, 0, ItemObject.instance.itemQuality.Count - 1);
            GUI.backgroundColor = EditorUtils._black;
            EditorUtils.PopUp(ref _itemData.quality, _qualityOptions, new GUIContent("Quality: ", "The quality level of this item."), 10, 150, 150);
            EditorUtils.ResetColor();

            EditorGUILayout.Separator();

            LoadMethod _lastMethod = _itemData.iconLoadMethod;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label(new GUIContent("Icon Load Method:", "The icon can be directly referenced or load from Resources/AssetBundle/Custom Method during runtime for better performance."), GUILayout.Width(150));
            GUI.backgroundColor = EditorUtils._black;
            _itemData.iconLoadMethod = (LoadMethod)EditorGUILayout.EnumPopup(_itemData.iconLoadMethod, GUILayout.Width(150));
            GUILayout.EndHorizontal();
            EditorUtils.ResetColor();
            if (_lastMethod != _itemData.iconLoadMethod && _itemData.iconLoadMethod == LoadMethod.DirectReference)
            {
                _itemData.icon = GetIcon(_editorAsset, _id);
            }

            if (_itemData.iconLoadMethod != LoadMethod.DirectReference)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label(new GUIContent("Icon Path:", "The path of the icon texture."), GUILayout.Width(80));
                if (_itemData.iconLoadMethod == LoadMethod.Resources)
                {
                    GUI.color = EditorUtils._buttonColor;
                    GUILayout.Label("Resources/", GUILayout.Width(70));
                    GUI.color = Color.white;
                }
                if (string.IsNullOrEmpty(_itemData.iconPath) || _itemData.iconPath.Contains(" ")) GUI.color = EditorUtils._red;
                _itemData.iconPath = EditorGUILayout.TextField(_itemData.iconPath);
                GUI.color = Color.white;
                if (_itemData.iconLoadMethod == LoadMethod.Custom)
                {
                    GUI.color = EditorUtils._buttonColor;
                    GUILayout.Label("Format:BundleName#AssetPath", GUILayout.Width(200));
                    GUI.color = Color.white;
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                if (string.IsNullOrEmpty(_itemData.iconPath) || _itemData.iconPath.Contains(" "))
                {
                    EditorUtils.HelpInfo("You must fill in the path of the icon texture, make sure there is no invalid characters in the path.", 20);
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(new GUIContent("Icon: ", "Select the item's icon texture. The icon should be a transparent texture. A variety of icons are available in the \"Assets/SoftKitty/InventoryEngine/Textures\" folder."), GUILayout.Width(80));
            if (_itemData.iconLoadMethod == LoadMethod.DirectReference)
            {
                _itemData.icon = (Texture2D)EditorGUILayout.ObjectField(_itemData.icon, typeof(Texture2D), false);
            }
            else
            {
                Texture2D _iconPlaceHolder = GetIcon(_editorAsset, _id);
                EditorGUI.BeginChangeCheck();
                _iconPlaceHolder = (Texture2D)EditorGUILayout.ObjectField(_iconPlaceHolder, typeof(Texture2D), false);
                if (EditorGUI.EndChangeCheck())
                {
                    _editorAsset.SetIcon(_id, _iconPlaceHolder);
                }
                _itemData.icon = null;
                if (_itemData.iconLoadMethod == LoadMethod.Resources && GetIcon(_editorAsset, _id) != null)
                {
                    GUI.backgroundColor = EditorUtils._buttonColor;
                    if (GUILayout.Button("Apply Path", GUILayout.Width(100)))
                    {
                        string _iconPath = AssetDatabase.GetAssetPath(GetIcon(_editorAsset, _id));
                        int _resourceIndex = _iconPath.IndexOf("Resources");
                        if (_resourceIndex >= 0)
                        {
                            _itemData.iconPath = _iconPath.Substring(_resourceIndex + 10, _iconPath.Length - _resourceIndex - 10 - 4);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Warning", "Sorry, the icon texure is not inside a 'Resources' folder.", "OK");
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            if (_itemData.iconLoadMethod != LoadMethod.DirectReference)
            {
                EditorUtils.HelpInfo("The icon you selected here is just for editor preview only,it won't be loaded into memory during runtime.", 20);
            }
            EditorGUILayout.Separator();

            EditorUtils.IntField(ref _itemData.maxiumStack, new GUIContent("Maximum Stack:", "The maximum number of items that can stack in a single slot."), 10, true, 99, 150);
            _itemData.currency = Mathf.Clamp(_itemData.currency, 0, ItemObject.instance.currencies.Count - 1);
            EditorUtils.PopUpIntField(ref _itemData.currency, ref _itemData.price, _currencyOption,
                new GUIContent("Price:", "The trading price and its associated currency type."), 10, EditorUtils._black, EditorUtils._titleColor, 150, 130, 70);
            EditorUtils.FloatField(ref _itemData.weight, new GUIContent("Weight:",
                    "he weight of this item. The inventory interface includes a weight bar. You can create custom logic to slow down the player's movement or prevent them from moving when they are over-encumbered. Use GetWeight() to retrieve the current weight value of a character."), 10, true, 0.1F, 150);
            EditorUtils.IntSlider(ref _itemData.dropRates, 0, 100, new GUIContent("Drop Rates:", "When this item is in a loot pack's item pool, the drop rate determines the percentage chance of it dropping."), "%", 10, 150);
            EditorUtils.Toggle(ref _itemData.useable, new GUIContent("Useable:", "Toggle whether this item can be used/equipped via right-click or a key press on an action slot."), 10, 150);
            if (_itemData.useable)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUILayout.Label(new GUIContent("Use/Equip restriction:", "Player won't be able to use/equip this item if their stats not fulfill the below setting."), GUILayout.Width(150));
                bool _useRestriction = EditorGUILayout.Toggle(_itemData.restrictionValue > 0 && !string.IsNullOrEmpty(_itemData.restrictionKey));
                GUILayout.EndHorizontal();

                if (_useRestriction)
                {
                    if (_itemData.restrictionValue < 1) _itemData.restrictionValue = 1;
                    if (string.IsNullOrEmpty(_itemData.restrictionKey)) _itemData.restrictionKey = GameManager.AttributeData.AttributeList[0].uid;
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    int _sel = 0;
                    string _attName = "";
                    for (int x = 0; x < GameManager.AttributeData.AttributeList.Count; x++)
                    {
                        if (GameManager.AttributeData.AttributeList[x].uid == _itemData.restrictionKey)
                        {
                            _sel = x;
                            _attName = GameManager.AttributeData.AttributeList[x].name;
                            break;
                        }
                    }
                    GUI.backgroundColor = EditorUtils._black;
                    EditorGUI.BeginChangeCheck();
                    _sel = EditorGUILayout.Popup("", _sel, GameManager.AttributeData.AttributesNames, GUILayout.Width(120));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _itemData.restrictionKey = GameManager.AttributeData.AttributeList[_sel].uid;
                    }
                    GUILayout.Label(">=", GUILayout.Width(30));
                    GUI.backgroundColor = EditorUtils._titleColor;
                    _itemData.restrictionValue = EditorGUILayout.IntField(_itemData.restrictionValue, GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                    EditorUtils.ResetColor();
                    EditorUtils.HelpInfo("Player/NPC won't be able to use/equip this item if their " + _attName + " is less than " + _itemData.restrictionValue.ToString(), 50);

                }
                else
                {
                    _itemData.restrictionValue = 0;
                    _itemData.restrictionKey = "";
                }
            }

            EditorUtils.Toggle(ref _itemData.consumable, new GUIContent("Consumable:", "Toggle whether this item will be consumed upon use."), 10, 150);
            EditorUtils.Toggle(ref _itemData.tradeable, new GUIContent("Tradable:", "Toggle whether this item can be traded between the player and merchants."), 10, 150);
            EditorUtils.Toggle(ref _itemData.deletable, new GUIContent("Deletable:", "Toggle whether this item can be deleted by the player."), 10, 150);
            EditorUtils.Toggle(ref _itemData.visible, new GUIContent("Visible:", "If unchecked, the item is treated as a Hidden Item, which is useful for special items like [Skills]"), 10, 150);
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._titleColor * 0.6F;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
            GUILayout.Label(new GUIContent("Attributes: (" + _itemData.attributes.Count + ")", " The attributes of this item. Access the attributes using <InventoryData>().GetAttributeValue(key)."), GUILayout.Width(160));
            GUILayout.FlexibleSpace();
            EditorUtils.ResetColor();
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy the settings of the attributes"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                ClipboardAttributeList = new List<AttributeData>();
                for (int x = 0; x < _itemData.attributes.Count; x++)
                {
                    ClipboardAttributeList.Add(_itemData.attributes[x].Copy());
                }
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste the settings of the attributes"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (ClipboardAttributeList.Count > 0)
                {
                    _itemData.attributes.Clear();
                    for (int y = 0; y < ClipboardAttributeList.Count; y++)
                    {
                        _itemData.attributes.Add(ClipboardAttributeList[y].Copy());
                    }
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of the attributes"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _itemData.attributes.Clear();
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            List<string> _mAttList = new List<string>();
            int _randomStats = 0;
            foreach (var obj in _itemData.attributes)
            {
                if (!_mAttList.Contains(obj.uid)) _mAttList.Add(obj.uid);
                if (!obj.isFixed) _randomStats++;
            }
            List<string> _attOptionsList = new List<string>();

            foreach (var obj in GameManager.AttributeData.AttributesUidArray)
            {
                if (!_mAttList.Contains(obj)) _attOptionsList.Add(obj);
            }

            if (_randomStats > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.Label(new GUIContent("Maximum non-static attributes: ", "The maximum number of non-static extra attributes this item can have."), GUILayout.Width(200));
                _itemData.maximumRandomAttributes = EditorGUILayout.IntSlider(_itemData.maximumRandomAttributes, 1, _randomStats);
                GUILayout.Label("/" + _randomStats.ToString(), GUILayout.Width(30));
                GUILayout.Space(10);
                GUILayout.EndHorizontal();

                EditorUtils.HelpInfo("When an item is generated, it's assigned its static attributes, as defined by the 'Static' checkbox. " +
                    "In addition, it may receive a random number of dynamic, non-static attributes. The chance of receiving each non-static attribute is configurable. " +
                    "The specific values of these dynamic attributes are then randomized between the minimum and maximum values you set.", 10);
            }

            if (_attOptionsList.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUI.backgroundColor = EditorUtils._black;
                GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                GUI.backgroundColor = EditorUtils._titleColor;
                GUILayout.Box("New: ", EditorUtils._boxStyle, GUILayout.Width(40));
                GUI.color = Color.white;

                _newAttSel = Mathf.Clamp(_newAttSel, 0, _attOptionsList.Count - 1);
                GUI.backgroundColor = EditorUtils._black;
                _newAttSel = EditorGUILayout.Popup("", _newAttSel, _attOptionsList.ToArray(), GUILayout.Width(100));
                GUI.backgroundColor = EditorUtils._titleColor;
                if (GUILayout.Button("Add", GUILayout.Width(80)))
                {
                    string _attUid = GameManager.AttributeData.AttributeList[_allAtt.IndexOf(_attOptionsList[_newAttSel])].uid;
                    AttributeData _newAttribute = new AttributeData(_attUid, 0F, "");
                    _itemData.attributes.Add(_newAttribute);
                    EditorGUI.FocusTextInControl(null);
                    _valueChanged = true;
                }
                EditorUtils.ResetColor();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndHorizontal();
            }
            for (int u = 0; u < _itemData.attributes.Count; u++)
            {
                if (!_allAtt.Contains(_itemData.attributes[u].uid))
                {
                    _itemData.attributes.RemoveAt(u);
                    break;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);

                GUILayout.Box(GameManager.GetAttribute(_itemData.attributes[u].uid).visible ? EditorUtils.GetTexture(EditorIcon.visible0) : EditorUtils.GetTexture(EditorIcon.visible1), EditorUtils._toolButtonStyle, GUILayout.Width(17), GUILayout.Height(17));

                GUI.backgroundColor = EditorUtils._titleColor;
                GUILayout.Box(new GUIContent(GameManager.GetAttribute(_itemData.attributes[u].uid).name, "UID: " + _itemData.attributes[u].uid), EditorUtils._titleButtonStyle, GUILayout.Width(120), GUILayout.Height(20));
                EditorUtils.ResetColor();
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy UID to clipboard."), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    EditorGUI.FocusTextInControl(null);
                    GUIUtility.systemCopyBuffer = _itemData.attributes[u].uid;
                    EditorUtils.ShowBoxNotification("UID string copied!");
                }
                GUILayout.Label(" :", GUILayout.Width(15));
                _itemData.attributes[u].isFixed = !GUILayout.Toggle(!_itemData.attributes[u].isFixed,
                    new GUIContent("Random", "Randomized this attribute when acquire. (eg: for rogue-like games)"), GUILayout.Width(70), GUILayout.Height(20));
                if (GameManager.GetAttribute(_itemData.attributes[u].uid).stringValue)
                {
                    _itemData.attributes[u].stringValue = EditorGUILayout.TextField(_itemData.attributes[u].stringValue, GUILayout.Width(100), GUILayout.Height(20)).ToString();
                }
                else if (_itemData.attributes[u].isFixed)
                {
                    _itemData.attributes[u].floatValue = EditorGUILayout.FloatField(_itemData.attributes[u].floatValue, GUILayout.Width(100), GUILayout.Height(20));
                    _itemData.attributes[u].minValue = _itemData.attributes[u].floatValue;
                    _itemData.attributes[u].maxValue = _itemData.attributes[u].floatValue;
                }
                else
                {
                    _itemData.attributes[u].minValue = EditorGUILayout.FloatField(_itemData.attributes[u].minValue, GUILayout.Width(38), GUILayout.Height(20));
                    GUILayout.Label("~", GUILayout.Width(15));
                    _itemData.attributes[u].maxValue = EditorGUILayout.FloatField(_itemData.attributes[u].maxValue, GUILayout.Width(38), GUILayout.Height(20));
                    _itemData.attributes[u].floatValue = _itemData.attributes[u].minValue;
                }

                GUI.backgroundColor = Color.white;


                GUI.color = u > 0 ? Color.white : Color.gray;
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Up), "Move this attribute up."), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (u > 0)
                    {
                        AttributeData _mAtt = _itemData.attributes[u].Copy();
                        _itemData.attributes.RemoveAt(u);
                        _itemData.attributes.Insert(u - 1, _mAtt);
                        EditorGUI.FocusTextInControl(null);
                        ApplyAndSave(ItemObject.instance, _editorAsset);
                        return _valueChanged || GUI.changed;
                    }
                }
                GUI.color = u < _itemData.attributes.Count - 1 ? Color.white : Color.gray;
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Down), "Move this attribute down."), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (u < _itemData.attributes.Count - 1)
                    {
                        AttributeData _mAtt = _itemData.attributes[u].Copy();
                        _itemData.attributes.RemoveAt(u);
                        _itemData.attributes.Insert(u + 1, _mAtt);
                        EditorGUI.FocusTextInControl(null);
                        ApplyAndSave(ItemObject.instance, _editorAsset);
                        return _valueChanged || GUI.changed;
                    }
                }

                GUI.backgroundColor = EditorUtils._black;
                GUI.color = Color.white;
                if (GUILayout.Button(new GUIContent("X", "Delete this attribute."), GUILayout.Width(25), GUILayout.Height(20)))
                {
                    _itemData.attributes.RemoveAt(u);
                    EditorGUI.FocusTextInControl(null);
                    ApplyAndSave(ItemObject.instance, _editorAsset);
                    return _valueChanged || GUI.changed;
                }
                EditorUtils.ResetColor();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

            }

            EditorGUILayout.Separator();


            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._red * 0.6F;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
            EditorUtils.ResetColor();
            GUILayout.Label(new GUIContent("Use Actions (" + _itemData.actions.Count + "): ",
                "When this item is used, the commands in this list are sent to all registered callbacks. Register callbacks using GameManager.PlayerInventoryData.RegisterItemUseCallback()."), GUILayout.Width(140));

            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Add), "Create a new action command."), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _itemData.actions.Add("New Action" + _itemData.actions.Count.ToString());
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy the settings of the actions"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                ClipboardStringList = _itemData.actions;
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste the settings of the actions"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (ClipboardStringList.Count > 0)
                {
                    _itemData.actions.Clear();
                    for (int y = 0; y < ClipboardStringList.Count; y++)
                    {
                        _itemData.actions.Add(ClipboardStringList[y]);
                    }
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of the actions"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _itemData.actions.Clear();
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            for (int u = 0; u < _itemData.actions.Count; u++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUI.backgroundColor = EditorUtils._red * 0.6F;
                GUILayout.Label(">", GUILayout.Width(15));
                _itemData.actions[u] = EditorGUILayout.TextField(_itemData.actions[u], GUILayout.Width(180));
                EditorUtils.ResetColor();
                GUI.backgroundColor = EditorUtils._black;
                if (GUILayout.Button(new GUIContent("X", "Delete this action command."), GUILayout.Width(20)))
                {
                    _itemData.actions.RemoveAt(u);
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                    ApplyAndSave(ItemObject.instance, _editorAsset);
                    return _valueChanged || GUI.changed;
                }
                EditorUtils.ResetColor();
                GUILayout.Space(30);
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._buttonColor * 0.6F;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
            EditorUtils.ResetColor();
            GUILayout.Label(new GUIContent("Tags: (" + _itemData.tags.Count + ")",
                "Tags are useful for providing additional definitions to items. For example, for equipment, tags can define the slot it belongs to (Head, Torso, Legs, etc.), whether a weapon is two-handed or one-handed, or if an item is a crafting blueprint."), GUILayout.Width(140));
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Add), "Create a new tag."), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _itemData.tags.Add("New Tag" + _itemData.tags.Count.ToString());
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy the settings of the tags"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                ClipboardStringList = _itemData.tags;
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste the settings of the tags"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (ClipboardStringList.Count > 0)
                {
                    _itemData.tags.Clear();
                    for (int y = 0; y < ClipboardStringList.Count; y++)
                    {
                        _itemData.tags.Add(ClipboardStringList[y]);
                    }
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of the tags"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _itemData.tags.Clear();
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            for (int u = 0; u < _itemData.tags.Count; u++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUI.backgroundColor = EditorUtils._buttonColor * 0.6F;
                GUILayout.Label(">", GUILayout.Width(15));
                _itemData.tags[u] = EditorGUILayout.TextField(_itemData.tags[u], GUILayout.Width(180));
                EditorUtils.ResetColor();
                GUI.backgroundColor = EditorUtils._black;
                if (GUILayout.Button(new GUIContent("X", "Delete this tag."), GUILayout.Width(25)))
                {
                    _itemData.tags.RemoveAt(u);
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                    ApplyAndSave(ItemObject.instance, _editorAsset);
                    return _valueChanged || GUI.changed;
                }
                EditorUtils.ResetColor();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._titleColor * 0.6F;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
            EditorUtils.ResetColor();
            GUILayout.Label(new GUIContent("Crafting Materials: (" + _itemData.craftMaterials.Count + ")", "Specify the items required as materials when crafting this item."), GUILayout.Width(140));

            if (_itemData.craftMaterials.Count < 4)
            {
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Add), "Add a new crafting material"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    _itemData.craftMaterials.Add(new Vector2(0, 1));
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy the settings of the materials"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                ClipboardVector2List = _itemData.craftMaterials;
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste the settings of the materials"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (ClipboardVector2List.Count > 0)
                {
                    _itemData.craftMaterials.Clear();
                    for (int y = 0; y < ClipboardVector2List.Count; y++)
                    {
                        _itemData.craftMaterials.Add(ClipboardVector2List[y]);
                    }
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
            }
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of the materials"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _itemData.craftMaterials.Clear();
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            for (int u = 0; u < _itemData.craftMaterials.Count; u++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUI.backgroundColor = EditorUtils._titleColor * 0.6F;
                GUILayout.Label(">", GUILayout.Width(15));
                int _matItemid = Mathf.FloorToInt(Mathf.Max(Mathf.FloorToInt(_itemData.craftMaterials[u].x), 0));
                int _mat = ItemObject.instance.IndexOfItems(_matItemid);
                if (_mat < 0)
                {
                    _mat = 0;
                }
                else
                {
                    ItemIcon(_editorAsset, _matItemid);
                }
                EditorGUI.BeginChangeCheck();
                _mat = EditorGUILayout.Popup("", _mat, _itemOption, GUILayout.Width(180), GUILayout.Height(20));
                if (EditorGUI.EndChangeCheck()) _itemData.craftMaterials[u] = new Vector2(ItemObject.instance.GetItemByIndex(_mat).id, _itemData.craftMaterials[u].y);
                GUILayout.Label("x", GUILayout.Width(30), GUILayout.Height(20));
                _itemData.craftMaterials[u] = new Vector2(_itemData.craftMaterials[u].x, EditorGUILayout.IntField(Mathf.FloorToInt(_itemData.craftMaterials[u].y), GUILayout.Width(70), GUILayout.Height(20)));
                EditorUtils.ResetColor();
                GUI.backgroundColor = EditorUtils._black;
                if (GUILayout.Button(new GUIContent("X", "Remove this crafting material."), GUILayout.Width(25)))
                {
                    _itemData.craftMaterials.RemoveAt(u);
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
                EditorUtils.ResetColor();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();


            if (ItemObject.instance.EnableSocketing && _itemData.type == ItemObject.instance.SocketedCategoryFilter)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUI.backgroundColor = EditorUtils._black;
                GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
                EditorUtils.ResetColor();
                GUILayout.Label(new GUIContent("Socketing Tags: (" + _itemData.socketingTag.Count + ")", "This item will only recieve skocketing items with any tag in this list."), GUILayout.Width(140));
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Add), "Add a new socketing tag filter"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    _itemData.socketingTag.Add("");
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Copy), "Copy the settings of the tag filter"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    ClipboardStringList = _itemData.socketingTag;
                }
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Paste), "Paste the settings of the tag filter"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    if (ClipboardStringList.Count > 0)
                    {
                        _itemData.socketingTag.Clear();
                        for (int y = 0; y < ClipboardStringList.Count; y++)
                        {
                            _itemData.socketingTag.Add(ClipboardStringList[y]);
                        }
                        _valueChanged = true;
                        EditorGUI.FocusTextInControl(null);
                    }
                }
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of the tag filter"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    _itemData.socketingTag.Clear();
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                GUILayout.EndHorizontal();

                for (int u = 0; u < _itemData.socketingTag.Count; u++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.backgroundColor = EditorUtils._black;
                    GUILayout.Label(">", GUILayout.Width(15));
                    _itemData.socketingTag[u] = EditorGUILayout.TextField(_itemData.socketingTag[u], GUILayout.Width(180));
                    if (GUILayout.Button(new GUIContent("X", "Delete this socketing tag filter."), GUILayout.Width(25)))
                    {
                        _itemData.socketingTag.RemoveAt(u);
                        _valueChanged = true;
                        EditorGUI.FocusTextInControl(null);
                        ApplyAndSave(ItemObject.instance, _editorAsset);
                        return _valueChanged || GUI.changed;
                    }
                    EditorUtils.ResetColor();
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._gray;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
            EditorUtils.ResetColor();
            GUILayout.Label(new GUIContent("Custom Data: (" + _itemData.customData.Count + ")", "Setup custom data for audio clips,prefabs,images,etc"), GUILayout.Width(140));
            if (_itemData.customData.Count < 4)
            {
                if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Add), "Add a new custom data"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    _itemData.customData.Add(new CustomField() { key = "Custom" + _itemData.customData.Count.ToString(), value = null });
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent(EditorUtils.GetTexture(EditorIcon.Reset), "Reset the settings of the custom data"), EditorUtils._toolButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
            {
                _itemData.customData.Clear();
                _valueChanged = true;
                EditorGUI.FocusTextInControl(null);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            EditorUtils.HelpInfo("You can attach custom data of any Object type to items, identified by unique string keys." +
                "Use the item.GetCustomData(string _key) function to retrieve this data at any time.", 30);

            for (int u = 0; u < _itemData.customData.Count; u++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUI.backgroundColor = EditorUtils._gray;
                GUILayout.Label(">", GUILayout.Width(15));
                GUILayout.Label("Key:", GUILayout.Width(40));
                _itemData.customData[u].key = GUILayout.TextField(_itemData.customData[u].key, GUILayout.Width(80));
                GUI.backgroundColor = EditorUtils._black;
                GUILayout.Label("Load Method:", GUILayout.Width(90));
                _itemData.customData[u].loadMethod = (LoadMethod)EditorGUILayout.EnumPopup(_itemData.customData[u].loadMethod, GUILayout.Width(120));
                GUI.backgroundColor = EditorUtils._black;
                if (GUILayout.Button(new GUIContent("X", "Remove this custom data."), GUILayout.Width(25)))
                {
                    _itemData.customData.RemoveAt(u);
                    _valueChanged = true;
                    EditorGUI.FocusTextInControl(null);
                }
                EditorUtils.ResetColor();
                GUILayout.Space(20);
                GUILayout.EndHorizontal();

                if (u < _itemData.customData.Count)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    if (_itemData.customData[u].loadMethod == LoadMethod.DirectReference)
                    {
                        GUILayout.Label("Data:", GUILayout.Width(40));
                        _itemData.customData[u].value = EditorGUILayout.ObjectField(_itemData.customData[u].value, typeof(Object), false, GUILayout.Width(150));
                    }
                    else
                    {
                        _itemData.customData[u].value = null;
                        GUILayout.Label("Data Path:", GUILayout.Width(70));
                        if (_itemData.customData[u].loadMethod == LoadMethod.Resources)
                        {
                            GUI.color = EditorUtils._titleColor;
                            GUI.backgroundColor = EditorUtils._titleColor;
                            GUILayout.Box("Resources/", EditorUtils._boxStyle, GUILayout.Width(70));
                            EditorUtils.ResetColor();
                        }
                        if (string.IsNullOrEmpty(_itemData.customData[u].loadPath) || _itemData.customData[u].loadPath.Contains(" ")) GUI.color = Color.red;
                        _itemData.customData[u].loadPath = GUILayout.TextField(_itemData.customData[u].loadPath, GUILayout.Height(18));

                        if (_itemData.customData[u].loadMethod == LoadMethod.Custom)
                        {
                            GUI.color = EditorUtils._titleColor;
                            GUI.backgroundColor = EditorUtils._titleColor;
                            GUILayout.Box("Format:BundleName#AssetPath", EditorUtils._boxStyle, GUILayout.Width(200));
                            EditorUtils.ResetColor();
                        }
                    }
                    GUILayout.Space(20);
                    GUILayout.EndHorizontal();

                    if (_itemData.customData[u].loadMethod != LoadMethod.DirectReference && (string.IsNullOrEmpty(_itemData.customData[u].loadPath) || _itemData.customData[u].loadPath.Contains(" ")))
                    {
                        EditorUtils.Warnning("You must fill in the path of the data object, make sure there is no invalid characters in the path.", 50);
                    }
                }
            }
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.backgroundColor = EditorUtils._titleColor;
            GUILayout.BeginHorizontal(EditorUtils._titleBgStyle);
            EditorUtils.ResetColor();
            GUILayout.Label(new GUIContent("[Master Character Creator] Appearance:", "Specify the appearance mesh of this item."));
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

#if MASTER_CHARACTER_CREATOR

            if (CharacterCustomizationObject.instance == null)
            {
                EditorUtils.Warnning("Character Customization Object is not assigned, You can create this object via the context menu in any Project folder:" +
                "<color=6688FF>Create ¡ú SoftKitty ¡ú Data Objects ¡ú Character Customization</color>" +
                "You can assign the created asset to the database in: <color=6688FF>Project Settings ¡ú SoftKitty ¡ú Data Settings ¡ú Data</color>", 30);

                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GUI.backgroundColor = EditorUtils._buttonColor;
                if (GUILayout.Button("Assign One"))
                {
                    SGD_SettingsProvider.CurrentSettings.data_expand = true;
                    SettingsService.OpenProjectSettings("Project/SoftKitty/SubData - Character Customization");
                }
                EditorUtils.ResetColor();
                GUILayout.EndHorizontal();
            }
            else
            {
                int _outfitEnumCount = 4;
                int[] _outfitSlotsCountM = new int[_outfitEnumCount];
                int[] _outfitSlotsCountF = new int[_outfitEnumCount];
                string[] _outfitSlotsOption = new string[_outfitEnumCount];
                if (CharacterCustomizationObject.instance != null)
                {
                    _outfitEnumCount = System.Enum.GetValues(typeof(MasterCharacterCreator.OutfitSlots)).Length;
                    _outfitSlotsCountM = new int[_outfitEnumCount];
                    _outfitSlotsCountF = new int[_outfitEnumCount];
                    _outfitSlotsOption = new string[_outfitEnumCount];
                    for (int i = 0; i < _outfitSlotsOption.Length; i++) _outfitSlotsOption[i] = ((MasterCharacterCreator.OutfitSlots)i).ToString();
                    for (int i = 0; i < _outfitSlotsCountM.Length; i++)
                    {
                        _outfitSlotsCountM[i] = CharacterCustomizationObject.instance.GetOutfitSettings(MasterCharacterCreator.Sex.Male, (MasterCharacterCreator.OutfitSlots)i).Length;
                        _outfitSlotsCountF[i] = CharacterCustomizationObject.instance.GetOutfitSettings(MasterCharacterCreator.Sex.Female, (MasterCharacterCreator.OutfitSlots)i).Length;
                    }
                }

                if (_itemData.equipAppearance == null)
                {
                    _itemData.equipAppearance = new MasterCharacterCreator.EquipmentAppearance();
                    _itemData.equipAppearance.MaleMeshId = 0;
                    _itemData.equipAppearance.FemaleMeshId = 0;
                    _itemData.equipAppearance.UseCustomColor = false;
                }
                if (_itemData.equipAppearance.MaleMeshId == 0 && _itemData.equipAppearance.FemaleMeshId == 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.backgroundColor = EditorUtils._buttonColor;
                    if (GUILayout.Button("Create Mesh Binding", GUILayout.Width(150)))
                    {
                        _itemData.equipAppearance.MaleMeshId = 1;
                        _itemData.equipAppearance.FemaleMeshId = 1;
                        _itemData.equipAppearance.UseCustomColor = false;
                        _itemData.equipAppearance.CustomColor1 = Color.white;
                        _itemData.equipAppearance.CustomColor2 = Color.white;
                        _itemData.equipAppearance.CustomColor3 = Color.white;
                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUI.backgroundColor = EditorUtils._red;
                    if (GUILayout.Button("Remove Mesh Binding", GUILayout.Width(150)))
                    {
                        _itemData.equipAppearance.MaleMeshId = 0;
                        _itemData.equipAppearance.FemaleMeshId = 0;
                        _itemData.equipAppearance.UseCustomColor = false;
                        if (MeshPreview.instance != null) MeshPreview.instance.Close();
                        return _valueChanged || GUI.changed;
                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("Slot:", GUILayout.Width(120));
                    EditorGUI.BeginChangeCheck();
                    int _sel = (int)_itemData.equipAppearance.Type;
                    _sel = EditorGUILayout.Popup("", _sel, _outfitSlotsOption, GUILayout.Width(100));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _itemData.equipAppearance.Type = (MasterCharacterCreator.OutfitSlots)_sel;
                        _itemData.equipAppearance.MaleMeshId = 1;
                        _itemData.equipAppearance.FemaleMeshId = 1;
                        if (MeshPreview.instance != null) MeshPreview.instance.Close();
                    }
                    GUILayout.EndHorizontal();

                    MasterCharacterCreator.OutfitInfo _outfitInfoM = CharacterCustomizationObject.instance.GetOutfitSettings(MasterCharacterCreator.Sex.Male, _itemData.equipAppearance.Type)[_itemData.equipAppearance.MaleMeshId];
                    MasterCharacterCreator.OutfitInfo _outfitInfoF = CharacterCustomizationObject.instance.GetOutfitSettings(MasterCharacterCreator.Sex.Female, _itemData.equipAppearance.Type)[_itemData.equipAppearance.FemaleMeshId];

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("Male Mesh id:", GUILayout.Width(120));
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Preview", GUILayout.Width(100)))
                    {
                        if (MeshPreview.instance != null) MeshPreview.instance.Close();
                        if (_itemData.equipAppearance.Type ==  MasterCharacterCreator.OutfitSlots.Back || _itemData.equipAppearance.Type == MasterCharacterCreator.OutfitSlots.Tail)
                        {
                            MeshPreview.ShowPreview("", "", "Assets/SoftKitty/MasterCharacterCreator/Resources/" + _outfitInfoM.MeshPath + ".prefab");
                        }
                        else
                        {
                            string _path = "Assets/SoftKitty/MasterCharacterCreator/Resources/MasterCharacterCreator/Core/PreviewModel.prefab";
                            MeshPreview.ShowPreview(_outfitInfoM.MeshPath, _outfitInfoM.MaterialPath, _path);
                        }
                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    int _maleId = _itemData.equipAppearance.MaleMeshId;
                    _itemData.equipAppearance.MaleMeshId = EditorGUILayout.IntSlider(_itemData.equipAppearance.MaleMeshId, 1, _outfitSlotsCountM[_sel] - 1, GUILayout.Width(200));
                    GUILayout.Label(_outfitInfoM.DisplayName);
                    GUILayout.EndHorizontal();
                    if (MeshPreview.instance != null && _maleId != _itemData.equipAppearance.MaleMeshId)
                    {
                        if (MeshPreview.instance != null) MeshPreview.instance.Close();
                        _outfitInfoM = CharacterCustomizationObject.instance.GetOutfitSettings(MasterCharacterCreator.Sex.Male, _itemData.equipAppearance.Type)[_itemData.equipAppearance.MaleMeshId];
                        if (_itemData.equipAppearance.Type == MasterCharacterCreator.OutfitSlots.Back || _itemData.equipAppearance.Type == MasterCharacterCreator.OutfitSlots.Tail)
                        {
                            MeshPreview.ShowPreview("", "", "Assets/SoftKitty/MasterCharacterCreator/Resources/" + _outfitInfoM.MeshPath + ".prefab");
                        }
                        else
                        {
                            string _path = "Assets/SoftKitty/MasterCharacterCreator/Resources/MasterCharacterCreator/Core/PreviewModel.prefab";
                            MeshPreview.ShowPreview(_outfitInfoM.MeshPath, _outfitInfoM.MaterialPath, _path);
                        }
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("Female Mesh id:", GUILayout.Width(120));
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Preview", GUILayout.Width(100)))
                    {
                        if (MeshPreview.instance != null) MeshPreview.instance.Close();
                        if (_itemData.equipAppearance.Type == MasterCharacterCreator.OutfitSlots.Back || _itemData.equipAppearance.Type == MasterCharacterCreator.OutfitSlots.Tail)
                        {
                            MeshPreview.ShowPreview("", "", "Assets/SoftKitty/MasterCharacterCreator/Resources/" + _outfitInfoF.MeshPath + ".prefab");
                        }
                        else
                        {
                            string _path = "Assets/SoftKitty/MasterCharacterCreator/Resources/MasterCharacterCreator/Core/PreviewModel.prefab";
                            MeshPreview.ShowPreview(_outfitInfoF.MeshPath, _outfitInfoF.MaterialPath, _path);
                        }
                    }
                    EditorUtils.ResetColor();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    int _femaleId = _itemData.equipAppearance.FemaleMeshId;
                    _itemData.equipAppearance.FemaleMeshId = EditorGUILayout.IntSlider(_itemData.equipAppearance.FemaleMeshId, 1, _outfitSlotsCountF[_sel] - 1, GUILayout.Width(200));
                    GUILayout.Label(_outfitInfoF.DisplayName);
                    GUILayout.EndHorizontal();

                    if (MeshPreview.instance != null && _femaleId != _itemData.equipAppearance.FemaleMeshId)
                    {

                        if (MeshPreview.instance != null) MeshPreview.instance.Close();
                        _outfitInfoF = CharacterCustomizationObject.instance.GetOutfitSettings(MasterCharacterCreator.Sex.Female, _itemData.equipAppearance.Type)[_itemData.equipAppearance.FemaleMeshId];
                        if (_itemData.equipAppearance.Type == MasterCharacterCreator.OutfitSlots.Back || _itemData.equipAppearance.Type == MasterCharacterCreator.OutfitSlots.Tail)
                        {
                            MeshPreview.ShowPreview("", "", "Assets/SoftKitty/MasterCharacterCreator/Resources/" + _outfitInfoF.MeshPath + ".prefab");
                        }
                        else
                        {
                            string _path = "Assets/SoftKitty/MasterCharacterCreator/Resources/MasterCharacterCreator/Core/PreviewModel.prefab";
                            MeshPreview.ShowPreview(_outfitInfoF.MeshPath, _outfitInfoF.MaterialPath, _path);
                        }
                    }

                    bool _useCustomColor = _itemData.equipAppearance.UseCustomColor;
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label("Custom Color:", GUILayout.Width(120));
                    _itemData.equipAppearance.UseCustomColor = GUILayout.Toggle(_itemData.equipAppearance.UseCustomColor, "", GUILayout.Width(30));
                    GUILayout.EndHorizontal();




                    if (_itemData.equipAppearance.UseCustomColor)
                    {
                        Color _color1 = _itemData.equipAppearance.CustomColor1;
                        Color _color2 = _itemData.equipAppearance.CustomColor2;
                        Color _color3 = _itemData.equipAppearance.CustomColor3;
                        if (!_useCustomColor)
                        {
                            MasterCharacterCreator.OutfitColorSetting _colorSetting = CharacterCustomizationObject.instance.GetOutfitColorSetting(
                                MasterCharacterCreator.Sex.Male, _itemData.equipAppearance.Type, _itemData.equipAppearance.MaleMeshId);
                            _itemData.equipAppearance.CustomColor1 = _colorSetting.DefaultColor1;
                            _itemData.equipAppearance.CustomColor2 = _colorSetting.DefaultColor2;
                            _itemData.equipAppearance.CustomColor3 = _colorSetting.DefaultColor3;
                        }
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUILayout.Label("#1", GUILayout.Width(20));
                        _itemData.equipAppearance.CustomColor1 = EditorGUILayout.ColorField(_itemData.equipAppearance.CustomColor1, GUILayout.Width(50));
                        GUILayout.Space(10);
                        GUILayout.Label("#2", GUILayout.Width(20));
                        _itemData.equipAppearance.CustomColor2 = EditorGUILayout.ColorField(_itemData.equipAppearance.CustomColor2, GUILayout.Width(50));
                        GUILayout.Space(10);
                        GUILayout.Label("#3", GUILayout.Width(20));
                        _itemData.equipAppearance.CustomColor3 = EditorGUILayout.ColorField(_itemData.equipAppearance.CustomColor3, GUILayout.Width(50));
                        GUILayout.EndHorizontal();

                        if (MeshPreview.renderer != null
                            && (_color1 != _itemData.equipAppearance.CustomColor1
                            || _color2 != _itemData.equipAppearance.CustomColor2
                            || _color3 != _itemData.equipAppearance.CustomColor3))
                        {
                            MeshPreview.renderer.sharedMaterial.SetColor("_Color1", _itemData.equipAppearance.CustomColor1);
                            MeshPreview.renderer.sharedMaterial.SetColor("_Color2", _itemData.equipAppearance.CustomColor2);
                            MeshPreview.renderer.sharedMaterial.SetColor("_Color3", _itemData.equipAppearance.CustomColor3);
                        }
                    }


                }
            }

#else
            EditorUtils.Warnning("[Master Character Creator] is not installed.", 30);
#endif

            EditorGUILayout.Separator();

            GUILayout.EndVertical();
            GUILayout.Space(25);
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            return _valueChanged || GUI.changed;
        }

        public static Texture2D GetIcon(ItemEditorAssets _editorAsset, int _id)
        {
            if (_editorAsset != null && ItemObject.instance != null)
            {
                if (_id >= 0)
                {
                    Item _item = ItemObject.instance.GetItem(_id);

                    if (_item != null)
                    {
                        Texture2D _icon = null;
                        if (_item.iconLoadMethod == LoadMethod.DirectReference)
                        {
                            _icon = _item.icon;
                        }
                        else
                        {
                            _icon = (Texture2D)_editorAsset.GetIcon(_id);
                        }
                        if (_icon == null)
                        {
                            _icon = _item.icon;
                        }
                        return _icon;
                    }
                }
            }
            return null;
        }
        public static void ItemIcon(ItemEditorAssets _editorAsset, int _itemId)
        {
            Texture _icon = GetIcon(_editorAsset, _itemId);
            if (_icon != null)
            {
                GUILayout.Box(_editorAsset.GetIcon(_itemId), EditorUtils._toolButtonStyle, GUILayout.Width(17), GUILayout.Height(17));
            }
            else
            {
                GUILayout.Box("-", GUILayout.Width(17), GUILayout.Height(17));
            }
        }
    }
}
