using System;
using UnityEngine;

public class EventBus : InstanceToCLass<EventBus>
{
    /// <summary>
    /// This Action work for player killed to enemy
    /// </summary>
    public event Action<object> OnEnemyKilled;

    /// <summary>
    /// This Action work for player trigger to ýtem
    /// </summary>
    public event Action<object> OnItemCollected;

    /// <summary>
    /// This Action work for player entered zone
    /// </summary>
    public event Action<object> OnEnteredZone;

    /// <summary>
    /// This Action work for player finished dialogue
    /// </summary>
    public event Action<object> OnNPCDialogueFinished;

    /// <summary>
    /// This Action work for Timer finished
    /// </summary>
    public event Action<object> OnTimerExpired;

    /// <summary>
    /// This function callback work for Enemy Killed
    /// </summary>
    public void EnemyKilled<T>(T t) =>
        OnEnemyKilled?.Invoke(t);

    /// <summary>
    /// This function callback work for Player collect to ýtem
    /// </summary>
    public void ItemCollected<T>(T t) =>
        OnItemCollected?.Invoke(t);

    /// <summary>
    /// This function callback work for player entered to zone
    /// </summary>
    public void EnteredZone<T>(T t) =>
        OnEnteredZone?.Invoke(t);

    /// <summary>
    /// This function callback work for player finished to dialogue
    /// </summary>
    public void NPCDialogueFinished<T>(T t) =>
        OnNPCDialogueFinished?.Invoke(t);

    /// <summary>
    /// This function callback work for Time Finished
    /// </summary>
    public void TimerExpired<T>(T t) =>
        OnTimerExpired?.Invoke(t);
}
