using System;
using System.Collections.Generic;
using SoftKitty;

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
