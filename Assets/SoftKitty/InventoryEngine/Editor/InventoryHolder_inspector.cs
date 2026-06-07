using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SoftKitty.InventoryEngine
{
    [CustomEditor(typeof(InventoryHolder))]
    public class InventoryHolder_inspector : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            InventoryHolder myTarget = (InventoryHolder)target;
            EditorUtils.Warnning("The <color=#12FF33><InventoryHolder></color> component is deprecated. Inventory data is now managed in:\n" +
                "<color=#1288FF>Project Settings ¡ú SoftKitty ¡ú Entity Manager</color>\n" +
                "Click the button below to <color=#12FF33>copy the existing data</color> into the <color=#1288FF>Entity Manager</color>.\n" +
                "Note: <color=#12FF33>Base stats</color> will not be copied because the data structure has changed. You can use the <color=#1288FF>Batch Managing tools</color> in the <color=#1288FF>Entity Manager</color> to add base stats to all entities at once.", 20);
            if (string.IsNullOrEmpty(myTarget.uid))
            {
                if (myTarget.Type== InventoryHolder.HolderType.PlayerInventory || myTarget.Type == InventoryHolder.HolderType.PlayerEquipment)
                {
                    myTarget.uid = "player";
                }
                else
                {
                    myTarget.uid = myTarget.gameObject.name.Replace(" ", "").Replace("(", "").Replace(")", "").Substring(0, Mathf.Max(myTarget.gameObject.name.Length, 10));
                }
            }
            EditorGUILayout.Separator();
            EditorUtils.Document("master-inventory-engine/getting-started/upgrade", false);
            EditorGUILayout.Separator();

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
                return;
            }
            if (GameManager.EntityManagerData == null)
            {

                EditorUtils.Warnning("Entity Manager Object is not assigned, You can create this object via the context menu in any Project folder:" +
                "<color=6688FF>Create ¡ú SoftKitty ¡ú Data Objects ¡ú Entity Manager Data</color>" +
                "You can assign the created asset to the database in: <color=6688FF>Project Settings ¡ú SoftKitty ¡ú Data Settings ¡ú Data</color>", 10);
                GUILayout.BeginHorizontal();
                GUI.backgroundColor = EditorUtils._buttonColor;
                if (GUILayout.Button("Assign One"))
                {
                    SGD_SettingsProvider.CurrentSettings.data_expand = true;
                    SettingsService.OpenProjectSettings("Project/SoftKitty/Entity Manager");
                }
                EditorUtils.ResetColor();
                GUILayout.EndHorizontal();
                return;
            }

            EditorUtils.TitleBox(new GUIContent(" Original Component data:"), EditorUtils._black);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUI.backgroundColor = EditorUtils._black;
            EditorGUILayout.BeginVertical(EditorUtils._groupStyle);
            EditorGUILayout.Separator();
            EditorUtils.Label(new GUIContent("Name: " +myTarget.Name),10);
            EditorUtils.Label(new GUIContent("Type: " + myTarget.Type), 10);
            EditorUtils.Label(new GUIContent("Inventory: " + myTarget.Stacks.Count), 10);
            EditorUtils.Label(new GUIContent("Hidden Items: " + myTarget.HiddenStacks.Count), 10);
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();

            GUI.backgroundColor = EditorUtils._black;
            EditorGUI.BeginChangeCheck();
            EditorUtils.TextField(ref myTarget.uid, new GUIContent("Entity UID:"), 20, 150, true);
            if (EditorGUI.EndChangeCheck()) {
                foreach (var obj in myTarget.gameObject.GetComponents<InventoryHolder>()) {
                    obj.uid = myTarget.uid;
                }
            }
            EditorUtils.ResetColor();

            EditorUtils.HelpInfo("Please input a string UID for this entity, make sure it is unique across the whole project.", 20);

            if (GameManager.EntityManagerData==null) {
                EditorUtils.Warnning("The <Entity Manager> data is not created yet. please go to:\n" +
               "Project Settings ¡ú SoftKitty ¡ú Entity Manager\n" +
               "to create a data object before transfer your inventory data.", 20);
            } else {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUI.backgroundColor = EditorUtils._titleColor;
                if (GUILayout.Button("Transfer Data to Entity Manager")) {
                    Entity _entity = GameManager.EntityManagerData.GetEntity(myTarget.uid);
                    if (_entity == null) _entity = GameManager.EntityManagerData.NewEntity(myTarget.uid);
                    _entity.uiFold = false;
                    InventoryData _newData = new InventoryData();
                    _newData.Name = myTarget.Name;
                    _newData.EntityUid = _entity.uid;
                    _newData.Type= (InventoryData.HolderType)(int)myTarget.Type;
                    _newData.Stacks = new List<InventoryStack>();
                    for (int i = 0; i < myTarget.Stacks.Count; i++)
                    {
                        var _stack = myTarget.Stacks[i].Copy();
                        List<string> _list = new List<string>(ItemObject.instance.ItemNames);
                        int _id = _list.IndexOf(_stack.Item.name);
                        Item _item = ItemObject.instance.GetItem(_id);
                        _stack.Item.uid = _item.uid;
                        _newData.Stacks.Add(_stack);
                    }
                    _newData.HiddenStacks = new List<InventoryStack>();
                    for (int i = 0; i < myTarget.HiddenStacks.Count; i++)
                    {
                        var _stack = myTarget.HiddenStacks[i].Copy();
                        List<string> _list = new List<string>(ItemObject.instance.ItemNames);
                        int _id = _list.IndexOf(_stack.Item.name);
                        Item _item = ItemObject.instance.GetItem(_id);
                        _stack.Item.uid = _item.uid;
                        _newData.HiddenStacks.Add(_stack);
                    }
                    _newData.Currency = myTarget.Currency.Copy();
                    _newData.InventorySize = myTarget.InventorySize;
                    _newData.MaxiumCarryWeight = myTarget.MaxiumCarryWeight;
                    _newData.SellPriceMultiplier = myTarget.SellPriceMultiplier;
                    _newData.BuyPriceMultiplier = myTarget.BuyPriceMultiplier;
                    _newData.SpecificPriceMultiplier = new List<Vector3>();
                    _newData.SpecificPriceMultiplier.AddRange(myTarget.SpecificPriceMultiplier);
                    _newData.TradeAllItems = myTarget.TradeAllItems;
                    _newData.TradeList = new List<int>();
                    _newData.TradeList.AddRange(myTarget.TradeList);
                    _newData.TradeCategoryList = new List<int>();
                    _newData.TradeCategoryList.AddRange(myTarget.TradeCategoryList);
#if MASTER_INVENTORY_ENGINE
                    InventoryEngine.InventoryModule _inventoryModule = _entity.GetModule<InventoryEngine.InventoryModule>();
                    if (_inventoryModule == null)
                    {
                        _inventoryModule = new InventoryEngine.InventoryModule();
                        _entity.Modules.Add(new EntityModuleWrapper(_inventoryModule));
                    }
                    _inventoryModule.Inventory.Add(_newData);
#endif
                    GameManager.EntityManagerData.GenerateUniqueHash();
                    GameManager.EntityManagerData.IdManager.idToKey.Clear();
                    EditorUtility.SetDirty(GameManager.EntityManagerData);
                    if (!myTarget.gameObject.GetComponent<EntityComponent>())
                    {
                        var _ec=  myTarget.gameObject.AddComponent<EntityComponent>();
                        _ec.uid = myTarget.uid;
                    }
                    EditorUtility.SetDirty(myTarget.gameObject);
                    AssetDatabase.SaveAssets();
                    DestroyImmediate(myTarget);
                }
                GUILayout.Space(20);
                GUILayout.EndHorizontal();
                EditorUtils.ResetColor();
                EditorGUILayout.Separator();
            }
        }
    }
}
