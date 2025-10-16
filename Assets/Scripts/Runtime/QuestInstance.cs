using UnityEngine;
using static EnumAll;

/// <summary>
/// This script keeps track of tasks.
/// performs task switching.
/// saves the task.
/// </summary>
public class QuestInstance : InstanceToCLass<QuestInstance>
{
    [field: SerializeField]
    public MissionInfo currentMission { get; private set; }

    [field: SerializeField]
    public CompletionStatus currentStatus { get; private set; }

    [field: SerializeField]
    [Range(0, 120)]
    public float missionTime { get; private set; }

    /// <summary>
    /// performs next task switching.
    /// it already exists in the data.
    /// </summary>
    public void SetNextLevel()
    {
        currentMission = currentMission.NextLevels;
    }

    /// <summary>
    /// Changes the status of the current task.
    /// </summary>
    public void SetStatus(CompletionStatus status)
    {
        status = currentStatus;
    }

    /// <summary>
    /// Ensures that the task is registered.
    /// </summary>
    public void SaveMission()
    {

    }
}
