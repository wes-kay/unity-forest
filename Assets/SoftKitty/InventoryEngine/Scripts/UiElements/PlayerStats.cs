using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.InventoryEngine
{
    public class PlayerStats : MonoBehaviour
    {
        private IEnumerator Start()
        {
#if MASTER_INVENTORY_ENGINE
            while (ItemObject.PlayerEquipmentData == null) yield return 1;
#endif
            yield return new WaitForSecondsRealtime(0.5F);
            GetComponentInChildren<StatsUi>(true).Init("player");

    }
  }
}
