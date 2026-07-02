// GameSystemInstaller.cs
using CharacterAttributes;
using Zenject;

public class GameSystemInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<CharacterSystemService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PartyService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<CharacterRosterService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<CharacterAttributeService>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<GameSystem>().AsSingle().NonLazy();
    }
}