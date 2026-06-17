using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject asset defining a single quest.
/// Create one per quest in Assets/Data/Quests/.
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/New Quest")]
public class QuestDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique quest ID — follows format QST_[FACTION]_[NUMBER], e.g. QST_ASH_001")]
    public string questId;

    [Header("Display")]
    public string title;
    [TextArea(3, 6)]
    public string description;
    [TextArea(3, 6)]
    public string completedDescription;
    [TextArea(3, 6)]
    public string failedDescription;

    [Header("Quest Giver")]
    [Tooltip("Entity UID of the NPC who gives this quest")]
    public string giverEntityUid;

    [Header("Target")]
    [Tooltip("Scene name to load when the player accepts this quest")]
    public string targetSceneName;

    [Header("Conditions")]
    [Tooltip("Conditions that must be met to complete this quest")]
    public List<QuestCondition> conditions = new List<QuestCondition>();

    [Header("Rewards")]
    [Tooltip("Entity UID of the reward entity granted on completion")]
    public string rewardEntityUid;
    [Tooltip("XP granted on completion")]
    public float rewardXp;
}
