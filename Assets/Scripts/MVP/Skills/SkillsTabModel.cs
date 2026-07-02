using System;
using Domain.MVP.Tab;

namespace Domain.MVP.Skills
{
    /// <summary>
    /// Model for the Skills tab. Manages skill categories and active skill state.
    /// </summary>
    public class SkillsTabModel : TabModel
    {
        /// <summary>Currently selected skill UID (empty = none).</summary>
        public string SelectedSkillId { get; private set; }

        /// <summary>Fired when a skill is selected.</summary>
        public event Action<string> OnSkillSelected;

        /// <summary>Fired when skill data refreshes.</summary>
        public event Action OnSkillDataChanged;

        public SkillsTabModel()
            : base("skills", "Skills", new[] { "all", "combat", "magic", "crafting" })
        {
        }

        public override void LoadFromService()
        {
            // TODO: Load skill data from skill system
        }

        public void SelectSkill(string skillId)
        {
            SelectedSkillId = skillId;
            OnSkillSelected?.Invoke(skillId);
        }

        public void ClearSelection()
        {
            SelectedSkillId = string.Empty;
        }

        /// <summary>Get skills filtered by category. TODO: implement with skill data.</summary>
        public string[] GetSkillsByCategory(string categoryId)
        {
            // TODO: Query skill system for skills in this category
            return Array.Empty<string>();
        }
    }
}
