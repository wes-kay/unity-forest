using System;
using System.Collections.Generic;
using CharacterAttributes;
using SoftKitty;
using UnityEngine;
using Zenject;

/// <summary>
/// Zenject-injected service that manages quests, conditions, and progress tracking.
/// Quest definitions are auto-loaded from Assets/Resources/Quests/ at runtime.
/// Progress is tracked via event-driven condition updates.
/// </summary>
public class QuestService : IQuestService
{
    private const string SaveKey = "quest_progress";
    private const string ResourcesQuestsPath = "Quests";

    [Inject] CharacterAttributeService _characterAttributeService;

    // All quest definitions loaded from the SO assets.
    private List<QuestDefinitionSO> _questDefinitions = new List<QuestDefinitionSO>();

    // Runtime progress for each quest, keyed by quest ID.
    private Dictionary<string, QuestProgress> _questProgress = new Dictionary<string, QuestProgress>();

    public event Action<QuestDefinitionSO, QuestState, QuestState> OnQuestStateChanged;
    public event Action<QuestDefinitionSO> OnQuestAccepted;
    public event Action<QuestDefinitionSO> OnQuestCompleted;

    public Dictionary<string, QuestProgress> AllQuests => _questProgress;

    /// <summary>
    /// Initialize the service. Auto-loads quest definitions from Resources/Quests/.
    /// Called automatically by Zenject after dependency injection.
    /// </summary>
    [Inject]
    public void Initialize()
    {
        _questDefinitions = new List<QuestDefinitionSO>(
            Resources.LoadAll<QuestDefinitionSO>(ResourcesQuestsPath));

        foreach (var def in _questDefinitions)
        {
            if (!_questProgress.ContainsKey(def.questId))
            {
                _questProgress[def.questId] = new QuestProgress(def);
            }
        }
    }

    public QuestProgress GetProgress(string questId)
    {
        _questProgress.TryGetValue(questId, out var progress);
        return progress;
    }

    public List<QuestProgress> GetAvailableQuests()
    {
        var result = new List<QuestProgress>();
        foreach (var kvp in _questProgress)
        {
            if (kvp.Value.state == QuestState.Available)
                result.Add(kvp.Value);
        }
        return result;
    }

    public List<QuestProgress> GetActiveQuests()
    {
        var result = new List<QuestProgress>();
        foreach (var kvp in _questProgress)
        {
            if (kvp.Value.state == QuestState.Active)
                result.Add(kvp.Value);
        }
        return result;
    }

    public List<QuestProgress> GetCompletedQuests()
    {
        var result = new List<QuestProgress>();
        foreach (var kvp in _questProgress)
        {
            if (kvp.Value.state == QuestState.Completed)
                result.Add(kvp.Value);
        }
        return result;
    }

    public void AcceptQuest(string questId)
    {
        if (!_questProgress.TryGetValue(questId, out var progress))
            return;

        if (progress.state != QuestState.Available)
            return;

        progress.state = QuestState.Active;
        OnQuestStateChanged?.Invoke(progress.questDefinition, QuestState.Available, QuestState.Active);
        OnQuestAccepted?.Invoke(progress.questDefinition);
    }

    public void CompleteQuest(string questId)
    {
        if (!_questProgress.TryGetValue(questId, out var progress))
            return;

        if (progress.state != QuestState.Active)
            return;

        if (!progress.IsComplete())
            return;

        progress.state = QuestState.Completed;
        OnQuestStateChanged?.Invoke(progress.questDefinition, QuestState.Active, QuestState.Completed);
        OnQuestCompleted?.Invoke(progress.questDefinition);

        // Grant rewards
        GrantRewards(progress);
    }

    public void FailQuest(string questId)
    {
        if (!_questProgress.TryGetValue(questId, out var progress))
            return;

        if (progress.state != QuestState.Active)
            return;

        progress.state = QuestState.Failed;
        OnQuestStateChanged?.Invoke(progress.questDefinition, QuestState.Active, QuestState.Failed);
    }

    /// <summary>
    /// Grant the quest's rewards (XP, entity, etc.) to the player.
    /// </summary>
    private void GrantRewards(QuestProgress progress)
    {
        var def = progress.questDefinition;

        // Grant XP
        if (def.rewardXp > 0)
        {
            // TODO: Add XP system when available. For now, log.
            Debug.Log("[QuestService] Granted " + def.rewardXp + " XP for quest " + def.questId);
        }

        // Grant reward entity
        if (!string.IsNullOrEmpty(def.rewardEntityUid))
        {
            Debug.Log("[QuestService] Reward entity: " + def.rewardEntityUid);
        }
    }

    public void RecordKill(string targetEntityUid)
    {
        foreach (var kvp in _questProgress)
        {
            var progress = kvp.Value;
            if (progress.state != QuestState.Active)
                continue;

            for (int i = 0; i < progress.currentProgress.Length && i < progress.questDefinition.conditions.Count; i++)
            {
                var condition = progress.questDefinition.conditions[i];
                if (condition is KillCondition killCond && killCond.targetEntityUid == targetEntityUid)
                {
                    progress.currentProgress[i]++;
                    OnConditionProgressChanged(progress, i);
                }
            }
        }
    }

    public void RecordCollect(string itemUid)
    {
        foreach (var kvp in _questProgress)
        {
            var progress = kvp.Value;
            if (progress.state != QuestState.Active)
                continue;

            for (int i = 0; i < progress.currentProgress.Length && i < progress.questDefinition.conditions.Count; i++)
            {
                var condition = progress.questDefinition.conditions[i];
                if (condition is CollectCondition collectCond && collectCond.itemUid == itemUid)
                {
                    progress.currentProgress[i]++;
                    OnConditionProgressChanged(progress, i);
                }
            }
        }
    }

    public void RecordSceneVisit(string sceneName)
    {
        foreach (var kvp in _questProgress)
        {
            var progress = kvp.Value;
            if (progress.state != QuestState.Active)
                continue;

            for (int i = 0; i < progress.currentProgress.Length && i < progress.questDefinition.conditions.Count; i++)
            {
                var condition = progress.questDefinition.conditions[i];
                if (condition is SceneCondition sceneCond && sceneCond.targetSceneName == sceneName)
                {
                    progress.currentProgress[i] = 1; // Scene visit is binary: visited or not
                    OnConditionProgressChanged(progress, i);
                }
            }
        }
    }

    public void CheckAttributeConditions(Entity entity)
    {
        foreach (var kvp in _questProgress)
        {
            var progress = kvp.Value;
            if (progress.state != QuestState.Active)
                continue;

            for (int i = 0; i < progress.currentProgress.Length && i < progress.questDefinition.conditions.Count; i++)
            {
                var condition = progress.questDefinition.conditions[i];
                if (condition is AttributeCondition attrCond && attrCond.targetEntityUid == entity.uid)
                {
                    float currentValue = _characterAttributeService.GetFloat(entity, attrCond.attributeUid);
                    progress.currentProgress[i] = Mathf.FloorToInt(currentValue);
                    OnConditionProgressChanged(progress, i);
                }
            }
        }
    }

    private void OnConditionProgressChanged(QuestProgress progress, int conditionIndex)
    {
        // Check if this condition now meets its requirement
        if (conditionIndex < progress.questDefinition.conditions.Count)
        {
            var condition = progress.questDefinition.conditions[conditionIndex];
            if (condition.IsMet(progress, conditionIndex) && progress.currentProgress[conditionIndex] > 0)
            {
                // Condition just became met — check if all conditions are now complete
                if (progress.IsComplete())
                {
                    CompleteQuest(progress.questDefinition.questId);
                }
            }
        }
    }

    public void Save()
    {
        try
        {
            var saveData = new Dictionary<string, int>();
            foreach (var kvp in _questProgress)
            {
                saveData[kvp.Key] = (int)kvp.Value.state;
                // Save progress array as comma-separated string
                var progressStr = "";
                for (int i = 0; i < kvp.Value.currentProgress.Length; i++)
                {
                    progressStr += kvp.Value.currentProgress[i] + ",";
                }
                ES3.Save(kvp.Key + "_progress", progressStr);
            }
            ES3.Save(SaveKey, saveData);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[QuestService] Failed to save quest progress: " + e.Message);
        }
    }

    public void Load()
    {
        try
        {
            var savedStates = ES3.Load<Dictionary<string, int>>(SaveKey, new Dictionary<string, int>());
            foreach (var kvp in savedStates)
            {
                if (_questProgress.TryGetValue(kvp.Key, out var progress))
                {
                    progress.state = (QuestState)kvp.Value;

                    // Load progress array
                    var progressStr = ES3.Load<string>(kvp.Key + "_progress", "");
                    if (!string.IsNullOrEmpty(progressStr))
                    {
                        var parts = progressStr.Split(',');
                        for (int i = 0; i < progress.currentProgress.Length && i < parts.Length; i++)
                        {
                            int.TryParse(parts[i], out var val);
                            progress.currentProgress[i] = val;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[QuestService] Failed to load quest progress: " + e.Message);
        }
    }
}
