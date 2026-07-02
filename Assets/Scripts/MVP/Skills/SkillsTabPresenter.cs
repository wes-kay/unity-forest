using System.Collections.Generic;
using UnityEngine;
using Zenject;
using SoftKitty.InventoryEngine;
using Domain.MVP.Tab;

namespace Domain.MVP.Skills
{
    /// <summary>
    /// Presenter for the Skills tab. Bridges MVP architecture with SoftKitty InventoryEngine.
    /// When the tab is activated, opens the SoftKitty Skills window prefab.
    /// </summary>
    public class SkillsTabPresenter : TabPresenter<SkillsTabModel, SkillsTabView>
    {
        [Inject] private SkillsTabModel _model;
        [Inject] private SkillsTabView _view;

        /// <summary>The currently open SoftKitty Skills window instance.</summary>
        private UiWindow _openedWindow;

        public override void OnTabActivated()
        {
            if (!_model.IsLoaded)
            {
                _model.LoadFromService();
            }

            // Open the SoftKitty Skills window
            OpenSkillWindow();

            // Sync skill list
            RefreshSkillList();
        }

        public override void OnTabDeactivated()
        {
            // Close the SoftKitty window when tab is deactivated
            CloseSkillWindow();
        }

        public override void OnSubtabChanged(string subtabId)
        {
            // Category changed — refresh the skill list
            RefreshSkillList();
            _view.ClearSkillDetail();
        }

        /// <summary>Handle skill item click from the view.</summary>
        public void OnSkillClicked(string skillId)
        {
            _model.SelectSkill(skillId);
            _view.UpdateSkillDetail(skillId, skillId, string.Empty);
            _view.SetDetailPanelActive(true);
        }

        /// <summary>Handle detail panel close from the view.</summary>
        public void OnDetailCloseClicked()
        {
            _model.ClearSelection();
            _view.ClearSkillDetail();
            _view.SetDetailPanelActive(false);
        }

        // ==================== SoftKitty Window Management ====================

        private void OpenSkillWindow()
        {
            if (_openedWindow != null)
            {
                _openedWindow.gameObject.SetActive(true);
                return;
            }

            // Use the Skills prefab from SoftKitty Resources
            // _openedWindow = WindowsManager.GetWindow("Skills", ItemObject.PlayerInventoryData);
            if (_openedWindow == null) return;

            // Reparent the window under the MVP content panel
            var rt = _openedWindow.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.SetParent(_view.contentContainer, false);
                rt.localPosition = Vector3.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(500f, 400f);
            }

            _openedWindow.ActiveWindow();
        }

        private void CloseSkillWindow()
        {
            if (_openedWindow != null)
            {
                _openedWindow.Close();
                _openedWindow = null;
            }
        }

        // ==================== Helpers ====================

        private void RefreshSkillList()
        {
            var skills = _model.GetSkillsByCategory(_model.ActiveSubtab);
            _view.RefreshSkillList(skills);
        }

        public override void Initialize()
        {
            base.Initialize();

            // Subscribe to view events
            _view.OnSkillClicked += OnSkillClicked;
            _view.OnDetailCloseClicked += OnDetailCloseClicked;
        }

        public override void Destroy()
        {
            _view.OnSkillClicked -= OnSkillClicked;
            _view.OnDetailCloseClicked -= OnDetailCloseClicked;

            CloseSkillWindow();

            base.Destroy();
        }
    }
}
