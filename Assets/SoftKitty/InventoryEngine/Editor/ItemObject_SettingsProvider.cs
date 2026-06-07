using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SoftKitty
{
    public class ItemObject_SettingsProvider : AssetSettingsProvider
    {
        private string searchContext;
        private VisualElement rootElement;
        public static ItemObject CurrentSettings
        {
            get
            {
                return SGD_SettingsProvider.CurrentSettings.GetData<ItemObject>();
            }
        }

        public ItemObject_SettingsProvider()
       : base("Project/SoftKitty/SubData - Items", () => CurrentSettings)
        {
            keywords = GetSearchKeywordsFromGUIContentProperties<ItemObject>();
        }



        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            this.rootElement = rootElement;
            this.searchContext = searchContext;
            base.OnActivate(searchContext, rootElement);
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            if (CurrentSettings == null)
            {
                DisplaySettingsCreationGUI();
            }
            else
            {
                base.OnGUI(searchContext);
            }
        }



        private void RefreshEditor()
        {
            base.OnActivate(searchContext, rootElement);
        }

        private void AssignObject(DataObject _obj)
        {
            bool _assigned = false;
            for (int i = 0; i < SGD_SettingsProvider.CurrentSettings.DataObjects.Count; i++)
            {
                if (SGD_SettingsProvider.CurrentSettings.DataObjects[i] == null)
                {
                    SGD_SettingsProvider.CurrentSettings.DataObjects[i] = _obj;
                    _assigned = true;
                }
            }
            if (!_assigned) SGD_SettingsProvider.CurrentSettings.DataObjects.Add(_obj);
            EditorUtility.SetDirty(SGD_SettingsProvider.CurrentSettings);
            AssetDatabase.SaveAssets();
        }

        private void DisplaySettingsCreationGUI()
        {
            const string message = "You have no <color=#22FFAA>Item Data Object</color> Assigned. Please assign one in <color=#22AAFF>General Data Settings</color>.\n" +
                "You can create one by right click in a folder in the project panel, select in the context menu:\n" +
                "'<color=#22AAFF>Create > SoftKitty > Data Objects > Item Data</color>'\n" +
                "Then assign the create data object in:\n" +
                "'<color=#22AAFF>ProjectSettings > SoftKitty > General Data Settings > Data > Add</color>'";
            EditorUtils.HelpInfo(message, 0);

            EditorUtils.BoxInfo("  Item Object in Project", 0);
            var filter = $"t:{typeof(ItemObject).FullName}";
            var guids = AssetDatabase.FindAssets(filter);
            foreach (var _guid in guids)
            {
                var objpath = AssetDatabase.GUIDToAssetPath(_guid);
                var obj = AssetDatabase.LoadAssetAtPath<DataObject>(objpath);
                if (obj != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Box(EditorUtils.GetTexture(EditorIcon.DataIcon), EditorUtils._toolButtonStyle, GUILayout.Width(15), GUILayout.Height(15));
                    GUILayout.Box(objpath, EditorUtils._boxLeftStyle);
                    GUI.backgroundColor = EditorUtils._red;
                    if (GUILayout.Button(new GUIContent("Assign", "Automatically assign this data object."), GUILayout.Width(70)))
                    {
                        AssignObject(obj);
                    }
                    EditorUtils.ResetColor();
                    EditorUtils.End(20);
                }
            }

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = EditorUtils._titleColor;
            if (GUILayout.Button("Auto Create & Assign"))
            {
                if (!AssetDatabase.IsValidFolder(EditorUtils.DataPath))
                {
                    AssetDatabase.CreateFolder(EditorUtils.DataPath.Replace("Data/", ""), "Data");
                }
                ItemObject _newObj = (ItemObject)ScriptableObject.CreateInstance(typeof(ItemObject)); ;
                string _basepath = EditorUtils.DataPath + "ItemObject";
                string _path = _basepath + ".asset";
                for (int i = 1; i < 10; i++)
                {
                    if (AssetDatabase.AssetPathExists(_path))
                    {
                        _path = _basepath + "_" + i.ToString() + ".asset";
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(_newObj, _path);
                        AssetDatabase.Refresh();
                        break;
                    }
                }
                AssignObject(_newObj);
                EditorUtility.DisplayDialog("DataObject", "DataObject created in :" + _path, "OK");
            }
            EditorUtils.ResetColor();
            GUILayout.EndHorizontal();
        }


        [SettingsProvider]
        private static SettingsProvider CreateProjectSettingsMenu() => new ItemObject_SettingsProvider();
    }
}
