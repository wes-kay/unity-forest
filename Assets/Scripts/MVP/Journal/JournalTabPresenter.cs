using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Domain.MVP.Tab;

namespace Domain.MVP.Journal
{
    /// <summary>
    /// Presenter for the Journal tab. Handles quest selection, accept/complete/fail operations, and subtab sync.
    /// </summary>
    public class JournalTabPresenter : TabPresenter<JournalTabModel, JournalTabView>
    {
        [Inject] private JournalTabModel _journalModel;
        [Inject] private JournalTabView _journalView;

        /// <summary>Currently selected quest ID (null = none).</summary>
        private string _selectedQuestId;

        public override void OnTabActivated()
        {
            if (!_journalModel.IsLoaded)
            {
                _journalModel.LoadFromService();
            }

            // Sync quest list for the active subtab
            RefreshQuestList();

            // Sync achievement list
            RefreshAchievementList();

            // Show the correct subtab
            _journalView.ShowSubtab(_journalModel.ActiveSubtab);

            // Update active quest count
            _journalView.SetActiveCountText(_journalModel.GetActiveQuestCount());
        }

        public override void OnTabDeactivated()
        {
            // No cleanup needed — data stays cached
        }

        public override void OnSubtabChanged(string subtabId)
        {
            if (subtabId == "quests")
            {
                RefreshQuestList();
            }
            else
            {
                RefreshAchievementList();
            }

            // Hide detail panel when switching subtabs
            _journalView.ClearDetailPanel();
        }

        /// <summary>Handle quest item click from the view.</summary>
        public void OnQuestItemClick(string questId)
        {
            _selectedQuestId = questId;

            // Get quest progress data
            var (state, progress, conditionCount, isComplete) = _journalModel.GetQuestProgress(questId);

            // Get quest title/description from the active or completed list
            string title = questId;
            string description = string.Empty;

            var activeQuests = _journalModel.GetActiveQuests();
            foreach (var q in activeQuests)
            {
                if (q.id == questId) { title = q.title; description = q.description; break; }
            }
            if (string.IsNullOrEmpty(description))
            {
                var completedQuests = _journalModel.GetCompletedQuests();
                foreach (var q in completedQuests)
                {
                    if (q.id == questId) { title = q.title; description = q.description; break; }
                }
            }

            int currentProgress = 0;
            for (int i = 0; i < progress.Length; i++)
            {
                currentProgress += progress[i];
            }

            _journalView.UpdateDetailPanel(title, description, currentProgress, conditionCount, state);
        }

        /// <summary>Handle accept button press.</summary>
        public void OnAcceptPressed()
        {
            if (_selectedQuestId == null) return;
            _journalModel.AcceptQuest(_selectedQuestId);
            RefreshQuestList();
            _journalView.SetActiveCountText(_journalModel.GetActiveQuestCount());
        }

        /// <summary>Handle complete button press.</summary>
        public void OnCompletePressed()
        {
            if (_selectedQuestId == null) return;
            _journalModel.CompleteQuest(_selectedQuestId);
            RefreshQuestList();
            _journalView.SetActiveCountText(_journalModel.GetActiveQuestCount());
        }

        /// <summary>Handle fail button press.</summary>
        public void OnFailPressed()
        {
            if (_selectedQuestId == null) return;
            _journalModel.FailQuest(_selectedQuestId);
            RefreshQuestList();
            _journalView.SetActiveCountText(_journalModel.GetActiveQuestCount());
        }

        // ==================== Helpers ====================

        private void RefreshQuestList()
        {
            List<(string id, string title, string description, string iconPath)> quests;

            if (_journalModel.ActiveSubtab == "quests")
            {
                // Show active quests by default when on the quests subtab
                quests = _journalModel.GetActiveQuests();
                if (quests.Count == 0)
                {
                    // Fall back to available if no active quests
                    quests = _journalModel.GetAvailableQuests();
                }
            }
            else
            {
                // On achievements subtab, show completed quests as reference
                quests = _journalModel.GetCompletedQuests();
            }

            _journalView.RefreshQuestList(quests);
        }

        private void RefreshAchievementList()
        {
            // TODO: Query achievement data from a service
            var achievements = new List<(string id, string title, string description, bool isUnlocked)>();
            _journalView.RefreshAchievementList(achievements);
        }

        public override void Initialize()
        {
            base.Initialize();

            // Subscribe to view events
            _journalView.OnQuestItemClick += OnQuestItemClick;
            _journalView.OnAcceptPressed += OnAcceptPressed;
            _journalView.OnCompletePressed += OnCompletePressed;
            _journalView.OnFailPressed += OnFailPressed;

            // Subscribe to model events
            _journalModel.OnQuestStateChanged += OnModelQuestStateChanged;
            _journalModel.OnQuestDataChanged += OnModelQuestDataChanged;
        }

        private void OnModelQuestStateChanged(string questId, QuestState oldState, QuestState newState)
        {
            // If the selected quest changed state, refresh its detail panel
            if (_selectedQuestId == questId)
            {
                var (state, progress, conditionCount, isComplete) = _journalModel.GetQuestProgress(questId);
                int currentProgress = 0;
                for (int i = 0; i < progress.Length; i++) currentProgress += progress[i];

                // Get title/description
                string title = questId;
                string description = string.Empty;
                var activeQuests = _journalModel.GetActiveQuests();
                foreach (var q in activeQuests)
                {
                    if (q.id == questId) { title = q.title; description = q.description; break; }
                }

                _journalView.UpdateDetailPanel(title, description, currentProgress, conditionCount, state);
            }
        }

        private void OnModelQuestDataChanged()
        {
            // Refresh the quest list and active count when model data changes externally
            RefreshQuestList();
            _journalView.SetActiveCountText(_journalModel.GetActiveQuestCount());
        }

        public override void Destroy()
        {
            _journalView.OnQuestItemClick -= OnQuestItemClick;
            _journalView.OnAcceptPressed -= OnAcceptPressed;
            _journalView.OnCompletePressed -= OnCompletePressed;
            _journalView.OnFailPressed -= OnFailPressed;

            _journalModel.OnQuestStateChanged -= OnModelQuestStateChanged;
            _journalModel.OnQuestDataChanged -= OnModelQuestDataChanged;

            base.Destroy();
        }
    }
}
