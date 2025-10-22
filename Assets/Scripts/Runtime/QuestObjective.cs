using UnityEngine;
using static EnumAll;

public class QuestObjective
{
    [field: SerializeField]
    public CompletionStatus completionStatus { get; private set; }
}