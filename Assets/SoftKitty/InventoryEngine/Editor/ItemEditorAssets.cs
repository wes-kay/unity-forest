using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.InventoryEngine
{
    [System.Serializable]
    public class EditorAssetData
    {
        public int uid;
        public Texture2D icon;
        public EditorAssetData Copy()
        {
            EditorAssetData _copy = new EditorAssetData();
            _copy.uid = uid;
            _copy.icon = icon;
            return _copy;
        }
    }
    
    [System.Serializable]
    public class ItemEditorAssets : ScriptableObject
    {
        public List<EditorAssetData> ItemAssets = new List<EditorAssetData>();

        public Texture GetIcon(int _id)
        {
            foreach (var obj in ItemAssets) {
                if (obj.uid == _id) return obj.icon;
            }
            return null;
        }

        public void SetIcon(int _id, Texture2D _icon)
        {
            foreach (var obj in ItemAssets)
            {
                if (obj.uid == _id) obj.icon= _icon;
            }
        }

    }
}
