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
        currentStatus = status;
    }

    /// <summary>
    /// Ensures that the task is registered.
    /// </summary>
    /// <param name="saveMisson"> write to message from param </param>
    public void SaveMission(string save)
    {
        SaveMissionData.Save(save);
    }

    /// <summary>
    /// Ensures that the task is registered.
    /// </summary>
    public void LoadMission()
    {
        string a = SaveMissionData.Load<QuestInstance>(this);
    }
}
