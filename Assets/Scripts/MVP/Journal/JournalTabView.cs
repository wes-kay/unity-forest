using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;

namespace Domain.MVP.Journal
{
    /// <summary>
    /// View for the Journal tab. Manages quest lists, progress bars, and detail panel.
    /// </summary>
    public class JournalTabView : TabView
    {
        [Header("Quest List")]
        [Tooltip("Container for quest list items.")]
        public RectTransform questListContainer;

        [Tooltip("Prefab for a quest list item.")]
        public GameObject questItemPrefab;

        [Tooltip("Text showing active quest count.")]
        public TextMeshProUGUI activeCountText;

        [Header("Quest Detail Panel")]
        [Tooltip("Container for the quest detail/info panel.")]
        public RectTransform detailPanel;

        [Tooltip("Quest title in the detail panel.")]
        public TextMeshProUGUI detailTitleText;

        [Tooltip("Quest description in the detail panel.")]
        public TextMeshProUGUI detailDescText;

        [Tooltip("Progress bar fill for quest objectives.")]
        public Image progressBarFill;

        [Tooltip("Progress text (e.g. '2/5').")]
        public TextMeshProUGUI progressText;

        [Tooltip("Accept button for available quests.")]
        public Button acceptButton;

        [Tooltip("Complete button for active quests.")]
        public Button completeButton;

        [Tooltip("Fail button for active quests.")]
        public Button failButton;

        [Header("Achievement List")]
        [Tooltip("Container for achievement list items.")]
        public RectTransform achievementListContainer;

        [Tooltip("Prefab for an achievement list item.")]
        public GameObject achievementItemPrefab;

        /// <summary>Fired when a quest item is clicked.</summary>
        public event Action<string> OnQuestItemClick;

        /// <summary>Fired when the accept button is pressed.</summary>
        public event Action OnAcceptPressed;

        /// <summary>Fired when the complete button is pressed.</summary>
        public event Action OnCompletePressed;

        /// <summary>Fired when the fail button is pressed.</summary>
        public event Action OnFailPressed;

        private Dictionary<string, GameObject> _questItems;
        private Dictionary<string, GameObject> _achievementItems;

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);
            _questItems = new Dictionary<string, GameObject>();
            _achievementItems = new Dictionary<string, GameObject>();
        }

        /// <summary>Update the active quest count text (e.g. '3 Active').</summary>
        public void SetActiveCountText(int count)
        {
            if (activeCountText != null)
                activeCountText.text = count > 0 ? $"{count} Active" : "No Active Quests";
        }

        /// <summary>Refresh the quest list for a given state.</summary>
        public void RefreshQuestList(List<(string id, string title, string description, string iconPath)> quests)
        {
            if (questListContainer == null || questItemPrefab == null) return;

            // Clear existing items
            foreach (var kvp in _questItems)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _questItems.Clear();

            foreach (var quest in quests)
            {
                var item = Instantiate(questItemPrefab, questListContainer);
                item.SetActive(true);

                var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null) nameText.text = quest.title;

                var btn = item.GetComponent<Button>();
                if (btn != null)
                {
                    var questId = quest.id;
                    btn.onClick.AddListener(() => OnQuestItemClick?.Invoke(questId));
                }

                _questItems[quest.id] = item;
            }
        }

        /// <summary>Update the detail panel with the selected quest's data.</summary>
        public void UpdateDetailPanel(string title, string description, int currentProgress, int totalProgress, QuestState state)
        {
            if (detailPanel != null) detailPanel.gameObject.SetActive(true);
            if (detailTitleText != null) detailTitleText.text = title;
            if (detailDescText != null) detailDescText.text = description;

            if (progressText != null)
            {
                progressText.text = totalProgress > 0 ? $"{currentProgress}/{totalProgress}" : string.Empty;
            }

            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = totalProgress > 0 ? (float)currentProgress / totalProgress : 0f;
            }

            // Show/hide action buttons based on quest state
            if (acceptButton != null) acceptButton.gameObject.SetActive(state == QuestState.Available);
            if (completeButton != null) completeButton.gameObject.SetActive(state == QuestState.Active);
            if (failButton != null) failButton.gameObject.SetActive(state == QuestState.Active);
        }

        /// <summary>Clear the detail panel.</summary>
        public void ClearDetailPanel()
        {
            if (detailPanel != null) detailPanel.gameObject.SetActive(false);
            if (detailTitleText != null) detailTitleText.text = string.Empty;
            if (detailDescText != null) detailDescText.text = string.Empty;
            if (progressBarFill != null) progressBarFill.fillAmount = 0f;
            if (progressText != null) progressText.text = string.Empty;
        }

        /// <summary>Refresh the achievement list.</summary>
        public void RefreshAchievementList(List<(string id, string title, string description, bool isUnlocked)> achievements)
        {
            if (achievementListContainer == null || achievementItemPrefab == null) return;

            foreach (var kvp in _achievementItems)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _achievementItems.Clear();

            foreach (var achievement in achievements)
            {
                var item = Instantiate(achievementItemPrefab, achievementListContainer);
                item.SetActive(true);

                var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null) nameText.text = achievement.title;

                _achievementItems[achievement.id] = item;
            }
        }

        public override void ShowSubtab(string subtabId)
        {
            base.ShowSubtab(subtabId);

            // Hide detail panel when switching subtabs
            if (detailPanel != null)
                detailPanel.gameObject.SetActive(false);
        }
    }
}
