using UnityEngine;

public class ConditionChecker : MonoBehaviour
{
    public MissionInfo missionInfo;

    private void OnTriggerEnter(Collider other)
    {
        missionInfo.Evaluate();
        EventBus.Instance.ItemCollected<ConditionChecker>(this);

    }

    private void OnTriggerExit(Collider other)
    {
        
    }
}
