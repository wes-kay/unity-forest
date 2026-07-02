using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;
using System.Collections.Generic;

namespace Domain.MVP.Skills
{
    /// <summary>
    /// View for the Skills tab. Manages skill category buttons and skill list display.
    /// The actual skill window is opened via the presenter calling SoftKitty WindowsManager.
    /// </summary>
    public class SkillsTabView : TabView
    {
        [Header("Skill List")]
        [Tooltip("Container where skill items are parented.")]
        public RectTransform skillListContainer;

        [Tooltip("Prefab for a skill item button.")]
        public GameObject skillItemPrefab;

        [Tooltip("Text showing total skill count.")]
        public TextMeshProUGUI skillCountText;

        [Header("Skill Detail")]
        [Tooltip("Container for the selected skill detail panel.")]
        public RectTransform skillDetailPanel;

        [Tooltip("Skill name in detail panel.")]
        public TextMeshProUGUI detailSkillNameText;

        [Tooltip("Skill description in detail panel.")]
        public TextMeshProUGUI detailSkillDescText;

        [Tooltip("Skill icon in detail panel.")]
        public Image detailSkillIconImage;

        /// <summary>Fired when a skill item is clicked.</summary>
        public event Action<string> OnSkillClicked;

        /// <summary>Fired when the detail panel close button is clicked.</summary>
        public event Action OnDetailCloseClicked;

        private readonly Dictionary<string, GameObject> _skillItems = new Dictionary<string, GameObject>();

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);
        }

        /// <summary>Refresh the skill list display.</summary>
        public void RefreshSkillList(string[] skillIds)
        {
            if (skillListContainer == null || skillItemPrefab == null) return;

            // Clear existing items
            foreach (var kvp in _skillItems)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _skillItems.Clear();

            foreach (var skillId in skillIds)
            {
                var item = Instantiate(skillItemPrefab, skillListContainer);
                item.SetActive(true);

                var text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = skillId;

                var btn = item.GetComponent<Button>();
                if (btn != null)
                {
                    var sid = skillId;
                    btn.onClick.AddListener(() => OnSkillClicked?.Invoke(sid));
                }

                _skillItems[skillId] = item;
            }

            // Update count text
            if (skillCountText != null)
                skillCountText.text = $"Skills: {skillIds.Length}";
        }

        /// <summary>Update the detail panel with selected skill data.</summary>
        public void UpdateSkillDetail(string name, string description, string iconPath)
        {
            if (detailSkillNameText != null) detailSkillNameText.text = name;
            if (detailSkillDescText != null) detailSkillDescText.text = description;
            // TODO: Load icon from iconPath and set detailSkillIconImage.sprite
        }

        /// <summary>Clear the detail panel.</summary>
        public void ClearSkillDetail()
        {
            if (detailSkillNameText != null) detailSkillNameText.text = string.Empty;
            if (detailSkillDescText != null) detailSkillDescText.text = string.Empty;
            if (detailSkillIconImage != null) detailSkillIconImage.sprite = null;
        }

        /// <summary>Toggle the detail panel visibility.</summary>
        public void SetDetailPanelActive(bool active)
        {
            if (skillDetailPanel != null)
                skillDetailPanel.gameObject.SetActive(active);
        }
    }
}
