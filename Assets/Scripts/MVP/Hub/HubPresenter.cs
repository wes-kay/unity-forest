using UnityEngine;
using Zenject;
using Domain.MVP.Tab;
using UnityEngine.InputSystem;

namespace Domain.MVP.Hub
{
    public class HubPresenter : MonoBehaviour
    {
        [Inject] private HubModel      _model;
        [Inject] private HubView       _view;
        [Inject] private TabPresenter[] _tabPresenters;

        [Header("Presentation Settings")]
        public float fadeDuration = 0.4f;

        private bool         _isFading;
        private bool         _hubVisible;
        private TabPresenter _activePresenter;
        private string       _activeTabId;

        public bool   IsVisible   => _hubVisible;
        public string ActiveTabId => _activeTabId;

        public TabPresenter GetTabPresenter(string tabId)
        {
            foreach (var p in _tabPresenters)
                if (p != null && p.TabId == tabId) return p;
            return null;
        }

        public T GetTabPresenter<T>() where T : TabPresenter
        {
            foreach (var p in _tabPresenters)
                if (p is T typed) return typed;
            return null;
        }

        [Inject]
        public void Initialize()
        {
            foreach (var presenter in _tabPresenters)
            {
                if (presenter == null) continue;
                RegisterPresenter(presenter);
            }

            _view.OnTabClicked   += OnViewTabClicked;
            _view.OnCloseClicked += OnCloseClicked;
            _model.OnTabChanged  += OnModelTabChanged;

            // Activate the first tab
            if (!string.IsNullOrEmpty(_model.ActiveTabId))
            {
                _view.SetActiveTab(_model.ActiveTabId);
                _activeTabId      = _model.ActiveTabId;
                _activePresenter  = GetTabPresenter(_model.ActiveTabId);
                _activePresenter?.Activate();
            }
        }

        public void Update()
        {
            if (Keyboard.current != null && Keyboard.current[Key.Enter].wasPressedThisFrame)
                ToggleVisibility();
        }

        private void RegisterPresenter(TabPresenter presenter)
        {
            if (presenter == null || presenter.Model == null) return;

            // The presenter's root GameObject IS the tab content panel.
            // Parent it under the hub's content container so it lives in
            // the correct place in the hierarchy, then hide it by default.
            var tabGO = presenter.gameObject;
            tabGO.transform.SetParent(_view.contentContainer, false);
            tabGO.SetActive(false);

            _model.RegisterTab(presenter.Model);
            // Pass the root GO as the content panel — HubView stores and manages it.
            _view.RegisterTab(presenter.Model.TabId, presenter.Model.TabName, tabGO);
            presenter.Initialize();
        }

        // ==================== Event Handlers ====================

        private void OnViewTabClicked(string tabId)
        {
            if (_isFading) return;
            SwitchTab(tabId);
        }

        private void OnCloseClicked()
        {
            if (_isFading) return;
            ToggleVisibility();
        }

        private void OnModelTabChanged(string oldTabId, string newTabId) { }

        // ==================== Public API ====================

        public void SwitchTab(string tabId)
        {
            if (!_model.HasTab(tabId) || _model.ActiveTabId == tabId) return;

            _activePresenter?.Deactivate();
            _activePresenter = null;

            _model.SetActiveTab(tabId);
            _view.SetActiveTab(tabId);

            _activeTabId     = tabId;
            _activePresenter = GetTabPresenter(tabId);
            _activePresenter?.Activate();
        }

        public void ToggleVisibility()
        {
            if (_isFading) return;
            if (_hubVisible) StartCoroutine(FadeOutAndHide());
            else             StartCoroutine(FadeInAndShow());
        }

        public void ShowImmediately()
        {
            _hubVisible = true;
            _model.Show();
            _view.ShowHub();

            if (!string.IsNullOrEmpty(_model.ActiveTabId))
            {
                _view.SetActiveTab(_model.ActiveTabId);
                _activeTabId     = _model.ActiveTabId;
                _activePresenter = GetTabPresenter(_model.ActiveTabId);
                _activePresenter?.Activate();
            }
        }

        public void HideImmediately()
        {
            _hubVisible = false;
            _model.Hide();
            _view.HideHub();
            _activePresenter?.Deactivate();
            _activePresenter = null;
        }

        // ==================== Fades ====================

        private System.Collections.IEnumerator FadeOutAndHide()
        {
            _isFading = true;
            float startAlpha = _view.canvasGroup != null ? _view.canvasGroup.alpha : 1f;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeDuration;
                if (_view.canvasGroup != null)
                    _view.canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            if (_view.canvasGroup != null)
            {
                _view.canvasGroup.alpha          = 0f;
                _view.canvasGroup.interactable   = false;
                _view.canvasGroup.blocksRaycasts = false;
            }

            _hubVisible = false;
            _model.Hide();
            _activePresenter?.Deactivate();
            _isFading = false;
        }

        private System.Collections.IEnumerator FadeInAndShow()
        {
            _isFading = true;
            _view.gameObject.SetActive(true);

            if (_view.canvasGroup != null)
            {
                _view.canvasGroup.alpha          = 0f;
                _view.canvasGroup.interactable   = true;
                _view.canvasGroup.blocksRaycasts = true;
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeDuration;
                if (_view.canvasGroup != null)
                    _view.canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            if (_view.canvasGroup != null)
                _view.canvasGroup.alpha = 1f;

            _hubVisible = true;
            _model.Show();

            if (_activePresenter == null && !string.IsNullOrEmpty(_activeTabId))
            {
                _activePresenter = GetTabPresenter(_activeTabId);
                _activePresenter?.Activate();
            }

            _isFading = false;
        }
    }
}
