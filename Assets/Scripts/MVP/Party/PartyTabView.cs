using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Domain.MVP.Tab;
using Domain.MVP.Party;
using PolyAndCode.UI;
using SoftKitty;
using SoftKitty.InventoryEngine;

namespace Domain.MVP.Party
{
    /// <summary>
    /// View for the Party tab. Manages roster grid and member detail panel.
    /// Carousel navigation is driven by the RecyclableScrollRect roster — clicking a cell
    /// updates the detail panel (portrait, name, bars) and the left/right preview portraits.
    /// </summary>
    public class PartyTabView : TabView
    {
        [Header("Roster Subtab")]
        [Tooltip("RecyclableScrollRect for the party roster list.")]
        public RecyclableScrollRect rosterScrollRect;

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
        private int _selectedIndex = -1;
        private string _selectedMemberId;

        public Button attributeButton, inventoryButton;

        public override void Initialize(TabModel model)
        {
            base.Initialize(model);

            // Wire button click handlers
            if (inventoryButton != null)
            {
                inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            }
            if (attributeButton != null)
            {
                attributeButton.onClick.AddListener(OnAttributeButtonClicked);
            }
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
            {
                rosterScrollRect.DataSource = _rosterDataSource;
                rosterScrollRect.Direction = RecyclableScrollRect.DirectionType.Vertical;

                // Always reinitialize — SelfInitialize may have run at Start with a null prototype
                rosterScrollRect.ReloadData();
            }
        }

        /// <summary>
        /// Called when a roster cell is clicked. Updates selection and detail panel.
        /// </summary>
        private void OnCellClicked(int index, Entity entity)
        {
            _selectedIndex = index;
            _selectedMemberId = entity.uid;

            // Update the roster highlight
            _rosterDataSource.SelectedIndex = index;

            // Update the detail panel
            UpdateDetail(_selectedMemberId, entity);

            // Forward the event for any external listeners
            OnMemberPortraitClicked?.Invoke(_selectedMemberId);
        }

        /// <summary>Called when the roster selection index changes.</summary>
        private void OnSelectionChanged()
        {
            if (rosterScrollRect != null)
            {
                rosterScrollRect.ReloadData();
            }
        }

        /// <summary>Update the health/stamina bars, name, and portrait for the selected member.</summary>
        public void UpdateDetail(string memberId, float hpPercent, float spPercent, Sprite portrait)
        {
            _selectedMemberId = memberId;

            // Name
            if (selectedNameText != null)
            {
                selectedNameText.text = memberId;
            }

            // Selected portrait
            if (portrait != null && selectedPortrait != null)
            {
                selectedPortrait.sprite = portrait;
                selectedPortrait.gameObject.SetActive(true);
            }
            else if (selectedPortrait != null)
            {
                // Try loading from Resources as fallback
                string portraitPath = $"characters/portrait/{memberId}";
                Sprite loaded = Resources.Load<Sprite>(portraitPath);
                if (loaded != null)
                {
                    selectedPortrait.sprite = loaded;
                    selectedPortrait.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"[PartyTabView] Portrait not found for '{memberId}' at path: {portraitPath}");
                    selectedPortrait.gameObject.SetActive(false);
                }
            }

            if (hpBarFill != null) hpBarFill.fillAmount = hpPercent;
            if (spBarFill != null) spBarFill.fillAmount = spPercent;

            // Update left/right portraits from roster neighbors
            UpdateSidePortraits();
        }

        /// <summary>Update the detail panel using the Entity object directly.</summary>
        public void UpdateDetail(string memberId, Entity entity)
        {
            _selectedMemberId = memberId;

            // Name
            if (selectedNameText != null)
            {
                selectedNameText.text = memberId;
            }

            // Selected portrait from Resources
            string portraitPath = $"characters/portrait/{memberId}";
            Sprite portrait = Resources.Load<Sprite>(portraitPath);
            if (portrait != null && selectedPortrait != null)
            {
                selectedPortrait.sprite = portrait;
                selectedPortrait.gameObject.SetActive(true);
            }
            else if (selectedPortrait != null)
            {
                Debug.LogWarning($"[PartyTabView] Portrait not found for '{memberId}' at path: {portraitPath}");
                selectedPortrait.gameObject.SetActive(false);
            }

            // HP / SP bars from entity attributes (default to 100% if unavailable)
            if (hpBarFill != null)
            {
                float hp = entity.GetAttributeFloat(1, true); // attribute ID 1 = HP (common convention)
                float maxHp = entity.GetAttributeFloat(2, true); // attribute ID 2 = Max HP
                hpBarFill.fillAmount = maxHp > 0 ? hp / maxHp : 1f;
            }
            if (spBarFill != null)
            {
                float sp = entity.GetAttributeFloat(3, true); // attribute ID 3 = SP
                float maxSp = entity.GetAttributeFloat(4, true); // attribute ID 4 = Max SP
                spBarFill.fillAmount = maxSp > 0 ? sp / maxSp : 1f;
            }

            // Update left/right portraits from roster neighbors
            UpdateSidePortraits();
        }

        /// <summary>Load and update left/right side portraits from roster neighbors.</summary>
        private void UpdateSidePortraits()
        {
            if (_rosterDataSource == null || _selectedMemberId == null) return;

            // Find the selected member's index in the roster
            int selectedIndex = -1;
            int count = _rosterDataSource.GetItemCount();
            for (int i = 0; i < count; i++)
            {
                Entity e = _rosterDataSource.GetEntityAt(i);
                if (e != null && e.uid == _selectedMemberId)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex < 0) return;

            // Left portrait (previous character)
            int leftIndex = selectedIndex - 1;
            if (leftPortrait != null)
            {
                if (leftIndex >= 0)
                {
                    Entity leftEntity = _rosterDataSource.GetEntityAt(leftIndex);
                    if (leftEntity != null)
                    {
                        string path = $"characters/portrait/{leftEntity.uid}";
                        var sprite = Resources.Load<Sprite>(path);
                        if (sprite != null)
                        {
                            leftPortrait.sprite = sprite;
                            leftPortrait.gameObject.SetActive(true);
                        }
                        else
                        {
                            leftPortrait.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        leftPortrait.gameObject.SetActive(false);
                    }
                }
                else
                {
                    leftPortrait.gameObject.SetActive(false);
                }
            }

            // Right portrait (next character)
            int rightIndex = selectedIndex + 1;
            if (rightPortrait != null)
            {
                if (rightIndex < count)
                {
                    Entity rightEntity = _rosterDataSource.GetEntityAt(rightIndex);
                    if (rightEntity != null)
                    {
                        string path = $"characters/portrait/{rightEntity.uid}";
                        var sprite = Resources.Load<Sprite>(path);
                        if (sprite != null)
                        {
                            rightPortrait.sprite = sprite;
                            rightPortrait.gameObject.SetActive(true);
                        }
                        else
                        {
                            rightPortrait.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        rightPortrait.gameObject.SetActive(false);
                    }
                }
                else
                {
                    rightPortrait.gameObject.SetActive(false);
                }
            }
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

        #region Button Handlers

        private void OnInventoryButtonClicked()
        {
            if (string.IsNullOrEmpty(_selectedMemberId))
            {
                Debug.LogWarning("[PartyTabView] No selected member for inventory.");
                return;
            }

            Entity entity = GameManager.GetEntity(_selectedMemberId);
            if (entity == null)
            {
                Debug.LogWarning($"[PartyTabView] Entity '{_selectedMemberId}' not found for inventory.");
                return;
            }

            InventoryModule invModule = entity.GetModule<InventoryModule>();
            if (invModule == null)
            {
                Debug.LogWarning($"[PartyTabView] Entity '{_selectedMemberId}' has no InventoryModule.");
                return;
            }

            InventoryData inventoryData = invModule.GetInventory();
            if (inventoryData == null)
            {
                Debug.LogWarning($"[PartyTabView] Entity '{_selectedMemberId}' has no inventory data.");
                return;
            }

            inventoryData.OpenWindow();
        }

        private void OnAttributeButtonClicked()
        {
            if (string.IsNullOrEmpty(_selectedMemberId))
            {
                Debug.LogWarning("[PartyTabView] No selected member for attributes.");
                return;
            }

            RefreshAttributes(_selectedMemberId);
        }

        #endregion
    }
}
