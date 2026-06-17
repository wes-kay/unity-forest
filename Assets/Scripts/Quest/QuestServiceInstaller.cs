using UnityEngine;
using Zenject;

/// <summary>
/// Zenject installer for the QuestService.
/// Place quest SO assets in Assets/Resources/Quests/ for auto-discovery.
/// To use: drag this script onto a GameObject in the scene.
/// </summary>
public class QuestServiceInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Follows the project's standard installer pattern: bind interface to concrete class.
        // QuestService has [Inject] on Initialize() so it auto-loads quest definitions.
        Container.Bind<IQuestService>().To<QuestService>().AsSingle();
    }
}
