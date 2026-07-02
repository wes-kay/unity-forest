using System;
using System.Collections.Generic;
using Domain.MVP.Tab;
using UnityEngine;

namespace Domain.MVP.Hub
{
    /// <summary>
    /// Model for the main UI hub. Manages visibility, tab registration, and active tab state.
    /// </summary>
    public class HubModel
    {
        /// <summary>Whether the hub is currently visible.</summary>
        public bool IsVisible { get; private set; }

        /// <summary>Currently active tab identifier.</summary>
        public string ActiveTabId { get; private set; }

        /// <summary>Currently active tab model (convenience accessor).</summary>
        public TabModel ActiveTabModel { get; private set; }

        /// <summary>Fired when the hub visibility changes.</summary>
        public event Action<bool> OnVisibilityChanged;

        /// <summary>Fired when the active tab changes. Returns (oldTabId, newTabId).</summary>
        public event Action<string, string> OnTabChanged;

        private readonly Dictionary<string, TabModel> _registeredTabs = new Dictionary<string, TabModel>();
        private readonly Dictionary<string, GameObject> _tabContentPanels = new Dictionary<string, GameObject>();

        /// <summary>Get the list of registered tab IDs.</summary>
        public string[] GetRegisteredTabIds()
        {
            var ids = new string[_registeredTabs.Count];
            _registeredTabs.Keys.CopyTo(ids, 0);
            return ids;
        }

        /// <summary>Register a tab with the hub.</summary>
        public void RegisterTab(TabModel tab)
        {
            if (_registeredTabs.ContainsKey(tab.TabId))
            {
                Debug.LogWarning($"HubModel: Tab '{tab.TabId}' is already registered.");
                return;
            }
            _registeredTabs[tab.TabId] = tab;

            if (string.IsNullOrEmpty(ActiveTabId) && tab.SubtabIds.Length > 0)
            {
                ActiveTabId = tab.TabId;
                ActiveTabModel = tab;
            }
        }

        /// <summary>Register a content panel for a tab.</summary>
        public void RegisterContentPanel(string tabId, GameObject panel)
        {
            _tabContentPanels[tabId] = panel;
        }

        /// <summary>Get the content panel for a tab.</summary>
        public GameObject GetContentPanel(string tabId)
        {
            _tabContentPanels.TryGetValue(tabId, out var panel);
            return panel;
        }

        /// <summary>Switch to a tab by ID. Returns the previous tab ID.</summary>
        public string SetActiveTab(string tabId)
        {
            if (!_registeredTabs.ContainsKey(tabId))
            {
                Debug.LogWarning($"HubModel: Tab '{tabId}' is not registered.");
                return ActiveTabId;
            }

            var oldTabId = ActiveTabId;
            ActiveTabId = tabId;
            ActiveTabModel = _registeredTabs[tabId];

            OnTabChanged?.Invoke(oldTabId, tabId);
            return oldTabId;
        }

        /// <summary>Toggle hub visibility.</summary>
        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
            OnVisibilityChanged?.Invoke(IsVisible);
        }

        /// <summary>Show the hub.</summary>
        public void Show()
        {
            if (IsVisible) return;
            IsVisible = true;
            OnVisibilityChanged?.Invoke(true);
        }

        /// <summary>Hide the hub.</summary>
        public void Hide()
        {
            if (!IsVisible) return;
            IsVisible = false;
            OnVisibilityChanged?.Invoke(false);
        }

        /// <summary>Get the tab model for a tab ID.</summary>
        public TabModel GetTabModel(string tabId)
        {
            _registeredTabs.TryGetValue(tabId, out var tab);
            return tab;
        }

        /// <summary>Check if a tab is registered.</summary>
        public bool HasTab(string tabId)
        {
            return _registeredTabs.ContainsKey(tabId);
        }
    }
}
