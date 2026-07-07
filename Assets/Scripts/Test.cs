using SoftKitty;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{

    void Update()
    {
         if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
        var _loot = ItemObject.DropLootPack(Vector3.zero, "LootPack1");//Drop a loot pack.
        _loot.OpenPack();
            
        }
    }
}

