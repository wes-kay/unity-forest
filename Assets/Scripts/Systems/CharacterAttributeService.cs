using System;
using System.Collections.Generic;
using SoftKitty;
using UnityEngine;
using Zenject;

namespace CharacterAttributes
{
    /// <summary>
    /// All attribute UIDs defined in AttributeObject.asset.
    /// Use these enums to avoid string literals when reading/writing attributes.
    /// </summary>
    public enum AttributeKey
    {
        Level,
        Attack,
        Defence,
        Resistance,
        Health,
        Stamina,
        Agility,
        CriticalChance,
        Luck,
        HealthSteal,
        CoolDown,
        Damage,
        DamageDelay,
        SpCost,
        StunChance,
        Name,
        Xp,
        MaxXp,
        CurrentHp,

        // ---- Core stats (from GameAttributeObject.asset) ----
        Strength,
        Endurance,
        Constitution,
        Dexterity,
        Reflexes,
        Perception,
        Memory,

        // ---- Current/Max resource pairs ----
        CurrentVitality,
        MaximumVitality,
        CurrentWillpower,
        MaximumWillpower,
        CurrentFocus,
        MaximumFocus,
        CurrentBlood,
        MaximumBlood,
        CurrentSanity,
        MaximumSanity,
        CurrentMorale,
        MaximumMorale,
        CurrentHunger,
        MaximumHunger,
        CurrentThirst,
        MaximumThirst,
        CurrentFatigue,
        MaximumFatigue,
        CurrentExposure,
        MaximumExposure,
        CurrentWarmth,
        MaximumWarmth,
        CurrentBurden,
        MaximumBurden,

        // ---- Body part health pairs ----
        CurrentHeadHealth,
        MaximumHeadHealth,
        CurrentTorsoHealth,
        MaximumTorsoHealth,
        CurrentLeftArmHealth,
        MaximumLeftArmHealth,
        CurrentRightArmHealth,
        MaximumRightArmHealth,
        CurrentLeftLegHealth,
        MaximumLeftLegHealth,
        CurrentRightLegHealth,
        MaximumRightLegHealth,
        CurrentLeftEarHealth,
        MaximumLeftEarHealth,
        CurrentRightEarHealth,
        MaximumRightEarHealth,
        CurrentForeheadHealth,
        MaximumForeheadHealth,
        CurrentTeethHealth,
        MaximumTeethHealth,
        CurrentGumsHealth,
        MaximumGumsHealth,
        CurrentVenisonHealth,
        MaximumVenisonHealth,
        CurrentCurse,
        MaximumCurse,
        CurrentMadness,
        MaximumMadness,
        CurrentMutation,
        MaximumMutation,
        CurrentPossession,
        MaximumPossession,

        // ---- Resistances ----
        PoisonResistance,
        IceResistance,
        CorrosionResistance,
        SoulResistance,
        FireResistance,
        MagicResistance,
        BlindnessResistance,
        VoidResistance,
        DarknessResistance,
        ColdResistance,
        HeatResistance,
        HeresyResistance,
        Karma,
        StunResistance,
        TorsoResistance,

        // ---- Combat & Weapon skills ----
        FirstAid,
        IceSkills,
        ShieldSkills,
        SorcerySkills,
        CarryingCapacity,
        ArmorRating,
        Evasion,
        BlockChance,
        CriticalDamage,
        Accuracy,
        MovementSpeed,
        Initiative,
        HealingRate,
        Regeneration,
        CarryWeight,
        SwordSkills,
        AxeSkills,
        SpearSkills,
        MaceSkills,
        DaggerSkills,
        ArcherySkills,
        CrossbowSkills,
        ThrowingSkills,
        UnarmedCombat,
        Parrying,
        Dodging,
        Tactics,
        DualWielding,
        Execution,
        Torture,
        Intimidation,

        // ---- Survival & Crafting skills ----
        Fishing,
        Hunting,
        Trapping,
        Skinning,
        Butchery,
        Tracking,
        Foraging,
        Campcraft,
        Navigation,
        Swimming,
        Climbing,
        Survival,
        Scavenging,
        CorpseWork,
        Mining,
        Lumberjacking,
        Harvesting,
        Herbalism,
        BloodlettingHouse,
        BoneWork,
        Reliquary,
        Blacksmithing,
        Armsmithing,
        Tailoring,
        Leatherworking,
        Furworking,
        Alchemy,
        Poisoncraft,
        Tinkerer,
        Runecarving,
        Enchanting,
        Engineering,
        FaithDamage,
        Surgery,
        Anatomy,
        Diagnosis,
        Toxicology,
        Empathy,
        Autopsy,
        Amputation,
        Reading,
        Writing,
        Research,
        Investigation,
        History,
        Law,
        Theology,
        Occultism,
        Demonology,
        Necrology,
        Astronomy,
        Rituals,
        MonsterLore,

        // ---- Social & Magic attributes ----
        Presence,
        Negotiation,
        Leadership,
        Deception,
        Etiquette,
        Intelligence,
        Communication,
        Fanaticism,
        Stealth,
        Pickpocketing,
        Lockpicking,
        Forgery,
        Espionage,
        Sabotage,
        Assassination,
        Smuggling,
        Trading,
        Appraisal,
        Bartending,
        Logistics,
        Faith,
        Prayer,
        Meditation,
        Exorcism,
        Blessing,
        Cursing,
        Sacrifice,
        ArcaneKnowledge,
        FireMagic,
        IceMagic,
        BloodMagic,
        Necromancy,
        Summoning,
        Illusion,
        Divination,
        ShadowMagic,
        SpiritBinding,
        Hexcraft,
        Plaguecraft,
        SoulManipulation,
        DarkInsight,
        Dreamwalking,
        VoidAffinity,
        ChaosAttunement,
        GraveDigging,
        MortuaryWork,
        BurialRites,
        Pilgrimage,
        VigilKeeping,
        HeresyDetection,
        Cannibalism,
        Bloodletting,
        MutationControl,
        Reanimation,
        Obsession,
    }

    /// <summary>
    /// Returns the string UID for an attribute enum value.
    /// Keep this in sync with AttributeObject.asset.
    /// </summary>
    public static class AttributeKeyExtensions
    {
        public static string GetUid(this AttributeKey key)
        {
            switch (key)
            {
                case AttributeKey.Level:          return "lvl";
                case AttributeKey.Attack:         return "atk";
                case AttributeKey.Defence:        return "def";
                case AttributeKey.Resistance:     return "resist";
                case AttributeKey.Health:         return "hp";
                case AttributeKey.Stamina:        return "sp";
                case AttributeKey.Agility:        return "agi";
                case AttributeKey.CriticalChance: return "crit";
                case AttributeKey.Luck:           return "luck";
                case AttributeKey.HealthSteal:    return "steal";
                case AttributeKey.CoolDown:       return "cd";
                case AttributeKey.Damage:         return "dmg";
                case AttributeKey.DamageDelay:    return "delay";
                case AttributeKey.SpCost:         return "spcost";
                case AttributeKey.StunChance:     return "stun";
                case AttributeKey.Name:           return "name";
                case AttributeKey.Xp:             return "xp";
                case AttributeKey.MaxXp:          return "mxp";
                case AttributeKey.CurrentHp:      return "chp";

                // ---- Core stats ----
                case AttributeKey.Strength:       return "strn";
                case AttributeKey.Endurance:      return "endu";
                case AttributeKey.Constitution:   return "cons";
                case AttributeKey.Dexterity:      return "dext";
                case AttributeKey.Reflexes:       return "refl";
                case AttributeKey.Perception:     return "perc";
                case AttributeKey.Memory:         return "memo";

                // ---- Current/Max resource pairs ----
                case AttributeKey.CurrentVitality:   return "cvit";
                case AttributeKey.MaximumVitality:   return "mvit";
                case AttributeKey.CurrentWillpower:  return "cwil";
                case AttributeKey.MaximumWillpower:  return "mwil";
                case AttributeKey.CurrentFocus:      return "cfoc";
                case AttributeKey.MaximumFocus:      return "mfoc";
                case AttributeKey.CurrentBlood:      return "cbld";
                case AttributeKey.MaximumBlood:      return "mbld";
                case AttributeKey.CurrentSanity:     return "csan";
                case AttributeKey.MaximumSanity:     return "msan";
                case AttributeKey.CurrentMorale:     return "cmor";
                case AttributeKey.MaximumMorale:     return "mmor";
                case AttributeKey.CurrentHunger:     return "chng";
                case AttributeKey.MaximumHunger:     return "mhng";
                case AttributeKey.CurrentThirst:     return "cthr";
                case AttributeKey.MaximumThirst:     return "mthr";
                case AttributeKey.CurrentFatigue:    return "cfat";
                case AttributeKey.MaximumFatigue:    return "mfat";
                case AttributeKey.CurrentExposure:   return "cexp";
                case AttributeKey.MaximumExposure:   return "mexp";
                case AttributeKey.CurrentWarmth:     return "cwrk";
                case AttributeKey.MaximumWarmth:     return "mwrk";
                case AttributeKey.CurrentBurden:     return "cbur";
                case AttributeKey.MaximumBurden:     return "mbur";

                // ---- Body part health pairs ----
                case AttributeKey.CurrentHeadHealth:   return "ched";
                case AttributeKey.MaximumHeadHealth:   return "mhed";
                case AttributeKey.CurrentTorsoHealth:  return "ctor";
                case AttributeKey.MaximumTorsoHealth:  return "mtor";
                case AttributeKey.CurrentLeftArmHealth: return "clar";
                case AttributeKey.MaximumLeftArmHealth: return "mlar";
                case AttributeKey.CurrentRightArmHealth: return "crar";
                case AttributeKey.MaximumRightArmHealth: return "mrar";
                case AttributeKey.CurrentLeftLegHealth: return "clrg";
                case AttributeKey.MaximumLeftLegHealth: return "mlrg";
                case AttributeKey.CurrentRightLegHealth: return "cpai";
                case AttributeKey.MaximumRightLegHealth: return "mpai";
                case AttributeKey.CurrentLeftEarHealth: return "cin";
                case AttributeKey.MaximumLeftEarHealth: return "minf";
                case AttributeKey.CurrentRightEarHealth: return "ccor";
                case AttributeKey.MaximumRightEarHealth: return "mcor";
                case AttributeKey.CurrentForeheadHealth: return "cfvr";
                case AttributeKey.MaximumForeheadHealth: return "mfvr";
                case AttributeKey.CurrentTeethHealth:  return "cstr";
                case AttributeKey.MaximumTeethHealth:  return "mstr";
                case AttributeKey.CurrentGumsHealth:   return "cble";
                case AttributeKey.MaximumGumsHealth:   return "mble";
                case AttributeKey.CurrentVenisonHealth: return "cven";
                case AttributeKey.MaximumVenisonHealth: return "mven";
                case AttributeKey.CurrentCurse:        return "ccur";
                case AttributeKey.MaximumCurse:        return "mcur";
                case AttributeKey.CurrentMadness:      return "cmad";
                case AttributeKey.MaximumMadness:      return "mmad";
                case AttributeKey.CurrentMutation:     return "cmut";
                case AttributeKey.MaximumMutation:     return "mmut";
                case AttributeKey.CurrentPossession:   return "cpos";
                case AttributeKey.MaximumPossession:   return "mpos";

                // ---- Resistances ----
                case AttributeKey.PoisonResistance:  return "pres";
                case AttributeKey.IceResistance:     return "ires";
                case AttributeKey.CorrosionResistance: return "corr";
                case AttributeKey.SoulResistance:    return "sres";
                case AttributeKey.FireResistance:    return "fres";
                case AttributeKey.MagicResistance:   return "mres";
                case AttributeKey.BlindnessResistance: return "bres";
                case AttributeKey.VoidResistance:    return "vres";
                case AttributeKey.DarknessResistance: return "dres";
                case AttributeKey.ColdResistance:    return "cold";
                case AttributeKey.HeatResistance:    return "heat";
                case AttributeKey.HeresyResistance:  return "hers";
                case AttributeKey.Karma:             return "kres";
                case AttributeKey.StunResistance:    return "stun";
                case AttributeKey.TorsoResistance:   return "tors";

                // ---- Combat & Weapon skills ----
                case AttributeKey.FirstAid:          return "firs";
                case AttributeKey.IceSkills:         return "ices";
                case AttributeKey.ShieldSkills:      return "shrs";
                case AttributeKey.SorcerySkills:     return "sors";
                case AttributeKey.CarryingCapacity:  return "cars";
                case AttributeKey.ArmorRating:       return "armr";
                case AttributeKey.Evasion:           return "evas";
                case AttributeKey.BlockChance:       return "bloc";
                case AttributeKey.CriticalDamage:    return "crit";
                case AttributeKey.Accuracy:          return "accu";
                case AttributeKey.MovementSpeed:     return "move";
                case AttributeKey.Initiative:        return "init";
                case AttributeKey.HealingRate:       return "heal";
                case AttributeKey.Regeneration:      return "regn";
                case AttributeKey.CarryWeight:       return "carr";
                case AttributeKey.SwordSkills:       return "swrd";
                case AttributeKey.AxeSkills:         return "axem";
                case AttributeKey.SpearSkills:       return "spea";
                case AttributeKey.MaceSkills:        return "mace";
                case AttributeKey.DaggerSkills:      return "dagg";
                case AttributeKey.ArcherySkills:     return "arch";
                case AttributeKey.CrossbowSkills:    return "xbow";
                case AttributeKey.ThrowingSkills:    return "thrw";
                case AttributeKey.UnarmedCombat:     return "unar";
                case AttributeKey.Parrying:          return "parr";
                case AttributeKey.Dodging:           return "dodg";
                case AttributeKey.Tactics:           return "tact";
                case AttributeKey.DualWielding:      return "dual";
                case AttributeKey.Execution:         return "exec";
                case AttributeKey.Torture:           return "tort";
                case AttributeKey.Intimidation:      return "inti";

                // ---- Survival & Crafting skills ----
                case AttributeKey.Fishing:           return "fish";
                case AttributeKey.Hunting:           return "hunt";
                case AttributeKey.Trapping:          return "trap";
                case AttributeKey.Skinning:          return "skin";
                case AttributeKey.Butchery:          return "butc";
                case AttributeKey.Tracking:          return "trac";
                case AttributeKey.Foraging:          return "fora";
                case AttributeKey.Campcraft:         return "camp";
                case AttributeKey.Navigation:        return "navi";
                case AttributeKey.Swimming:          return "swim";
                case AttributeKey.Climbing:          return "clim";
                case AttributeKey.Survival:          return "surv";
                case AttributeKey.Scavenging:        return "scav";
                case AttributeKey.CorpseWork:        return "corp";
                case AttributeKey.Mining:            return "mine";
                case AttributeKey.Lumberjacking:     return "lumb";
                case AttributeKey.Harvesting:        return "harv";
                case AttributeKey.Herbalism:         return "herb";
                case AttributeKey.BloodlettingHouse: return "bldh";
                case AttributeKey.BoneWork:          return "bone";
                case AttributeKey.Reliquary:         return "reli";
                case AttributeKey.Blacksmithing:     return "blks";
                case AttributeKey.Armsmithing:       return "arsm";
                case AttributeKey.Tailoring:         return "tail";
                case AttributeKey.Leatherworking:    return "leat";
                case AttributeKey.Furworking:        return "fles";
                case AttributeKey.Alchemy:           return "alch";
                case AttributeKey.Poisoncraft:       return "pois";
                case AttributeKey.Tinkerer:          return "trmk";
                case AttributeKey.Runecarving:       return "rune";
                case AttributeKey.Enchanting:        return "ench";
                case AttributeKey.Engineering:       return "engi";
                case AttributeKey.FaithDamage:       return "faid";
                case AttributeKey.Surgery:           return "surg";
                case AttributeKey.Anatomy:           return "anat";
                case AttributeKey.Diagnosis:         return "diag";
                case AttributeKey.Toxicology:        return "toxi";
                case AttributeKey.Empathy:           return "emba";
                case AttributeKey.Autopsy:           return "auto";
                case AttributeKey.Amputation:        return "ampu";
                case AttributeKey.Reading:           return "read";
                case AttributeKey.Writing:           return "writ";
                case AttributeKey.Research:          return "rese";
                case AttributeKey.Investigation:     return "inve";
                case AttributeKey.History:           return "hist";
                case AttributeKey.Law:               return "laws";
                case AttributeKey.Theology:          return "theo";
                case AttributeKey.Occultism:         return "occu";
                case AttributeKey.Demonology:        return "demo";
                case AttributeKey.Necrology:         return "necr";
                case AttributeKey.Astronomy:         return "astr";
                case AttributeKey.Rituals:           return "ritu";
                case AttributeKey.MonsterLore:       return "mons";

                // ---- Social & Magic attributes ----
                case AttributeKey.Presence:          return "pers";
                case AttributeKey.Negotiation:       return "nego";
                case AttributeKey.Leadership:        return "lead";
                case AttributeKey.Deception:         return "dece";
                case AttributeKey.Etiquette:         return "etiq";
                case AttributeKey.Intelligence:      return "inte";
                case AttributeKey.Communication:     return "comm";
                case AttributeKey.Fanaticism:        return "fana";
                case AttributeKey.Stealth:           return "stea";
                case AttributeKey.Pickpocketing:     return "pick";
                case AttributeKey.Lockpicking:       return "lock";
                case AttributeKey.Forgery:           return "forg";
                case AttributeKey.Espionage:         return "espi";
                case AttributeKey.Sabotage:          return "saba";
                case AttributeKey.Assassination:     return "assa";
                case AttributeKey.Smuggling:         return "smug";
                case AttributeKey.Trading:           return "trad";
                case AttributeKey.Appraisal:         return "appr";
                case AttributeKey.Bartending:        return "bart";
                case AttributeKey.Logistics:         return "logi";
                case AttributeKey.Faith:             return "fait";
                case AttributeKey.Prayer:            return "pray";
                case AttributeKey.Meditation:        return "medi";
                case AttributeKey.Exorcism:          return "exor";
                case AttributeKey.Blessing:          return "bles";
                case AttributeKey.Cursing:           return "curs";
                case AttributeKey.Sacrifice:         return "sacr";
                case AttributeKey.ArcaneKnowledge:   return "arca";
                case AttributeKey.FireMagic:         return "fire";
                case AttributeKey.IceMagic:          return "icem";
                case AttributeKey.BloodMagic:        return "blod";
                case AttributeKey.Necromancy:        return "necm";
                case AttributeKey.Summoning:         return "summ";
                case AttributeKey.Illusion:          return "illu";
                case AttributeKey.Divination:        return "divi";
                case AttributeKey.ShadowMagic:       return "shad";
                case AttributeKey.SpiritBinding:     return "spir";
                case AttributeKey.Hexcraft:          return "hexc";
                case AttributeKey.Plaguecraft:       return "plag";
                case AttributeKey.SoulManipulation:  return "soul";
                case AttributeKey.DarkInsight:       return "dark";
                case AttributeKey.Dreamwalking:      return "drea";
                case AttributeKey.VoidAffinity:      return "void";
                case AttributeKey.ChaosAttunement:   return "chao";
                case AttributeKey.GraveDigging:      return "grav";
                case AttributeKey.MortuaryWork:      return "mort";
                case AttributeKey.BurialRites:       return "buri";
                case AttributeKey.Pilgrimage:        return "pilg";
                case AttributeKey.VigilKeeping:      return "vigi";
                case AttributeKey.HeresyDetection:   return "here";
                case AttributeKey.Cannibalism:       return "cann";
                case AttributeKey.Bloodletting:      return "blst";
                case AttributeKey.MutationControl:   return "muta";
                case AttributeKey.Reanimation:       return "rean";
                case AttributeKey.Obsession:         return "obse";

                default:                          return "";
            }
        }
    }

    /// <summary>
    /// Service for reading and formatting character attributes from an Entity.
    /// Dependency-injected via Zenject — not a MonoBehaviour.
    /// </summary>
    public class CharacterAttributeService
    {
        [Inject]
        public void Construct()
        {
        }

        // =========================================================================
        // Generic helpers — work with any AttributeKey.
        // =========================================================================

        /// <summary>
        /// Get the current float value of an attribute for the given entity.
        /// Includes module bonuses and temporary values.
        /// </summary>
        public float GetValue(Entity entity, AttributeKey key)
        {
            return GetValue(entity, key.GetUid());
        }

        /// <summary>
        /// Get the current float value of an attribute by UID.
        /// Includes module bonuses and temporary values.
        /// </summary>
        public float GetValue(Entity entity, string uid)
        {
            return entity.GetAttributeFloat(uid);
        }

        /// <summary>
        /// Get the current int value of an attribute for the given entity.
        /// Includes module bonuses and temporary values.
        /// </summary>
        public int GetInt(Entity entity, AttributeKey key)
        {
            return GetInt(entity, key.GetUid());
        }

        /// <summary>
        /// Get the current int value of an attribute by UID.
        /// Includes module bonuses and temporary values.
        /// </summary>
        public int GetInt(Entity entity, string uid)
        {
            return entity.GetAttributeInt(uid);
        }

        /// <summary>
        /// Get the raw base float value of an attribute (excludes modules and temporaries).
        /// </summary>
        public float GetBaseValue(Entity entity, AttributeKey key)
        {
            return GetBaseValue(entity, key.GetUid());
        }

        /// <summary>
        /// Get the raw base float value of an attribute by UID (excludes modules and temporaries).
        /// </summary>
        public float GetBaseValue(Entity entity, string uid)
        {
            return entity.GetAttributeFloat(uid, false);
        }

        /// <summary>
        /// Get the display name for an attribute (from the AttributeObject definition).
        /// </summary>
        public string GetDisplayName(AttributeKey key)
        {
            return GetDisplayName(key.GetUid());
        }

        /// <summary>
        /// Get the display name for an attribute UID (from the AttributeObject definition).
        /// </summary>
        public string GetDisplayName(string uid)
        {
            return AttributeObject.instance.GetAttribute(uid).name;
        }

        /// <summary>
        /// Get the suffix string for an attribute (from the AttributeObject definition).
        /// </summary>
        public string GetSuffix(AttributeKey key)
        {
            return GetSuffix(key.GetUid());
        }

        /// <summary>
        /// Get the suffix string for an attribute UID (from the AttributeObject definition).
        /// </summary>
        public string GetSuffix(string uid)
        {
            return AttributeObject.instance.GetAttribute(uid).suffixes;
        }

        /// <summary>
        /// Check whether an attribute is numeric.
        /// </summary>
        public bool IsNumeric(AttributeKey key)
        {
            return IsNumeric(key.GetUid());
        }

        /// <summary>
        /// Check whether an attribute UID is numeric.
        /// </summary>
        public bool IsNumeric(string uid)
        {
            return !AttributeObject.instance.GetAttribute(uid).stringValue;
        }

        /// <summary>
        /// Check whether an attribute is a string value (e.g. character name).
        /// </summary>
        public bool IsString(AttributeKey key)
        {
            return IsString(key.GetUid());
        }

        /// <summary>
        /// Check whether an attribute UID is a string value.
        /// </summary>
        public bool IsString(string uid)
        {
            return AttributeObject.instance.GetAttribute(uid).stringValue;
        }

        /// <summary>
        /// Get the formatted display string for an attribute value.
        /// Combines the display name, value, and suffix in a readable format.
        /// </summary>
        public string GetDisplayString(Entity entity, AttributeKey key)
        {
            return GetDisplayString(entity, key.GetUid());
        }

        /// <summary>
        /// Get the formatted display string for an attribute value.
        /// Combines the display name, value, and suffix in a readable format.
        /// </summary>
        public string GetDisplayString(Entity entity, string uid)
        {
            var attr = AttributeObject.instance.GetAttribute(uid);
            float value = entity.GetAttributeFloat(uid);
            return attr.name + " : " + value + " " + attr.suffixes;
        }

        /// <summary>
        /// Get the formatted display string for a string-type attribute.
        /// </summary>
        public string GetDisplayString(Entity entity, AttributeKey key, string value)
        {
            return GetDisplayString(entity, key.GetUid(), value);
        }

        /// <summary>
        /// Get the formatted display string for a string-type attribute.
        /// </summary>
        public string GetDisplayString(Entity entity, string uid, string value)
        {
            var attr = AttributeObject.instance.GetAttribute(uid);
            return attr.name + " : " + value;
        }

        /// <summary>
        /// Add a value to an attribute and return the new total.
        /// </summary>
        public float AddValue(Entity entity, AttributeKey key, float amount)
        {
            return AddValue(entity, key.GetUid(), amount);
        }

        /// <summary>
        /// Add a value to an attribute by UID and return the new total.
        /// </summary>
        public float AddValue(Entity entity, string uid, float amount)
        {
            return entity.AddAttributeValue(uid, amount);
        }

        /// <summary>
        /// Add a clamped value to an attribute and return the new total.
        /// </summary>
        public float AddValueClamp(Entity entity, AttributeKey key, float amount, float min, float max)
        {
            return AddValueClamp(entity, key.GetUid(), amount, min, max);
        }

        /// <summary>
        /// Add a clamped value to an attribute by UID and return the new total.
        /// </summary>
        public float AddValueClamp(Entity entity, string uid, float amount, float min, float max)
        {
            return entity.AddAttributeValueClamp(uid, amount, min, max);
        }

        /// <summary>
        /// Set the absolute value of an attribute.
        /// </summary>
        public void SetAttributeValue(Entity entity, AttributeKey key, float value)
        {
            SetAttributeValue(entity, key.GetUid(), value);
        }

        /// <summary>
        /// Set the absolute value of an attribute by UID.
        /// </summary>
        public void SetAttributeValue(Entity entity, string uid, float value)
        {
            entity.SetAttributeValue(uid, value);
        }

        /// <summary>
        /// Get all attributes currently defined on the entity with their values.
        /// </summary>
        public List<AttributeEntry> GetAllAttributes(Entity entity)
        {
            var result = new List<AttributeEntry>();
            foreach (var data in entity.Attributes)
            {
                var attr = AttributeObject.instance.GetAttribute(data.uid);
                if (attr == null) continue;

                // Skip locked non-fixed attributes
                if (data.locked && !data.isFixed) continue;

                float value = entity.GetAttributeFloat(data.uid);
                string displayValue = data.isNumber() ? value.ToString("F1") : data.GetString();

                result.Add(new AttributeEntry
                {
                    uid = data.uid,
                    name = attr.name,
                    value = value,
                    displayValue = displayValue,
                    suffix = attr.suffixes,
                    isNumeric = data.isNumber(),
                    visibleInStats = attr.visibleInStatsPanel,
                    visible = attr.visible
                });
            }
            return result;
        }

        /// <summary>
        /// Check if the entity has a specific attribute.
        /// </summary>
        public bool HasAttribute(Entity entity, AttributeKey key)
        {
            return HasAttribute(entity, key.GetUid());
        }

        /// <summary>
        /// Check if the entity has a specific attribute by UID.
        /// </summary>
        public bool HasAttribute(Entity entity, string uid)
        {
            foreach (var data in entity.Attributes)
            {
                if (data.uid == uid) return true;
            }
            return false;
        }

        /// <summary>
        /// Get the upgrade/level increment for an attribute.
        /// </summary>
        public float GetUpgradeIncrement(AttributeKey key)
        {
            return GetUpgradeIncrement(key.GetUid());
        }

        /// <summary>
        /// Get the upgrade/level increment for an attribute UID.
        /// </summary>
        public float GetUpgradeIncrement(string uid)
        {
            return AttributeObject.instance.GetAttribute(uid).upgradeIncrement;
        }

        /// <summary>
        /// Get the current upgrade level of the entity's attributes.
        /// </summary>
        public int GetUpgradeLevel(Entity entity)
        {
            return entity.AttributesUpgradeLevel;
        }

        /// <summary>
        /// Set the upgrade level of the entity's attributes.
        /// </summary>
        public void SetUpgradeLevel(Entity entity, int level)
        {
            entity.AttributesUpgradeLevel = level;
        }

        // =========================================================================
        // Internal helpers that accept AttributeKey and convert to string internally.
        // These exist because C# won't implicitly convert enum to int/string for
        // overload resolution — Entity.GetAttributeFloat has both int and string
        // overloads, so passing an enum directly causes a compile error.
        // =========================================================================

        public float GetFloat(Entity entity, AttributeKey key, bool includeTemp = true)
        {
            string uid = key.GetUid();
            return entity.GetAttributeFloat((string)uid, includeTemp);
        }

        /// <summary>
        /// Get an attribute float value by its string UID.
        /// Used by systems that work with attribute names as strings (e.g. QuestService).
        /// </summary>
        public float GetFloat(Entity entity, string uid, bool includeTemp = true)
        {
            return entity.GetAttributeFloat(uid, includeTemp);
        }

        private int GetInt(Entity entity, AttributeKey key, bool includeTemp = true)
        {
            string uid = key.GetUid();
            return entity.GetAttributeInt((string)uid, includeTemp);
        }

        private void SetFloat(Entity entity, AttributeKey key, float value)
        {
            string uid = key.GetUid();
            entity.SetAttributeValue((string)uid, value);
        }

        private float AddFloat(Entity entity, AttributeKey key, float amount)
        {
            string uid = key.GetUid();
            return entity.AddAttributeValue((string)uid, amount);
        }

        private float AddFloatClamp(Entity entity, AttributeKey key, float amount, float min, float max)
        {
            string uid = key.GetUid();
            return entity.AddAttributeValueClamp((string)uid, amount, min, max);
        }

        // =========================================================================
        // Convenience helpers — per-attribute getters/setters/adders for every UID.
        // =========================================================================

        // ---- Level (lvl) ----

        /// <summary>Get current level (includes bonuses).</summary>
        public int GetLevel(Entity entity)
        {
            return GetInt(entity, AttributeKey.Level);
        }

        /// <summary>Get base level (excludes bonuses).</summary>
        public int GetBaseLevel(Entity entity)
        {
            return (int)GetFloat(entity, AttributeKey.Level, false);
        }

        /// <summary>Set absolute level value.</summary>
        public void SetLevel(Entity entity, int value)
        {
            SetFloat(entity, AttributeKey.Level, value);
        }

        /// <summary>Add to level (no clamp).</summary>
        public int AddLevel(Entity entity, int amount)
        {
            return (int)AddFloat(entity, AttributeKey.Level, amount);
        }

        // ---- Attack (atk) ----

        /// <summary>Get current attack (includes bonuses).</summary>
        public float GetAttack(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Attack);
        }

        /// <summary>Get base attack (excludes bonuses).</summary>
        public float GetBaseAttack(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Attack, false);
        }

        /// <summary>Set absolute attack value.</summary>
        public void SetAttack(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Attack, value);
        }

        /// <summary>Add to attack (no clamp).</summary>
        public float AddAttack(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Attack, amount);
        }

        /// <summary>Add to attack with clamp.</summary>
        public float AddAttackClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Attack, amount, min, max);
        }

        // ---- Defence (def) ----

        /// <summary>Get current defence (includes bonuses).</summary>
        public float GetDefense(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Defence);
        }

        /// <summary>Get base defence (excludes bonuses).</summary>
        public float GetBaseDefense(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Defence, false);
        }

        /// <summary>Set absolute defence value.</summary>
        public void SetDefense(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Defence, value);
        }

        /// <summary>Add to defence (no clamp).</summary>
        public float AddDefense(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Defence, amount);
        }

        /// <summary>Add to defence with clamp.</summary>
        public float AddDefenseClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Defence, amount, min, max);
        }

        // ---- Resistance (resist) ----

        /// <summary>Get current resistance (includes bonuses).</summary>
        public float GetResistance(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Resistance);
        }

        /// <summary>Get base resistance (excludes bonuses).</summary>
        public float GetBaseResistance(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Resistance, false);
        }

        /// <summary>Set absolute resistance value.</summary>
        public void SetResistance(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Resistance, value);
        }

        /// <summary>Add to resistance (no clamp).</summary>
        public float AddResistance(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Resistance, amount);
        }

        /// <summary>Add to resistance with clamp.</summary>
        public float AddResistanceClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Resistance, amount, min, max);
        }

        // ---- Health (hp) — base max HP ----

        /// <summary>Get base max HP (excludes bonuses).</summary>
        public float GetMaxHp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Health, false);
        }

        /// <summary>Get raw base max HP (excludes modules and temporaries).</summary>
        public float GetBaseMaxHp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Health, false);
        }

        /// <summary>Set absolute max HP value.</summary>
        public void SetMaxHp(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Health, value);
        }

        /// <summary>Add to max HP (no clamp).</summary>
        public float AddMaxHp(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Health, amount);
        }

        /// <summary>Add to max HP with clamp.</summary>
        public float AddMaxHpClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Health, amount, min, max);
        }

        // ---- Stamina (sp) ----

        /// <summary>Get current stamina (includes bonuses).</summary>
        public float GetSp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Stamina);
        }

        /// <summary>Get base stamina (excludes bonuses).</summary>
        public float GetBaseSp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Stamina, false);
        }

        /// <summary>Set absolute stamina value.</summary>
        public void SetSp(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Stamina, value);
        }

        /// <summary>Add to stamina (no clamp).</summary>
        public float AddSp(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Stamina, amount);
        }

        /// <summary>Add to stamina with clamp.</summary>
        public float AddSpClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Stamina, amount, min, max);
        }

        // ---- Agility (agi) ----

        /// <summary>Get current agility (includes bonuses).</summary>
        public float GetAgility(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Agility);
        }

        /// <summary>Get base agility (excludes bonuses).</summary>
        public float GetBaseAgility(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Agility, false);
        }

        /// <summary>Set absolute agility value.</summary>
        public void SetAgility(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Agility, value);
        }

        /// <summary>Add to agility (no clamp).</summary>
        public float AddAgility(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Agility, amount);
        }

        /// <summary>Add to agility with clamp.</summary>
        public float AddAgilityClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Agility, amount, min, max);
        }

        // ---- Critical Chance (crit) ----

        /// <summary>Get current critical chance (includes bonuses).</summary>
        public float GetCriticalChance(Entity entity)
        {
            return GetFloat(entity, AttributeKey.CriticalChance);
        }

        /// <summary>Get base critical chance (excludes bonuses).</summary>
        public float GetBaseCriticalChance(Entity entity)
        {
            return GetFloat(entity, AttributeKey.CriticalChance, false);
        }

        /// <summary>Set absolute critical chance value.</summary>
        public void SetCriticalChance(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.CriticalChance, value);
        }

        /// <summary>Add to critical chance (no clamp).</summary>
        public float AddCriticalChance(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.CriticalChance, amount);
        }

        /// <summary>Add to critical chance with clamp.</summary>
        public float AddCriticalChanceClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.CriticalChance, amount, min, max);
        }

        // ---- Luck (luck) ----

        /// <summary>Get current luck (includes bonuses).</summary>
        public float GetLuck(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Luck);
        }

        /// <summary>Get base luck (excludes bonuses).</summary>
        public float GetBaseLuck(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Luck, false);
        }

        /// <summary>Set absolute luck value.</summary>
        public void SetLuck(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Luck, value);
        }

        /// <summary>Add to luck (no clamp).</summary>
        public float AddLuck(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Luck, amount);
        }

        /// <summary>Add to luck with clamp.</summary>
        public float AddLuckClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Luck, amount, min, max);
        }

        // ---- Health Steal (steal) ----

        /// <summary>Get current health steal (includes bonuses).</summary>
        public float GetHealthSteal(Entity entity)
        {
            return GetFloat(entity, AttributeKey.HealthSteal);
        }

        /// <summary>Get base health steal (excludes bonuses).</summary>
        public float GetBaseHealthSteal(Entity entity)
        {
            return GetFloat(entity, AttributeKey.HealthSteal, false);
        }

        /// <summary>Set absolute health steal value.</summary>
        public void SetHealthSteal(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.HealthSteal, value);
        }

        /// <summary>Add to health steal (no clamp).</summary>
        public float AddHealthSteal(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.HealthSteal, amount);
        }

        /// <summary>Add to health steal with clamp.</summary>
        public float AddHealthStealClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.HealthSteal, amount, min, max);
        }

        // ---- Cool Down (cd) ----

        /// <summary>Get current cool down (includes bonuses).</summary>
        public float GetCoolDown(Entity entity)
        {
            return GetFloat(entity, AttributeKey.CoolDown);
        }

        /// <summary>Get base cool down (excludes bonuses).</summary>
        public float GetBaseCoolDown(Entity entity)
        {
            return GetFloat(entity, AttributeKey.CoolDown, false);
        }

        /// <summary>Set absolute cool down value.</summary>
        public void SetCoolDown(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.CoolDown, value);
        }

        /// <summary>Add to cool down (no clamp).</summary>
        public float AddCoolDown(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.CoolDown, amount);
        }

        /// <summary>Add to cool down with clamp.</summary>
        public float AddCoolDownClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.CoolDown, amount, min, max);
        }

        // ---- Damage (dmg) ----

        /// <summary>Get current damage (includes bonuses).</summary>
        public float GetDamage(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Damage);
        }

        /// <summary>Get base damage (excludes bonuses).</summary>
        public float GetBaseDamage(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Damage, false);
        }

        /// <summary>Set absolute damage value.</summary>
        public void SetDamage(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Damage, value);
        }

        /// <summary>Add to damage (no clamp).</summary>
        public float AddDamage(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Damage, amount);
        }

        /// <summary>Add to damage with clamp.</summary>
        public float AddDamageClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.Damage, amount, min, max);
        }

        // ---- Damage Delay (delay) ----

        /// <summary>Get current damage delay (includes bonuses).</summary>
        public float GetDamageDelay(Entity entity)
        {
            return GetFloat(entity, AttributeKey.DamageDelay);
        }

        /// <summary>Get base damage delay (excludes bonuses).</summary>
        public float GetBaseDamageDelay(Entity entity)
        {
            return GetFloat(entity, AttributeKey.DamageDelay, false);
        }

        /// <summary>Set absolute damage delay value.</summary>
        public void SetDamageDelay(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.DamageDelay, value);
        }

        /// <summary>Add to damage delay (no clamp).</summary>
        public float AddDamageDelay(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.DamageDelay, amount);
        }

        /// <summary>Add to damage delay with clamp.</summary>
        public float AddDamageDelayClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.DamageDelay, amount, min, max);
        }

        // ---- SP Cost (spcost) ----

        /// <summary>Get current SP cost (includes bonuses).</summary>
        public float GetSpCost(Entity entity)
        {
            return GetFloat(entity, AttributeKey.SpCost);
        }

        /// <summary>Get base SP cost (excludes bonuses).</summary>
        public float GetBaseSpCost(Entity entity)
        {
            return GetFloat(entity, AttributeKey.SpCost, false);
        }

        /// <summary>Set absolute SP cost value.</summary>
        public void SetSpCost(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.SpCost, value);
        }

        /// <summary>Add to SP cost (no clamp).</summary>
        public float AddSpCost(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.SpCost, amount);
        }

        /// <summary>Add to SP cost with clamp.</summary>
        public float AddSpCostClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.SpCost, amount, min, max);
        }

        // ---- Stun Chance (stun) ----

        /// <summary>Get current stun chance (includes bonuses).</summary>
        public float GetStunChance(Entity entity)
        {
            return GetFloat(entity, AttributeKey.StunChance);
        }

        /// <summary>Get base stun chance (excludes bonuses).</summary>
        public float GetBaseStunChance(Entity entity)
        {
            return GetFloat(entity, AttributeKey.StunChance, false);
        }

        /// <summary>Set absolute stun chance value.</summary>
        public void SetStunChance(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.StunChance, value);
        }

        /// <summary>Add to stun chance (no clamp).</summary>
        public float AddStunChance(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.StunChance, amount);
        }

        /// <summary>Add to stun chance with clamp.</summary>
        public float AddStunChanceClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.StunChance, amount, min, max);
        }

        // ---- Name (name) — string attribute ----

        /// <summary>Get character name.</summary>
        public string GetName(Entity entity)
        {
            return entity.GetAttributeString(AttributeKey.Name.GetUid());
        }

        // ---- XP (xp) ----

        /// <summary>Get current XP (includes bonuses).</summary>
        public float GetXp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Xp);
        }

        /// <summary>Get base XP (excludes bonuses).</summary>
        public float GetBaseXp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.Xp, false);
        }

        /// <summary>Set absolute XP value.</summary>
        public void SetXp(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.Xp, value);
        }

        /// <summary>Add to XP (no clamp).</summary>
        public float AddXp(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.Xp, amount);
        }

        // ---- Max XP (mxp) ----

        /// <summary>Get base max XP (excludes bonuses).</summary>
        public float GetMaxXp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.MaxXp, false);
        }

        /// <summary>Get raw base max XP (excludes modules and temporaries).</summary>
        public float GetBaseMaxXp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.MaxXp, false);
        }

        /// <summary>Set absolute max XP value.</summary>
        public void SetMaxXp(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.MaxXp, value);
        }

        /// <summary>Add to max XP (no clamp).</summary>
        public float AddMaxXp(Entity entity, float amount)
        {
            return AddFloat(entity, AttributeKey.MaxXp, amount);
        }

        /// <summary>Add to max XP with clamp.</summary>
        public float AddMaxXpClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.MaxXp, amount, min, max);
        }

        // ---- Current HP (chp) ----

        /// <summary>Get current HP (includes bonuses).</summary>
        public float GetCurrentHp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.CurrentHp);
        }

        /// <summary>Get base current HP (excludes bonuses).</summary>
        public float GetBaseCurrentHp(Entity entity)
        {
            return GetFloat(entity, AttributeKey.CurrentHp, false);
        }

        /// <summary>Set absolute current HP value.</summary>
        public void SetCurrentHp(Entity entity, float value)
        {
            SetFloat(entity, AttributeKey.CurrentHp, value);
        }

        /// <summary>Add HP and clamp to [0, GetMaxHp(entity)].</summary>
        public float AddHp(Entity entity, float amount)
        {
            float maxHp = GetBaseMaxHp(entity);
            return AddFloatClamp(entity, AttributeKey.CurrentHp, amount, 0, maxHp);
        }

        /// <summary>Add HP with custom clamp.</summary>
        public float AddHpClamp(Entity entity, float amount, float min, float max)
        {
            return AddFloatClamp(entity, AttributeKey.CurrentHp, amount, min, max);
        }

        internal object GetCurrentVitality(Entity entity)
        {
            return GetFloat(entity, AttributeKey.CurrentVitality, false);
        }

    }

    /// <summary>
    /// Flat data container for attribute entries returned by GetAllAttributes.
    /// </summary>
    [System.Serializable]
    public class AttributeEntry
    {
        public string uid;
        public string name;
        public float value;
        public string displayValue;
        public string suffix;
        public bool isNumeric;
        public bool visibleInStats;
        public bool visible;
    }
}
