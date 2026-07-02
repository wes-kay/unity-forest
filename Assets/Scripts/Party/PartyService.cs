using System;
using System.Collections.Generic;
using SoftKitty;
using UnityEngine;
/// <summary>
/// Contract for managing the player's party — adding, removing, querying members.
/// Party starts empty; members are added by entity UID and resolved to Entity objects.
/// </summary>
public interface IPartyService
{
    /// <summary>
    /// Fired whenever the party changes (add, remove, clear).
    /// </summary>
    event Action OnPartyChanged;

    /// <summary>
    /// Current number of party members.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Whether the entity with the given UID is in the party.
    /// </summary>
    bool HasMember(string uid);

    /// <summary>
    /// Add a party member by entity UID. Resolves to an Entity at runtime.
    /// Does nothing if the member is already in the party or UID is null/empty.
    /// </summary>
    void AddMember(string uid);

    /// <summary>
    /// Remove a party member by entity UID.
    /// Does nothing if the member is not in the party.
    /// </summary>
    void RemoveMember(string uid);

    /// <summary>
    /// Get the current party members as Entity objects.
    /// Returns null if the entity for a UID cannot be resolved.
    /// </summary>
    List<Entity> GetMembers();

    /// <summary>
    /// Get the entity UID at the given index.
    /// </summary>
    string GetMemberUidAt(int index);

    /// <summary>
    /// Get the Entity at the given index.
    /// </summary>
    Entity GetMemberAt(int index);

    /// <summary>
    /// Get the portrait sprite for the party member at the given index.
    /// Returns null for a default icon.
    /// </summary>
    Sprite GetMemberPortrait(int index);

    /// <summary>
    /// Remove all members from the party.
    /// </summary>
    void Clear();

    /// <summary>
    /// Save the party member UIDs to persistent storage.
    /// </summary>
    void Save();

    /// <summary>
    /// Load the party member UIDs from persistent storage.
    /// </summary>
    void Load();
}


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

    public Sprite GetMemberPortrait(int index)
    {
        if (index < 0 || index >= _memberUids.Count) return null;
        var entity = GameManager.GetEntity(_memberUids[index]);
        if (entity == null) return null;

        var portraitUid = entity.GetAttributeString("portrait");
        if (string.IsNullOrEmpty(portraitUid)) return null;

        // TODO: Load sprite from Resources/Portraits/{portraitUid} or asset bundle
        // return Resources.Load<Sprite>(string.Format("Portraits/{0}", portraitUid));
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
