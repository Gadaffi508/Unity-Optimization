using UnityEngine;
using static EnumAll;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MissionInfo", menuName = "Mission Create/Item")]
public class MissionInfo : ScriptableObject
{
    [Header("Property Reference")]

    [field: SerializeField]
    public int ID { get; private set; }

    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public string Explain { get; private set; }

    [field: SerializeField]
    public bool IsComploted { get; set; }

    [Header("Mission Reference")]

    [field: SerializeField]
    public MissionType m_MissionType { get; private set; }

    [field: SerializeField]
    public int LevelRequirement { get; private set; }

    [field: SerializeField]
    public List<MissionInfo> PreviousLevels { get; private set; }

    [Header("Prize Reference")]

    [field: SerializeField]
    public float XP { get; private set; }

    [field: SerializeField]
    public float Money { get; private set; }
}
