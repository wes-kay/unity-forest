using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SoftKitty.InventoryEngine
{
    [CustomEditor(typeof(ItemScriptableObject))]
    public class ItemScriptableObject_inspector : UnityEditor.Editor
    {
        ItemEditorAssets EditorAsset;
        public override void OnInspectorGUI()
        {
            ItemScriptableObject myTarget = (ItemScriptableObject)target;
            var script = MonoScript.FromScriptableObject(this);
            string _thePath = AssetDatabase.GetAssetPath(script);
            _thePath = _thePath.Replace("ItemScriptableObject_inspector.cs", "");

            if (EditorAsset == null) EditorAsset = (ItemEditorAssets)AssetDatabase.LoadAssetAtPath(_thePath + "ItemEditorAssets.asset", typeof(ItemEditorAssets));

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUI.backgroundColor = Color.gray;
            GUILayout.BeginHorizontal(EditorUtils._groupStyle);
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.white;
            List<GUILayoutOption> _options = new List<GUILayoutOption>();
            GUILayout.Box(EditorUtils.GetTexture(EditorIcon.DataLogo), EditorUtils._logoStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(25);
            GUILayout.EndHorizontal();

            if (myTarget.mItem!=null) {
                string[] _typeOptions = new string[ItemObject.instance.itemTypes.Count];
                for (int i = 0; i < ItemObject.instance.itemTypes.Count; i++) _typeOptions[i] = ItemObject.instance.itemTypes[i].name;

                string[] _qualityOptions = new string[ItemObject.instance.itemQuality.Count];
                for (int i = 0; i < ItemObject.instance.itemQuality.Count; i++) _qualityOptions[i] = ItemObject.instance.itemQuality[i].name;

                string[] _currencyOption = new string[ItemObject.instance.currencies.Count];
                for (int i = 0; i < ItemObject.instance.currencies.Count; i++) _currencyOption[i] = ItemObject.instance.currencies[i].name;
                List<string> _allAtt = new List<string>(GameManager.AttributeData.AttributesUidArray);

                bool _duplicateUid = ItemObject.instance.ItemUidList.Contains(myTarget.mItem.uid);
                ItemObject_Inspector.ItemInspector(myTarget.mItem, EditorAsset, _duplicateUid, ItemObject.instance.ItemNames, _typeOptions, _qualityOptions, _currencyOption, _allAtt);
            }
        }
    }
}
