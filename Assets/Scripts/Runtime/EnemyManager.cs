using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    List<EnemyKilledEventArgs> enemyKilledEventArgs;

    private void Start()
    {
        enemyKilledEventArgs.Add(new EnemyKilledEventArgs(10,10));
    }
}

struct EnemyKilledEventArgs
{
    public int enemyID;
    public float xpReward;

    public EnemyKilledEventArgs(int _enemyID, float _xpReward)
    {
        enemyID = _enemyID;
        xpReward = _xpReward;
    }
}
