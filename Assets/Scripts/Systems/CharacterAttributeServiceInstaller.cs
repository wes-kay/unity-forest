using CharacterAttributes;
using Zenject;

public class CharacterAttributeServiceInstaller : MonoInstaller
{
     public override void InstallBindings()
    {
        Container.Bind<CharacterAttributeService>().AsSingle();
    }
}
