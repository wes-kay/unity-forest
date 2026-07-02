using System;

namespace Domain.MVP.Tab
{
    /// <summary>
    /// Base model for a hub tab. Holds metadata and subtab state.
    /// Concrete tabs extend this to add domain-specific data.
    /// </summary>
    public abstract class TabModel
    {
        /// <summary>Unique identifier for this tab.</summary>
        public string TabId { get; }

        /// <summary>Display name shown in the tab bar.</summary>
        public string TabName { get; }

        /// <summary>Available subtab identifiers for this tab.</summary>
        public string[] SubtabIds { get; }

        /// <summary>Currently active subtab.</summary>
        public string ActiveSubtab { get; protected set; }

        /// <summary>Whether this tab's data has been loaded.</summary>
        public bool IsLoaded { get; protected set; }

        /// <summary>Fired when the active subtab changes.</summary>
        public event Action<string> OnSubtabChanged;

        /// <summary>Fired when data is refreshed.</summary>
        public event Action OnDataChanged;

        protected TabModel(string tabId, string tabName, string[] subtabIds)
        {
            TabId = tabId;
            TabName = tabName;
            SubtabIds = subtabIds;
            ActiveSubtab = subtabIds.Length > 0 ? subtabIds[0] : string.Empty;
        }

        /// <summary>Activate this tab — called when it becomes the active tab.</summary>
        public virtual void Activate()
        {
            if (!IsLoaded)
            {
                LoadFromService();
                IsLoaded = true;
            }
        }

        /// <summary>Deactivate this tab — called when it loses focus.</summary>
        public virtual void Deactivate() { }

        /// <summary>Switch to a subtab by identifier.</summary>
        public void SetActiveSubtab(string subtabId)
        {
            if (ActiveSubtab == subtabId) return;
            if (System.Array.IndexOf(SubtabIds, subtabId) < 0)
            {
                throw new ArgumentException($"Subtab '{subtabId}' is not valid for tab '{TabId}'.", nameof(subtabId));
            }
            ActiveSubtab = subtabId;
            OnSubtabChanged?.Invoke(subtabId);
        }

        /// <summary>Load data from backing services. Override in concrete implementations.</summary>
        public abstract void LoadFromService();
    }
}
