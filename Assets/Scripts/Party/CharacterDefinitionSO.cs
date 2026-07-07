// using System.Collections.Generic;
// using UnityEngine;

// /// <summary>
// /// ScriptableObject asset defining a single roster character.
// /// Create one per character in Assets/Data/Characters/.
// /// Used by CharacterSystemService to seed the roster on first load.
// /// </summary>
// [CreateAssetMenu(fileName = "NewCharacter", menuName = "Party/New Character")]
// public class CharacterDefinitionSO : ScriptableObject
// {
//     [Header("Identity")]
//     [Tooltip("Entity UID — must match the UID of the entity in the EntityManager")]
//     public string entityUid;

//     [Header("Display")]
//     [Tooltip("Display name shown in UI")]
//     public string displayName;

//     [Tooltip("Portrait sprite — loaded from Resources/characters/portrait/{entityUid} if null")]
//     public Sprite portrait;

//     [Tooltip("Short description shown in roster detail")]
//     [TextArea(2, 4)]
//     public string description;

//     [Header("Unlock Conditions")]
//     [Tooltip("Conditions that must be met before this character can be added to the roster.")]
//     public List<QuestCondition> unlockConditions = new List<QuestCondition>();

//     [Header("Starting Stats")]
//     [Tooltip("Initial level override (0 = use entity default)")]
//     public int startingLevel = 0;

//     [Tooltip("Initial XP override (0 = use entity default)")]
//     public float startingXp = 0f;

//     [Tooltip("Starting equipment entity UIDs to equip")]
//     public List<string> startingEquipmentUids = new List<string>();
// }
