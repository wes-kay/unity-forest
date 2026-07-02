using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Domain.MVP.Hub
{
    public class HubView : MonoBehaviour
    {
        [Header("Tab Bar")]
        public RectTransform tabButtonContainer;
        public Button        tabButtonPrefab;

        [Header("Content Area")]
        public RectTransform contentContainer;

        [Header("Close Button")]
        public Button closeButton;

        [Header("Styling")]
        public Color normalTabColor  = new Color(0.12f, 0.12f, 0.12f, 1f);
        public Color activeTabColor  = new Color(0.55f, 0.12f, 0.08f, 1f);
        public Color normalTextColor = new Color(0.80f, 0.75f, 0.70f, 1f);
        public Color activeTextColor = new Color(1.00f, 0.90f, 0.85f, 1f);

        [Header("Canvas Group for fade")]
        public CanvasGroup canvasGroup;

        public event Action<bool>   OnVisibilityChanged;
        public event Action<string> OnTabClicked;
        public event Action         OnCloseClicked;

        private readonly Dictionary<string, Button>     _tabButtons    = new Dictionary<string, Button>();
        private readonly Dictionary<string, GameObject> _contentPanels = new Dictionary<string, GameObject>();
        private readonly List<string>                   _tabOrder      = new List<string>();

        private string _activeTabId;
        public  string ActiveTabId => _activeTabId;

        public void Awake()
        {
            canvasGroup = canvasGroup ?? GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha          = 0f;
                canvasGroup.interactable   = false;
                canvasGroup.blocksRaycasts = false;
            }
            // gameObject.SetActive(false);
        }

        public void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        /// <summary>
        /// Register a tab. <paramref name="contentPanel"/> must already be a child of
        /// contentContainer (HubPresenter.RegisterPresenter handles the reparenting).
        /// </summary>
        public void RegisterTab(string tabId, string tabName, GameObject contentPanel)
        {
            if (tabButtonContainer == null || tabButtonPrefab == null) return;

            // Create the tab button
            var btn  = Instantiate(tabButtonPrefab, tabButtonContainer);
            btn.gameObject.SetActive(true);

            var img  = btn.GetComponent<Image>();
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();

            if (img  != null) img.color  = normalTabColor;
            if (text != null) { text.text = tabName; text.color = normalTextColor; }

            var capturedId = tabId;
            btn.onClick.AddListener(() => OnTabClicked?.Invoke(capturedId));

            _tabButtons[tabId] = btn;
            _tabOrder.Add(tabId);

            // Store the panel reference; it's already parented and hidden by HubPresenter
            if (contentPanel != null)
                _contentPanels[tabId] = contentPanel;
        }

        /// <summary>Highlight the active tab button and show only its content panel.</summary>
        public void SetActiveTab(string tabId)
        {
            _activeTabId = tabId;

            foreach (var id in _tabOrder)
            {
                bool isActive = id == tabId;

                if (_tabButtons.TryGetValue(id, out var btn))
                {
                    var img  = btn.GetComponent<Image>();
                    var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (img  != null) img.color  = isActive ? activeTabColor  : normalTabColor;
                    if (text != null) text.color = isActive ? activeTextColor : normalTextColor;
                    btn.interactable = true;
                }

                if (_contentPanels.TryGetValue(id, out var panel))
                    panel.SetActive(isActive);
            }
        }

        public void ShowHub()
        {
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.interactable   = true;
                canvasGroup.blocksRaycasts = true;
            }
            OnVisibilityChanged?.Invoke(true);
        }

        public void HideHub()
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable   = false;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
            OnVisibilityChanged?.Invoke(false);
        }

        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null) canvasGroup.alpha = alpha;
        }

        public Button     GetTabButton    (string tabId) { _tabButtons.TryGetValue(tabId,    out var b); return b; }
        public GameObject GetContentPanel (string tabId) { _contentPanels.TryGetValue(tabId, out var p); return p; }
    }
}
