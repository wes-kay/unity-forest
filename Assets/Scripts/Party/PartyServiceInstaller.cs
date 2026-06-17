using Zenject;

/// <summary>
/// Zenject installer for the PartyService.
/// Bind the service as a singleton and auto-inject it.
/// </summary>
public class PartyServiceInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IPartyService>().To<PartyService>().AsSingle();
        // Container.Bind<PartyService>().AsSingle();
    }
}
