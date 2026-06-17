using System;
using System.Collections.Generic;
using SoftKitty;
using UnityEngine;

/// <summary>
/// Zenject-injected service that manages the player's party.
/// Stores member UIDs (not Entity objects) for Easy Save serialization.
/// Party starts empty — members must be added explicitly.
/// </summary>
public class PartyService : IPartyService
{
    private const string SaveKey = "party_members";

    /// List of entity UIDs currently in the party.
    private List<string> _memberUids = new List<string>();

    public event Action OnPartyChanged;

    public int Count => _memberUids.Count;

    public bool HasMember(string uid)
    {
        return uid != null && _memberUids.Contains(uid);
    }

    public void AddMember(string uid)
    {
        if (string.IsNullOrEmpty(uid) || HasMember(uid))
            return;

        _memberUids.Add(uid);
        OnPartyChanged?.Invoke();
    }

    public void RemoveMember(string uid)
    {
        if (_memberUids.Remove(uid))
            OnPartyChanged?.Invoke();
    }

    public List<Entity> GetMembers()
    {
        var members = new List<Entity>();
        for (int i = 0; i < _memberUids.Count; i++)
        {
            Entity entity = GameManager.GetEntity(_memberUids[i]);
            if (entity != null)
                members.Add(entity);
        }
        return members;
    }

    public string GetMemberUidAt(int index)
    {
        return index >= 0 && index < _memberUids.Count ? _memberUids[index] : null;
    }

    public Entity GetMemberAt(int index)
    {
        if (index >= 0 && index < _memberUids.Count)
            return GameManager.GetEntity(_memberUids[index]);
        return null;
    }

    public void Clear()
    {
        _memberUids.Clear();
        OnPartyChanged?.Invoke();
    }

    public void Save()
    {
        try
        {
            // ES3 is provided by Easy Save 3 package.
            // If Easy Save is not installed yet, this will compile once the package is added.
            ES3.Save(SaveKey, _memberUids);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PartyService] Failed to save party: " + e.Message);
        }
    }

    public void Load()
    {
        try
        {
            // ES3 is provided by Easy Save 3 package.
            _memberUids = ES3.Load<List<string>>(SaveKey, new List<string>());
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PartyService] Failed to load party: " + e.Message);
            _memberUids = new List<string>();
        }
    }
}
