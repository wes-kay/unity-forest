using UnityEngine;
using Zenject;
using Domain.MVP.Tab;

namespace Domain.MVP.Party
{
    /// <summary>
    /// Presenter for the Party tab. Handles member selection and roster/detail sync.
    /// </summary>
    public class PartyTabPresenter : TabPresenter<PartyTabModel, PartyTabView>
    {
        [Inject] private PartyTabModel _partyModel;
        [Inject] private PartyTabView _partyView;

        public override void OnTabActivated()
        {
            if (!_partyModel.IsLoaded)
            {
                _partyModel.LoadFromService();
            }

            // Sync roster data
            _partyView.SetRosterData(
                _partyModel.MemberIds,
                _partyModel.GetMemberName,
                _partyModel.GetMemberPortrait
            );

            // Show the correct subtab
            _partyView.ShowSubtab(_partyModel.ActiveSubtab);

            // If detail subtab is active and a member is selected, update detail
            if (_partyModel.IsDetailActive && _partyModel.SelectedMemberId != null)
            {
                UpdateDetailPanel();
            }
        }

        public override void OnTabDeactivated()
        {
            // No cleanup needed — data stays cached
        }

        public override void OnSubtabChanged(string subtabId)
        {
            if (_partyModel.IsDetailActive && _partyModel.SelectedMemberId != null)
            {
                UpdateDetailPanel();
            }
        }

        /// <summary>Called when a roster cell is clicked.</summary>
        public void OnMemberPortraitClicked(string memberId)
        {
            _partyModel.SelectMember(memberId);
            UpdateDetailPanel();

            // Switch to detail subtab if roster was active
            if (_partyModel.IsRosterActive)
            {
                _partyModel.SetActiveSubtab("detail");
            }
        }

        private void UpdateDetailPanel()
        {
            if (_partyModel.SelectedMemberId == null) return;

            var hp = _partyModel.GetHealthPercent(_partyModel.SelectedMemberId);
            var sp = _partyModel.GetStaminaPercent(_partyModel.SelectedMemberId);
            var portrait = _partyModel.LoadPortraitByName(_partyModel.GetMemberName(_partyModel.SelectedMemberId));
            _partyView.UpdateDetail(_partyModel.SelectedMemberId, hp, sp, portrait);
            _partyView.RefreshAttributes(_partyModel.SelectedMemberId);
        }

        public override void Initialize()
        {
            base.Initialize();

            // Subscribe to view event for roster cell clicks
            _partyView.OnMemberPortraitClicked += OnMemberPortraitClicked;

            // Subscribe to model event for member selection
            _partyModel.OnMemberSelected += OnModelMemberSelected;
        }

        private void OnModelMemberSelected(string memberId)
        {
            if (_partyModel.IsDetailActive)
            {
                UpdateDetailPanel();
            }
        }

        public override void Destroy()
        {
            _partyView.OnMemberPortraitClicked -= OnMemberPortraitClicked;
            _partyModel.OnMemberSelected -= OnModelMemberSelected;
            base.Destroy();
        }
    }
}
