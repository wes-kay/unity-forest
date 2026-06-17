using System;
using UnityEngine;

/// <summary>
/// Abstract base for quest conditions. Each concrete type checks a specific requirement.
/// </summary>
[Serializable]
public abstract class QuestCondition
{
    /// <summary>
    /// The threshold value this condition checks against.
    /// </summary>
    public abstract int RequiredProgress { get; }

    /// <summary>
    /// Check if this condition is met given the current progress at the given index.
    /// </summary>
    public bool IsMet(QuestProgress progress, int index)
    {
        return progress.currentProgress[index] >= RequiredProgress;
    }

    public abstract string GetDescription();
}

/// <summary>
/// Kill/Defeat condition — defeat X entities of a specific type.
/// </summary>
[Serializable]
public class KillCondition : QuestCondition
{
    [Tooltip("Entity UID of the enemy type to defeat")]
    public string targetEntityUid;

    [Tooltip("Number of entities to defeat")]
    public int requiredProgress;

    public override int RequiredProgress => requiredProgress;

    public override string GetDescription()
    {
        return "Defeat " + requiredProgress + " " + targetEntityUid;
    }
}

/// <summary>
/// Collect/Item condition — collect X amount of a specific item.
/// </summary>
[Serializable]
public class CollectCondition : QuestCondition
{
    [Tooltip("Item UID to collect")]
    public string itemUid;

    [Tooltip("Number of items to collect")]
    public int requiredProgress;

    public override int RequiredProgress => requiredProgress;

    public override string GetDescription()
    {
        return "Collect " + requiredProgress + " " + itemUid;
    }
}

/// <summary>
/// Level/Attribute condition — reach a certain level or attribute threshold.
/// </summary>
[Serializable]
public class AttributeCondition : QuestCondition
{
    [Tooltip("Entity UID whose attribute to check")]
    public string targetEntityUid;

    [Tooltip("Attribute UID (e.g. hp, attack, level)")]
    public string attributeUid;

    [Tooltip("Required attribute value")]
    public float requiredProgress;

    public override int RequiredProgress => Mathf.FloorToInt(requiredProgress);

    public override string GetDescription()
    {
        return "Reach " + requiredProgress + " " + attributeUid + " on " + targetEntityUid;
    }
}

/// <summary>
/// Scene/Exploration condition — visit a specific scene.
/// </summary>
[Serializable]
public class SceneCondition : QuestCondition
{
    [Tooltip("Scene name to visit")]
    public string targetSceneName;

    public override int RequiredProgress => 1; // Scene visit is binary: visited or not

    public override string GetDescription()
    {
        return "Visit " + targetSceneName;
    }
}
