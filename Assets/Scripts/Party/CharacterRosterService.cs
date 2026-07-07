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
    /// Check whether the given character can be added to the roster based on the provided conditions.
    /// Conditions are checked against the current game state — if no conditions are provided,
    /// the character is always considered addable.
    /// </summary>
    bool CanAddCharacter(string uid, List<QuestCondition> conditions);

    /// <summary>
    /// Add a character to the roster by entity UID.
    /// Does nothing if the character is already in the roster or UID is null/empty.
    /// </summary>
    void AddCharacter(string uid);

    /// <summary>
    /// Add a character to the roster by entity UID, with unlock conditions.
    /// Only adds if all conditions are met. Does nothing if the character is already in the roster
    /// or UID is null/empty.
    /// </summary>
    void AddCharacter(string uid, List<QuestCondition> conditions);

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
    /// Get the portrait sprite for the roster character at the given index.
    /// Returns null for a default icon.
    /// </summary>
    Sprite GetCharacterPortrait(int index);

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

    public bool CanAddCharacter(string uid, List<QuestCondition> conditions)
    {
        if (string.IsNullOrEmpty(uid) || HasCharacter(uid))
            return false;

        // No conditions means always addable
        if (conditions == null || conditions.Count == 0)
            return true;

        Entity entity = GameManager.GetEntity(uid);
        if (entity == null)
        {
            Debug.LogWarning($"[CharacterRosterService] Cannot check conditions for unknown entity '{uid}'");
            return false;
        }

        return CheckConditions(entity, conditions);
    }

    /// <summary>
    /// Check all conditions against an entity's current state.
    /// Returns true if all conditions are met (or conditions is null/empty).
    /// </summary>
    private bool CheckConditions(Entity entity, List<QuestCondition> conditions)
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            QuestCondition condition = conditions[i];

            // KillCondition — check if target entity type has been defeated (requires a kill tracker service)
            if (condition is KillCondition killCond)
            {
                // TODO: Query a kill-count service for killCond.targetEntityUid >= killCond.requiredProgress
                // For now, default to true (unconditional) until a kill tracker is available
            }

            // CollectCondition — check if player has collected enough of an item
            if (condition is CollectCondition collectCond)
            {
                // TODO: Query an inventory/service for collectCond.itemUid count >= collectCond.requiredProgress
                // For now, default to true (unconditional) until an inventory tracker is available
            }

            // AttributeCondition — check if an entity's attribute meets the threshold
            if (condition is AttributeCondition attrCond)
            {
                Entity target = GameManager.GetEntity(attrCond.targetEntityUid);
                if (target != null)
                {
                    float value = target.GetAttributeFloat(attrCond.attributeUid);
                    if (value < attrCond.requiredProgress)
                        return false;
                }
                // If the target entity can't be resolved, allow the add (don't block on missing data)
            }

            // SceneCondition — check if a scene has been visited
            if (condition is SceneCondition sceneCond)
            {
                // TODO: Query a scene-visit tracker for sceneCond.targetSceneName visited
                // For now, default to true (unconditional) until a scene tracker is available
            }
        }

        return true;
    }

    public void AddCharacter(string uid)
    {
        AddCharacter(uid, null);
    }

    public void AddCharacter(string uid, List<QuestCondition> conditions)
    {
        if (string.IsNullOrEmpty(uid) || HasCharacter(uid))
            return;

        // Check conditions if provided
        if (conditions != null && conditions.Count > 0)
        {
            Entity entity = GameManager.GetEntity(uid);
            if (entity == null)
            {
                Debug.LogWarning($"[CharacterRosterService] Cannot add '{uid}' — entity not found in EntityManager");
                return;
            }

            if (!CheckConditions(entity, conditions))
            {
                Debug.Log($"[CharacterRosterService] Cannot add '{uid}' — conditions not yet met");
                return;
            }
        }

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

    public Sprite GetCharacterPortrait(int index)
    {
        if (index < 0 || index >= _rosterUids.Count) return null;
        var entity = GameManager.GetEntity(_rosterUids[index]);
        if (entity == null) return null;

        // Try loading portrait from Resources/characters/portrait/{uid}
        string portraitPath = $"characters/portrait/{entity.uid}";
        return Resources.Load<Sprite>(portraitPath);
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
