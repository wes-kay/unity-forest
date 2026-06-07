using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SoftKitty.InventoryEngine
{
    [CustomEditor(typeof(LootPack))]
    public class LootPack_inspector : UnityEditor.Editor
    {
 
        public override void OnInspectorGUI()
        {

            var script = MonoScript.FromScriptableObject(this);
            LootPack myTarget  = (LootPack)target;

            string _thePath = AssetDatabase.GetAssetPath(script);
            _thePath = _thePath.Replace("LootPack_inspector.cs", "");
            Texture logoIcon = (Texture)AssetDatabase.LoadAssetAtPath(_thePath + "Logo.png", typeof(Texture));
           
            GUILayout.BeginHorizontal();
            GUILayout.Box(logoIcon);
            GUILayout.EndHorizontal();

            EditorUtils.HelpInfo("Please setup Loot Packs in <color=#22AAFF>Project Settings/SoftKitty/SubData - Items</color>.\n" +
                "You can assign loot packs to < color =#22AAFF>Entity</color> in <color=#22AAFF>EntityManager</color>, then call <color=#22FFAA>_entity.DropLootPack()</color>, or you can simply call <color=#22FFAA>GameManage.DropLootPack(Vector3 _pos, string _uid)</color> to drop a loot pack."
               , 0);
            EditorGUILayout.Separator();
            EditorUtils.Document("master-inventory-engine/item-class/loot-pack");
        }
    }
}
