using System;
using System.Collections.Generic;
using SoftKitty;

/// <summary>
/// Contract for managing quests — accepting, tracking progress, and completing them.
/// </summary>
public interface IQuestService
{
    /// <summary>
    /// Fired when any quest's state changes.
    /// </summary>
    event Action<QuestDefinitionSO, QuestState, QuestState> OnQuestStateChanged;

    /// <summary>
    /// Fired when the player accepts a quest (Available → Active).
    /// </summary>
    event Action<QuestDefinitionSO> OnQuestAccepted;

    /// <summary>
    /// Fired when a quest is completed.
    /// </summary>
    event Action<QuestDefinitionSO> OnQuestCompleted;

    /// <summary>
    /// All tracked quests keyed by quest ID.
    /// </summary>
    System.Collections.Generic.Dictionary<string, QuestProgress> AllQuests { get; }

    /// <summary>
    /// Get the progress for a quest by its ID.
    /// </summary>
    QuestProgress GetProgress(string questId);

    /// <summary>
    /// Get all available quests (state == Available).
    /// </summary>
    List<QuestProgress> GetAvailableQuests();

    /// <summary>
    /// Get all active quests (state == Active).
    /// </summary>
    List<QuestProgress> GetActiveQuests();

    /// <summary>
    /// Get all completed quests (state == Completed).
    /// </summary>
    List<QuestProgress> GetCompletedQuests();

    /// <summary>
    /// Accept a quest. Transitions it from Available to Active.
    /// Does nothing if the quest is not available.
    /// </summary>
    void AcceptQuest(string questId);

    /// <summary>
    /// Complete a quest. Transitions it from Active to Completed and grants rewards.
    /// Does nothing if the quest is not active or conditions are not met.
    /// </summary>
    void CompleteQuest(string questId);

    /// <summary>
    /// Fail a quest. Transitions it from Active to Failed.
    /// Does nothing if the quest is not active.
    /// </summary>
    void FailQuest(string questId);

    /// <summary>
    /// Record a kill event — updates KillCondition progress.
    /// </summary>
    void RecordKill(string targetEntityUid);

    /// <summary>
    /// Record a collect event — updates CollectCondition progress.
    /// </summary>
    void RecordCollect(string itemUid);

    /// <summary>
    /// Record a scene visit event — updates SceneCondition progress.
    /// </summary>
    void RecordSceneVisit(string sceneName);

    /// <summary>
    /// Check attribute conditions for an entity (call when attributes change).
    /// </summary>
    void CheckAttributeConditions(Entity entity);

    /// <summary>
    /// Save all quest progress to persistent storage.
    /// </summary>
    void Save();

    /// <summary>
    /// Load all quest progress from persistent storage.
    /// </summary>
    void Load();
}
