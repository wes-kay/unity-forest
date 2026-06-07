using UnityEngine;
using System.Collections.Generic;

namespace SoftKitty.InventoryEngine
{
    [System.Serializable]
    public class SettingsExport 
    {
        public List<StringColorData> itemTypes = new List<StringColorData>();
        public List<StringColorData> itemQuality = new List<StringColorData>();
        public List<ClickSetting> clickSettings = new List<ClickSetting>();
        public List<Attribute> itemAttributes = new List<Attribute>();
        public List<string> AttributeKeys = new List<string>();
        public string itemAttributeObjectHash = "";
        public List<Enchantment> itemEnchantments = new List<Enchantment>();
        public List<Currency> currencies = new List<Currency>();
        public List<string> currencyIcons = new List<string>();
        public List<Item> items = new List<Item>();
        public List<string> itemIcons = new List<string>();
        public string NameAttributeKey = "name";
        public string LevelAttributeKey = "lvl";
        public string XpAttributeKey = "xp";
        public string MaxXpAttributeKey = "mxp";
        public string CoolDownAttributeKey = "cd";
        public float SharedGlobalCoolDown = 0.5F;
        public Color AttributeNameColor = new Color(0.17F, 0.53F, 0.82F, 1F);
        public bool UseQualityColorForItemName = true;
        public int MerchantStyle = 0;
        public bool HighlightEquipmentSlotWhenHoverItem = true;
        public bool AllowDropItem = true;
        public string CanvasTag = "";
        public bool EnableCrafting = true;
        public int CraftingMaterialCategoryID = 0;
        public string CraftingBlueprintTag = "Blueprint";
        public string PlayerName = "Player";
        public float CraftingTime = 0.5F;
        public Vector2[] EnhancingMaterials = new Vector2[2];
        public int EnhancingCurrencyType = 0;
        public int EnhancingCurrencyNeed = 0;
        public bool EnableEnhancing = true;
        public bool DestroyEquipmentWhenFail = false;
        public int DestroyEquipmentWhenFailLevel = 3;
        public int MaxiumEnhancingLevel = 10;
        public AnimationCurve EnhancingSuccessCurve;
        public int EnhancingCategoryID = 0;
        public float EnhancingTime = 0.5F;
        public bool EnableEnhancingGlow = true;
        public AnimationCurve EnhancingGlowCurve;
        public Vector2 EnchantingMaterial = new Vector2(1, 1);
        public int EnchantingCurrencyType = 0;
        public int EnchantingCurrencyNeed = 0;
        public Vector2 EnchantmentNumberRange = new Vector2(1, 3);
        public bool EnableEnchanting = true;
        public bool RandomEnchantmentsForNewItem = false;
        public int EnchantingSuccessRate = 30;
        public int EnchantingCategoryID = 0;
        public float EnchantingTime = 0.5F;
        public Color EnchantingPrefixesColor = new Color(0.32F, 0.55F, 0.18F, 1F);
        public Color EnchantingSuffxesColor = new Color(0.32F, 0.55F, 0.18F, 1F);
        public Color EnchantingNameColor = new Color(0.32F, 0.55F, 0.18F, 1F);
        public bool EnableSocketing = true;
        public string SocketingTagFilter = "";
        public int SocketingCategoryFilter = 0;
        public int SocketedCategoryFilter = 0;
        public bool EnableRemoveSocketing = true;
        public int RemoveSocketingPrice = 100;
        public int RemoveSocketingCurrency = 0;
        public bool DestroySocketItemWhenRemove = false;
        public bool RandomSocketingSlotsNummber = true;
        public int MinimalSocketingSlotsNumber = 0;
        public int MaxmiumSocketingSlotsNumber = 3;
        public bool LockSocketingSlotsByDefault = true;
        public int RandomChanceToLockSocketingSlots = 25;
        public int UnlockSocketingSlotsPrice = 50;
        public int UnlockSocketingSlotsCurrency = 0;
        public string msgBagFull = "Sorry, the bag is full.";
        public string msgItemUseRestricted = "Sorry, you can not use this item because of your {name} is less than {value}.";
        public string msgItemAssign = "Sorry, you can not assign this item here.";
        public string msgEnhancingFail = "Failed! [ <color=#FFA209>{name}</color> ] break into pieces.";
    }
}
