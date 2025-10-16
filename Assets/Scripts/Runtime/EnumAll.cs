using UnityEngine;

/// <summary>
/// all enums are here.
/// </summary>
public class EnumAll
{
    /// <summary>
    /// Mission Type Enum
    /// </summary>
    public enum MissionType
    {
        None,
        Fetch,
        Kill,
        Explore,
        Dialogue,
        Escort
    }

    /// <summary>
    /// CompletionStatus Enum
    /// </summary>
    public enum CompletionStatus
    {
        None,
        InProgress,
        Completed,
        Failed
    }
}
