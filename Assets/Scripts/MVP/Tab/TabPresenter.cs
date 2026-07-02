using UnityEngine;
using Zenject;

namespace Domain.MVP.Tab
{
    // Non-generic root — keeps TabPresenter[] arrays working in HubPresenter.
    public abstract class TabPresenter : MonoBehaviour
    {
        private bool _isActive;

        public bool IsActive => _isActive;
        public abstract string TabId { get; }
        public abstract TabModel Model { get; }

        public abstract void OnTabActivated();
        public abstract void OnTabDeactivated();
        public abstract void OnSubtabChanged(string subtabId);
        public abstract void Initialize();

        public void Activate()
        {
            if (_isActive) return;
            _isActive = true;
            Model?.Activate();
            OnTabActivated();
        }

        public void Deactivate()
        {
            if (!_isActive) return;
            _isActive = false;
            Model?.Deactivate();
            OnTabDeactivated();
        }
    }

    // Generic middle class — concrete tabs extend this, Zenject injects the right types.
    public abstract class TabPresenter<TModel, TView> : TabPresenter
        where TModel : TabModel
        where TView  : TabView
    {
        [Inject] protected TModel _model;
        [Inject] protected TView  _view;

        public override string   TabId => _model?.TabId ?? string.Empty;
        public override TabModel Model => _model;

        public override void Initialize()
        {
            _model.OnSubtabChanged += OnModelSubtabChanged;
            _view.OnSubtabClicked  += OnViewSubtabClicked;
            _view.Initialize(_model);
        }

        public virtual void Destroy()
        {
            if (_model != null) _model.OnSubtabChanged -= OnModelSubtabChanged;
            if (_view  != null) _view.OnSubtabClicked  -= OnViewSubtabClicked;
        }

        private void OnModelSubtabChanged(string subtabId)
        {
            _view?.ShowSubtab(subtabId);
            OnSubtabChanged(subtabId);
        }

        private void OnViewSubtabClicked(string subtabId)
        {
            _model.SetActiveSubtab(subtabId);
            // OnSubtabChanged fires via OnModelSubtabChanged — no double-call.
        }
    }
}
