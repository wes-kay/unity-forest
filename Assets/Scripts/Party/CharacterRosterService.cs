using System;
using System.Collections.Generic;
using SoftKitty;
using UnityEngine;

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
