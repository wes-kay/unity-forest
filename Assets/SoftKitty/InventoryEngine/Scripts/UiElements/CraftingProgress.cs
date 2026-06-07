using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SoftKitty.InventoryEngine
{
    public class CraftingProgress : MonoBehaviour
    {
        [Header("Input the UID of the entity to monitor")]
        public string entityUID;

        #region Variables
        [Header("References")]
        public RectTransform Progress;
        public Text NumberText;
        public ItemIcon ResultItem;
        public AdvAnimation ResultPop;
        public CanvasGroup Root;
        private float FadeTime = 0F;
        private Entity mEntity
        {
            get
            {
                return GameManager.GetEntity(entityUID);
            }
        }
        #endregion



#region Internal Method

        void Update()
        {
#if MASTER_INVENTORY_ENGINE
            if (mEntity == null || mEntity.GetModule<InventoryModule>()==null || mEntity.GetModule<InventoryModule>().GetInventory()==null) return;

            if (mEntity.GetModule<InventoryModule>().GetInventory().isCrafting)
            {
                if (FadeTime < 1F)
                {
                    FadeTime = 1F;
                    Root.alpha = FadeTime;
                    Root.gameObject.SetActive(true);
                    if(ItemObject.instance.TryGetItem(mEntity.GetModule<InventoryModule>().GetInventory().CraftingItemId)!=null) ResultItem.SetAppearance(ItemObject.instance.TryGetItem(mEntity.GetModule<InventoryModule>().GetInventory().CraftingItemId), true, false);
                    ResultItem.SetItemId(mEntity.GetModule<InventoryModule>().GetInventory().CraftingItemId);
                }
                Progress.localScale = new Vector3(mEntity.GetModule<InventoryModule>().GetInventory().CraftingProgress,1F,1F);
                if (mEntity.GetModule<InventoryModule>().GetInventory().CraftingProgress >= 1F)
                {
                    if (mEntity.GetModule<InventoryModule>().GetInventory().CraftingFailed)
                    {
                        NumberText.text = "Failed!";
                    }
                    else
                    {
                        ResultPop.Stop();
                        ResultPop.Play();
                    }
                }
                else
                {
                    NumberText.text = mEntity.GetModule<InventoryModule>().GetInventory().CraftedItemNumber.ToString() + "/" + mEntity.GetModule<InventoryModule>().GetInventory().CraftingItemNumber.ToString();
                }

            }
            else
            {
                FadeTime = Mathf.MoveTowards(FadeTime,0F,Time.unscaledDeltaTime);
                if (FadeTime > 0F)
                {
                    NumberText.text = "Done!";
                    Root.alpha = FadeTime;
                }
                else if (Root.gameObject.activeSelf)
                {
                    Root.gameObject.SetActive(false);
                }
            }
#endif
        }
#endregion

    }
}
