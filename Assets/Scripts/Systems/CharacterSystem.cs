// CharacterSystemService.cs
using System;
using System.IO;
using CharacterAttributes;
using SoftKitty;
using Zenject;

public interface ICharacterSystemService
{
    void Reload();
    void Save();
}

public class CharacterSystemService : ICharacterSystemService, IInitializable, IDisposable
{
    private const bool AutoSave = false;

    private readonly IPartyService _partyService;
    private readonly ICharacterRosterService _rosterService;
    private readonly CharacterAttributeService _characterAttributeService;

    public CharacterSystemService(
        IPartyService partyService,
        ICharacterRosterService rosterService,
        CharacterAttributeService characterAttributeService)
    {
        _partyService = partyService;
        _rosterService = rosterService;
        _characterAttributeService = characterAttributeService;
    }

    public void Initialize()
    {
        Reload();

        // Add default members — guarded by HasMember to avoid duplicates
        // if they were already saved from a previous session.
        if (!_partyService.HasMember("Test"))
            _partyService.AddMember("Test");
        if (!_partyService.HasMember("Eirik_Hrafnsson"))
            _partyService.AddMember("Eirik_Hrafnsson");
    }

    public void Reload()
    {
        _partyService.Load();
        _rosterService.Load();

        if (AutoSave && File.Exists(GameManager.GetFullSavePath("game.sav")))
        {
            GameManager.EntityManagerData.Load(GameManager.GetFullSavePath("game.sav"));
        }
    }

    public void Save()
    {
        _partyService.Save();
        _rosterService.Save();
    }

    public void Dispose()
    {
        Save();
    }
}