using System;
using UnityEngine;

public class EventBus : InstanceToCLass<EventBus>
{
    /// <summary>
    /// This Action work for player killed to enemy
    /// </summary>
    public event Action OnEnemyKilled;

    /// <summary>
    /// This Action work for player trigger to ýtem
    /// </summary>
    public event Action OnItemCollected;

    /// <summary>
    /// This Action work for player entered zone
    /// </summary>
    public event Action OnEnteredZone;

    /// <summary>
    /// This Action work for player finished dialogue
    /// </summary>
    public event Action OnNPCDialogueFinished;

    /// <summary>
    /// This Action work for Timer finished
    /// </summary>
    public event Action OnTimerExpired;

    /// <summary>
    /// This function callback work for Enemy Killed
    /// </summary>
    public void EnemyKilled() =>
        OnEnemyKilled?.Invoke();

    /// <summary>
    /// This function callback work for Player collect to ýtem
    /// </summary>
    public void ItemCollected() =>
        OnItemCollected?.Invoke();

    /// <summary>
    /// This function callback work for player entered to zone
    /// </summary>
    public void EnteredZone() =>
        OnEnteredZone?.Invoke();

    /// <summary>
    /// This function callback work for player finished to dialogue
    /// </summary>
    public void NPCDialogueFinished() =>
        OnNPCDialogueFinished?.Invoke();

    /// <summary>
    /// This function callback work for Time Finished
    /// </summary>
    public void TimerExpired() =>
        OnTimerExpired?.Invoke();
}
