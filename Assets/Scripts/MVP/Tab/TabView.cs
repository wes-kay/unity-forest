using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Domain.MVP.Tab
{
    /// <summary>
    /// Base view for a hub tab. Manages subtab buttons and content panels.
    /// Concrete tabs extend this to add domain-specific UI elements.
    /// </summary>
    public abstract class TabView : MonoBehaviour
    {
        [Header("Subtab Buttons")]
        [Tooltip("Container for subtab toggle buttons.")]
        public RectTransform subtabButtonContainer;

        [Header("Content Area")]
        [Tooltip("Container where subtab content panels are parented.")]
        public RectTransform contentContainer;

        [Header("Settings")]
        [Tooltip("Button prefab for subtab buttons.")]
        public Button subtabButtonPrefab;

        [Tooltip("Normal button color.")]
        public Color normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);

        [Tooltip("Selected button color.")]
        public Color selectedColor = new Color(0.6f, 0.15f, 0.1f, 1f);

        [Tooltip("Disabled button color.")]
        public Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        /// <summary>Fired when a subtab button is clicked.</summary>
        public event Action<string> OnSubtabClicked;

        // Keyed by subtabId for O(1) lookup
        protected Dictionary<string, Button> _subtabButtonMap;
        protected Dictionary<string, GameObject> _contentPanels;
        protected string _activeSubtab;

        public string ActiveSubtab => _activeSubtab;

        /// <summary>Initialize the view with tab model data. Creates subtab buttons.</summary>
        public virtual void Initialize(TabModel model)
        {
            _subtabButtonMap = new Dictionary<string, Button>();
            _contentPanels = new Dictionary<string, GameObject>();
            _activeSubtab = model.ActiveSubtab;

            CreateSubtabButtons(model);
        }

        /// <summary>Switch to a subtab by identifier — updates button highlights and panel visibility.</summary>
        public virtual void ShowSubtab(string subtabId)
        {
            if (_activeSubtab == subtabId) return;
            _activeSubtab = subtabId;

            // Update button highlight states
            foreach (var kvp in _subtabButtonMap)
            {
                SetButtonSelected(kvp.Value, kvp.Key == subtabId);
            }

            // Show only the matching content panel
            foreach (var kvp in _contentPanels)
            {
                kvp.Value.SetActive(kvp.Key == subtabId);
            }
        }

        /// <summary>Register a content panel for a subtab.</summary>
        public void RegisterContentPanel(string subtabId, GameObject panel)
        {
            if (panel == null) return;
            panel.transform.SetParent(contentContainer, false);
            _contentPanels[subtabId] = panel;
        }

        /// <summary>Get the content panel for a subtab.</summary>
        public GameObject GetContentPanel(string subtabId)
        {
            _contentPanels.TryGetValue(subtabId, out var panel);
            return panel;
        }

        /// <summary>Get the subtab button for a given subtab ID.</summary>
        public Button GetSubtabButton(string subtabId)
        {
            _subtabButtonMap.TryGetValue(subtabId, out var btn);
            return btn;
        }

        // ==================== Internals ====================

        /// <summary>
        /// Set a button's visual state to selected or unselected.
        /// Selected buttons remain interactable so users can always click a tab.
        /// </summary>
        protected void SetButtonSelected(Button btn, bool selected)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = selected ? selectedColor : normalColor;

            // Always keep buttons interactable — disabling the active button traps the user
            btn.interactable = true;
        }

        /// <summary>Create subtab buttons from the tab model.</summary>
        protected void CreateSubtabButtons(TabModel model)
        {
            if (subtabButtonContainer == null || subtabButtonPrefab == null) return;

            foreach (var subtabId in model.SubtabIds)
            {
                var btn = Instantiate(subtabButtonPrefab, subtabButtonContainer);
                btn.gameObject.SetActive(true);

                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = subtabId;

                // Highlight the initially active subtab
                SetButtonSelected(btn, subtabId == model.ActiveSubtab);

                var capturedId = subtabId;
                btn.onClick.AddListener(() => OnSubtabClicked?.Invoke(capturedId));

                _subtabButtonMap[subtabId] = btn;
            }

            // Show the initially active content panel if already registered
            if (!string.IsNullOrEmpty(model.ActiveSubtab) && _contentPanels.Count > 0)
            {
                foreach (var kvp in _contentPanels)
                    kvp.Value.SetActive(kvp.Key == model.ActiveSubtab);
            }
        }
    }
}
