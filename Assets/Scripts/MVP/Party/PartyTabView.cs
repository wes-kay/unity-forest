using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;
using Domain.MVP.Party;
using PolyAndCode.UI;
using SoftKitty;

namespace Domain.MVP.Party
{
    /// <summary>
    /// View for the Party tab. Manages roster grid and member detail panel.
    /// </summary>
    public class PartyTabView : TabView
    {
        [Header("Roster Subtab")]
        [Tooltip("RecyclableScrollRect for the party roster list.")]
        public RecyclableScrollRect rosterScrollRect;

        // [Header("Roster Cell")]
        // [Tooltip("Prototype cell prefab for the roster list.")]
        // public GameObject rosterCellPrefab;

        [Header("Detail Subtab")]
        [Tooltip("Portrait image for the selected member.")]
        public Image leftPortrait, selectedPortrait, rightPortrait;

        [Tooltip("Name text for the selected member.")]
        public TextMeshProUGUI selectedNameText;

        [Tooltip("HP bar fill.")]
        public Image hpBarFill;

        [Tooltip("SP bar fill.")]
        public Image spBarFill;

        [Tooltip("Container for attribute rows.")]
        public RectTransform attributeContainer;

        [Tooltip("Prefab for an attribute row.")]
        public GameObject attributeRowPrefab;

        /// <summary>Fired when a roster member cell is clicked.</summary>
        public event Action<string> OnMemberPortraitClicked;

        private PartyRosterDataSource _rosterDataSource;

        public Button attributeButton, inventoryButton;

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);
        }

        /// <summary>Set the roster data and wire up the RecyclableScrollRect.</summary>
        public void SetRosterData(string[] memberIds, Func<string, string> getName, Func<string, Sprite> getPortrait)
        {
            // Build entity list from member UIDs
            var entities = new List<Entity>();
            for (int i = 0; i < memberIds.Length; i++)
            {
                var entity = GameManager.GetEntity(memberIds[i]);
                if (entity != null)
                    entities.Add(entity);
            }

            // Create or reuse the data source
            if (_rosterDataSource == null)
            {
                _rosterDataSource = new PartyRosterDataSource();
            }
            _rosterDataSource.SetEntities(entities);
            _rosterDataSource.OnCellClicked = OnCellClicked;
            _rosterDataSource.OnSelectionChanged = OnSelectionChanged;

            // Wire up the scroll rect's prototype cell
            // if (rosterScrollRect != null && rosterCellPrefab != null)
            {
                // Set prototype cell so the recycling system can clone from it
                // var cellInstance = Instantiate(rosterCellPrefab, rosterScrollRect.content);
                // cellInstance.SetActive(false);
                // rosterScrollRect.PrototypeCell = cellInstance.GetComponent<RectTransform>();

                rosterScrollRect.DataSource = _rosterDataSource;
                rosterScrollRect.Direction = RecyclableScrollRect.DirectionType.Vertical;

                // Always reinitialize — SelfInitialize may have run at Start with a null prototype
                rosterScrollRect.ReloadData();
            }
        }

        private void OnCellClicked(int index, Entity entity)
        {
            var uid = entity.uid;
            OnMemberPortraitClicked?.Invoke(uid);
        }

        private void OnSelectionChanged()
        {
            // Trigger a reload so all visible cells update their highlight
            if (rosterScrollRect != null)
            {
                rosterScrollRect.ReloadData();
            }
        }

        /// <summary>Update the health/stamina bars, name, and portrait for the selected member.</summary>
        public void UpdateDetail(string memberId, float hpPercent, float spPercent, Sprite portrait)
        {
            var name = memberId;
            if (selectedNameText != null) selectedNameText.text = name;

            // Update portrait — null portrait leaves the current image unchanged
            if (selectedPortrait != null && portrait != null)
            {
                selectedPortrait.sprite = portrait;
            }

            if (hpBarFill != null) hpBarFill.fillAmount = hpPercent;
            if (spBarFill != null) spBarFill.fillAmount = spPercent;
        }

        /// <summary>Refresh the attribute list for the currently selected member.</summary>
        public void RefreshAttributes(string memberId)
        {
            if (attributeContainer == null || attributeRowPrefab == null) return;

            // Clear existing
            for (int i = attributeContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(attributeContainer.GetChild(i).gameObject);
            }

            // TODO: Query CharacterAttributeService for the entity's attributes
            // and create rows from the result.
        }

        /// <summary>
        /// Load and display a character portrait from Resources.
        /// Path: characters/portrait/{characterName}
        /// </summary>
        public void ShowCharacterPortrait(string characterName)
        {
            if (string.IsNullOrEmpty(characterName))
            {
                ClearPortrait();
                return;
            }

            string path = $"characters/portrait/{characterName}";

            if (selectedPortrait != null)
            {
                var sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    selectedPortrait.sprite = sprite;
                    selectedPortrait.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"[PartyTabView] Portrait not found for '{characterName}' at path: {path}");
                    selectedPortrait.sprite = null;
                    selectedPortrait.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>Clear the character portrait display.</summary>
        public void ClearPortrait()
        {
            if (selectedPortrait != null)
            {
                selectedPortrait.sprite = null;
                selectedPortrait.gameObject.SetActive(false);
            }
        }
    }
}
