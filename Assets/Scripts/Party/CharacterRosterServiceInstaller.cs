using Zenject;

/// <summary>
/// Zenject installer for the CharacterRosterService.
/// Bind the service as a singleton and auto-inject it.
/// </summary>
public class CharacterRosterServiceInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<ICharacterRosterService>().To<CharacterRosterService>().AsSingle();
        // Container.Bind<CharacterRosterService>().AsSingle();
    }
}
