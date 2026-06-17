/// <summary>
/// Tracks the runtime progress of a single quest.
/// </summary>
public class QuestProgress
{
    public QuestDefinitionSO questDefinition;
    public QuestState state;

    // Current progress for each condition (indexed the same as QuestDefinitionSO.conditions)
    public int[] currentProgress;

    public QuestProgress(QuestDefinitionSO definition)
    {
        questDefinition = definition;
        state = QuestState.Available;
        currentProgress = new int[definition.conditions.Count];
    }

    /// <summary>
    /// Check if all conditions are met.
    /// </summary>
    public bool IsComplete()
    {
        for (int i = 0; i < currentProgress.Length; i++)
        {
            if (i >= questDefinition.conditions.Count)
                return false;

            if (!questDefinition.conditions[i].IsMet(this, i))
                return false;
        }
        return true;
    }
}
