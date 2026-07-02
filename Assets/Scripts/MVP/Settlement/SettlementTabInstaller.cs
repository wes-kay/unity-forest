using Zenject;
using Domain.MVP.Settlement;

namespace Domain.Installers
{
    /// <summary>
    /// Zenject installer for the Settlement tab MVP.
    /// Registers the model, view, and presenter so they can be injected into HubPresenter.
    /// </summary>
    public class SettlementTabInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Model — instantiated directly with mock data
            var model = new SettlementTabModel();
            Container.BindInstance(model).AsSingle();
            Container.Bind<SettlementTabModel>().FromInstance(model).AsSingle();

            // View — resolve from scene or create new component
            Container.Bind<SettlementTabView>().FromNewComponentOnRoot().AsSingle();

            // Presenter — constructor injection gets model + view
            Container.Bind<SettlementTabPresenter>().FromNew().AsSingle();
        }
    }
}
