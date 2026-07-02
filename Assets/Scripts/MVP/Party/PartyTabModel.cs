using System;
using System.Collections.Generic;
using CharacterAttributes;
using Domain.MVP.Tab;
using PolyAndCode.UI;
using SoftKitty;
using UnityEngine;
using Zenject;

namespace Domain.MVP.Party
{
    /// <summary>
    /// Model for the Party tab. Manages party member roster, selection, and per-member stats.
    /// </summary>
    public class PartyTabModel : TabModel
    {
        [Inject] private IPartyService _partyService;

        /// <summary>All party member UIDs.</summary>
        public string[] MemberIds { get; private set; }

        /// <summary>Currently selected party member UID.</summary>
        public string SelectedMemberId { get; private set; }

        /// <summary>Whether the roster subtab is active.</summary>
        public bool IsRosterActive => ActiveSubtab == "roster";

        /// <summary>Whether the detail subtab is active.</summary>
        public bool IsDetailActive => ActiveSubtab == "detail";

        /// <summary>Fired when a party member is selected.</summary>
        public event System.Action<string> OnMemberSelected;

        /// <summary>Fired when party data refreshes.</summary>
        public Action OnDataChanged;

        /// <summary>Get a party member's current HP percentage (0-1).</summary>
        public float GetHealthPercent(string memberId)
        {
            var entity = _partyService?.GetMemberAt(System.Array.IndexOf(MemberIds, memberId));
            if (entity == null) return 1f;

            float current = entity.GetAttributeFloat(AttributeKey.CurrentHp.GetUid());
            float max = entity.GetAttributeFloat(AttributeKey.Health.GetUid());
            return max > 0f ? Mathf.Clamp01(current / max) : 1f;
        }

        /// <summary>Get a party member's current SP percentage (0-1).</summary>
        public float GetStaminaPercent(string memberId)
        {
            var entity = _partyService?.GetMemberAt(System.Array.IndexOf(MemberIds, memberId));
            if (entity == null) return 1f;

            float current = entity.GetAttributeFloat(AttributeKey.Stamina.GetUid());
            float max = entity.GetAttributeFloat(AttributeKey.Stamina.GetUid());
            // TODO: Use a separate max-stamina attribute if available (e.g. "msp")
            return max > 0f ? Mathf.Clamp01(current / max) : 1f;
        }

        public PartyTabModel()
            : base("party", "Party", new[] { "roster", "detail" })
        {
            MemberIds = Array.Empty<string>();
        }

        public override void LoadFromService()
        {
            if (_partyService == null) return;

            var members = _partyService.GetMembers();
            MemberIds = new string[members.Count];
            for (int i = 0; i < members.Count; i++)
            {
                MemberIds[i] = members[i].uid;
            }

            if (MemberIds.Length > 0)
            {
                SelectedMemberId = MemberIds[0];
            }
        }

        public void SelectMember(string memberId)
        {
            if (System.Array.IndexOf(MemberIds, memberId) < 0) return;
            SelectedMemberId = memberId;
            OnMemberSelected?.Invoke(memberId);
        }

        public void DeselectMember()
        {
            SelectedMemberId = null;
        }

        /// <summary>Get the display name for a party member.</summary>
        public string GetMemberName(string memberId)
        {
            int index = System.Array.IndexOf(MemberIds, memberId);
            var entity = _partyService?.GetMemberAt(index);
            if (entity == null) return memberId;

            var name = entity.GetAttributeString(AttributeKey.Name.GetUid());
            return string.IsNullOrEmpty(name) ? memberId : name;
        }

        /// <summary>Get the portrait sprite for a party member (null → default icon).</summary>
        public Sprite GetMemberPortrait(string memberId)
        {
            int index = System.Array.IndexOf(MemberIds, memberId);
            return _partyService?.GetMemberPortrait(index);
        }

        /// <summary>Load a portrait sprite from Resources by member name.</summary>
        public Sprite LoadPortraitByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            // Convert name to the expected sprite naming convention: "charname_portrait"
            var spriteName = $"{name}_portrait";
            var path = $"images/character/{spriteName}";

            try
            {
                var texture = Resources.Load<Texture2D>(path);
                if (texture != null)
                {
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
            }
            catch
            {
                // Silently fail — portrait is optional
            }

            return null;
        }
    }
}
