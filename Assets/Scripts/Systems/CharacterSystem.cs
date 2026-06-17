using System.Collections;
using System.IO;
using CharacterAttributes;
using PolyAndCode.UI;
using SoftKitty;
using SoftKitty.InventoryEngine;
using UnityEngine;
using Zenject;

public class CharacterSystem : MonoBehaviour
{
    [SerializeField] PartyListPanel partyListPanel;

    bool AutoSave = false;

    [Inject] IPartyService partyService;
    [Inject] ICharacterRosterService rosterService;
    [Inject] CharacterAttributeService characterAttributeService;

    IEnumerator Start()
    {
        // Load saved party and roster
        partyService.Load();
        rosterService.Load();

        if (AutoSave)
        {
            if (File.Exists(GameManager.GetFullSavePath("game.sav")))
            {
                GameManager.EntityManagerData.Load(GameManager.GetFullSavePath("game.sav"));
            }
        }

        yield return 1;

        // Debug.Log(characterAttributeService.GetCurrentVitality(GameManager.GetEntity("Test")));
        // GameManager.GetEntity("Test").GetModule<InventoryModule>().GetInventory().OpenWindow();
        // GameManager.EntityManagerData.Save("myGame.sav");
        partyService.AddMember("Test");
            partyListPanel.Show();
    }

    private void OnDestroy()
    {
        partyService.Save();
        rosterService.Save();
    }
}
