using System;
using System.Collections.Generic;
using SoftKitty;

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
