/// <summary>
/// Possible states for a quest. Transitions follow: Available → Active → Completed or Failed.
/// </summary>
public enum QuestState
{
    Available,
    Active,
    Completed,
    Failed
}
