// GameSystem.cs
using Zenject;

public class GameSystem : IInitializable
{
    public void Initialize()
    {
        // CharacterSystemService.Initialize() already calls Reload() via IInitializable.
        // Members are added after Reload() in that method, so no further setup needed here.
    }
}