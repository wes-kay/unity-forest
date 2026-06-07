using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftKitty.InventoryEngine
{
    public class ItemSlotsGrid : MonoBehaviour
    {

        void Awake()
        {
            if (GetComponent<GridLayoutGroup>() && ItemObject.instance != null)
            {
                GetComponent<GridLayoutGroup>().cellSize *= ItemObject.instance.InventorySlotScale;
            }
        }
        
    }
}
