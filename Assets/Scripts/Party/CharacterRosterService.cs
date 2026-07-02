using System;
using System.Collections.Generic;
using SoftKitty;
using UnityEngine;

/// <summary>
/// Contract for managing the player's character roster — all characters the player has access to.
/// Distinct from the active party; the roster is the full collection of known/unlocked characters.
/// </summary>
public interface ICharacterRosterService
{
    /// <summary>
    /// Fired whenever the roster changes (add, remove).
    /// </summary>
    event Action OnRosterChanged;

    /// <summary>
    /// Current number of characters in the roster.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Whether the entity with the given UID is in the roster.
    /// </summary>
    bool HasCharacter(string uid);

    /// <summary>
    /// Add a character to the roster by entity UID.
    /// Does nothing if the character is already in the roster or UID is null/empty.
    /// </summary>
    void AddCharacter(string uid);

    /// <summary>
    /// Remove a character from the roster by entity UID.
    /// Does nothing if the character is not in the roster.
    /// </summary>
    void RemoveCharacter(string uid);

    /// <summary>
    /// Get the roster characters as Entity objects.
    /// Entities not found in the EntityManager are skipped.
    /// </summary>
    List<Entity> GetCharacters();

    /// <summary>
    /// Get the entity UID at the given index.
    /// </summary>
    string GetCharacterUidAt(int index);

    /// <summary>
    /// Get the Entity at the given index.
    /// </summary>
    Entity GetCharacterAt(int index);

    /// <summary>
    /// Remove all characters from the roster.
    /// </summary>
    void Clear();

    /// <summary>
    /// Save the roster UIDs to persistent storage.
    /// </summary>
    void Save();

    /// <summary>
    /// Load the roster UIDs from persistent storage.
    /// </summary>
    void Load();
}


/// <summary>
/// Zenject-injected service that manages the player's character roster.
/// Stores member UIDs (not Entity objects) for Easy Save serialization.
/// Roster starts empty — characters must be added explicitly.
/// </summary>
public class CharacterRosterService : ICharacterRosterService
{
    private const string SaveKey = "character_roster";

    /// List of entity UIDs the player has access to.
    private List<string> _rosterUids = new List<string>();

    public event Action OnRosterChanged;

    public int Count => _rosterUids.Count;

    public bool HasCharacter(string uid)
    {
        return uid != null && _rosterUids.Contains(uid);
    }

    public void AddCharacter(string uid)
    {
        if (string.IsNullOrEmpty(uid) || HasCharacter(uid))
            return;

        _rosterUids.Add(uid);
        OnRosterChanged?.Invoke();
    }

    public void RemoveCharacter(string uid)
    {
        if (_rosterUids.Remove(uid))
            OnRosterChanged?.Invoke();
    }

    public List<Entity> GetCharacters()
    {
        var characters = new List<Entity>();
        for (int i = 0; i < _rosterUids.Count; i++)
        {
            Entity entity = GameManager.GetEntity(_rosterUids[i]);
            if (entity != null)
                characters.Add(entity);
        }
        return characters;
    }

    public string GetCharacterUidAt(int index)
    {
        return index >= 0 && index < _rosterUids.Count ? _rosterUids[index] : null;
    }

    public Entity GetCharacterAt(int index)
    {
        if (index >= 0 && index < _rosterUids.Count)
            return GameManager.GetEntity(_rosterUids[index]);
        return null;
    }

    public void Clear()
    {
        _rosterUids.Clear();
        OnRosterChanged?.Invoke();
    }

    public void Save()
    {
        try
        {
            ES3.Save(SaveKey, _rosterUids);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CharacterRosterService] Failed to save roster: " + e.Message);
        }
    }

    public void Load()
    {
        try
        {
            _rosterUids = ES3.Load<List<string>>(SaveKey, new List<string>());
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CharacterRosterService] Failed to load roster: " + e.Message);
            _rosterUids = new List<string>();
        }
    }
}
