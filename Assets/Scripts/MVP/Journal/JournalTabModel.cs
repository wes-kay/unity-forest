using System;
using System.Collections.Generic;
using Domain.MVP.Tab;

namespace Domain.MVP.Journal
{
    /// <summary>
    /// Model for the Journal tab. Manages quest lists and achievement data.
    /// </summary>
    public class JournalTabModel : TabModel
    {
        /// <summary>Fired when a quest's state changes.</summary>
        public event Action<string, QuestState, QuestState> OnQuestStateChanged;

        /// <summary>Fired when quest data refreshes.</summary>
        public event Action OnQuestDataChanged;

        /// <summary>Get all available quests (state == Available). Returns (questId, title, description, iconPath).</summary>
        public List<(string id, string title, string description, string iconPath)> GetAvailableQuests()
        {
            var result = new List<(string, string, string, string)>();
            // TODO: Query QuestService.GetAvailableQuests() and map to tuples
            return result;
        }

        /// <summary>Get all active quests (state == Active). Returns (questId, title, description, iconPath).</summary>
        public List<(string id, string title, string description, string iconPath)> GetActiveQuests()
        {
            var result = new List<(string, string, string, string)>();
            // TODO: Query QuestService.GetActiveQuests() and map to tuples
            return result;
        }

        /// <summary>Get all completed quests (state == Completed). Returns (questId, title, description, iconPath).</summary>
        public List<(string id, string title, string description, string iconPath)> GetCompletedQuests()
        {
            var result = new List<(string, string, string, string)>();
            // TODO: Query QuestService.GetCompletedQuests() and map to tuples
            return result;
        }

        /// <summary>Get progress data for a specific quest.</summary>
        public (QuestState state, int[] progress, int conditionCount, bool isComplete) GetQuestProgress(string questId)
        {
            // TODO: Query QuestService.GetProgress(questId)
            return (QuestState.Available, new int[0], 0, false);
        }

        /// <summary>Accept a quest. Transitions it from Available to Active.</summary>
        public void AcceptQuest(string questId)
        {
            // TODO: Call QuestService.AcceptQuest(questId)
            OnQuestDataChanged?.Invoke();
        }

        /// <summary>Complete a quest. Transitions it from Active to Completed.</summary>
        public void CompleteQuest(string questId)
        {
            // TODO: Call QuestService.CompleteQuest(questId)
            OnQuestDataChanged?.Invoke();
        }

        /// <summary>Fail a quest. Transitions it from Active to Failed.</summary>
        public void FailQuest(string questId)
        {
            // TODO: Call QuestService.FailQuest(questId)
            OnQuestDataChanged?.Invoke();
        }

        /// <summary>Get total active quest count.</summary>
        public int GetActiveQuestCount()
        {
            return GetActiveQuests().Count;
        }

        public JournalTabModel()
            : base("journal", "Journal", new[] { "quests", "achievements" })
        {
        }

        public override void LoadFromService()
        {
            // TODO: Subscribe to QuestService events and load initial quest data
            // QuestService.OnQuestStateChanged += HandleQuestStateChanged;
            // QuestService.Load(); // Load from save
        }

        private void HandleQuestStateChanged(object questDef, QuestState oldState, QuestState newState)
        {
            var def = questDef as QuestDefinitionSO;
            if (def != null)
            {
                OnQuestStateChanged?.Invoke(def.questId, oldState, newState);
                OnQuestDataChanged?.Invoke();
            }
        }
    }
}
