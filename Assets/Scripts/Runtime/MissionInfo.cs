using UnityEngine;
using static EnumAll;
using System.Collections.Generic;

/// <summary>
/// This is a scriptable object. It holds data about the task.
/// </summary>

[CreateAssetMenu(fileName = "MissionInfo", menuName = "Mission Create/Item")]
public class MissionInfo : ScriptableObject
{
    /// <summary>
    /// This area contains basic data.
    /// </summary>
    [Header("Property Reference")]

    [field: SerializeField]
    public int ID { get; private set; }

    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public string Explain { get; private set; }

    [field: SerializeField]
    public bool IsComploted { get; set; }

    /// <summary>
    /// This field contains the values ​​of the task.
    /// </summary>
    [Header("Mission Reference")]

    [field: SerializeField]
    public MissionType m_MissionType { get; private set; }

    [field: SerializeField]
    public int LevelRequirement { get; private set; }

    [field: SerializeField]
    public MissionInfo PreviousLevels { get; private set; }

    [field: SerializeField]
    public MissionInfo NextLevels { get; private set; }

    /// <summary>
    /// This field includes earnings references as a result of the task.
    /// </summary>
    [Header("Prize Reference")]

    [field: SerializeField]
    public float XP { get; private set; }

    [field: SerializeField]
    public float Money { get; private set; }
}
