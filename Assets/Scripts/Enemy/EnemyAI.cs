using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform Player;
    public float ChaseDistance = 5f;
    public float AttackDistance = 1.5f;
    public float Speed = 2f;
    private IEnemyState _currentState;
    public readonly IdleState _idle = new IdleState();
    public readonly ChaseState _chase = new ChaseState();
    public readonly AttackState _attack = new AttackState();
    private void Start() =>
        _currentState = _idle;
    private void Update() =>
        _currentState.Tick(this);
    public void SetState(IEnemyState newState) =>
        _currentState = newState;
}
public interface IEnemyState
{
    void Tick(EnemyAI context);
}
public class IdleState : IEnemyState
{
    public void Tick(EnemyAI context)
    {
        float distance = (context.Player.position - context.transform.position).sqrMagnitude;
        if (distance < context.ChaseDistance * context.ChaseDistance)
            context.SetState(context.ChaseDistance < context.AttackDistance ? context._attack : context._chase);
    }
}
public class ChaseState : IEnemyState
{
    public void Tick(EnemyAI context)
    {
        float distance = (context.Player.position - context.transform.position).sqrMagnitude;
        if (distance < context.AttackDistance * context.AttackDistance)
            context.SetState(context._attack);
        else if (distance > context.ChaseDistance * context.ChaseDistance)
            context.SetState(context._idle);
        else
            context.transform.position = Vector3.MoveTowards(context.transform.position, context.Player.position, context.Speed * Time.deltaTime);
    }
}
public class AttackState : IEnemyState
{
    public void Tick(EnemyAI context)
    {
        float distance = (context.Player.position - context.transform.position).sqrMagnitude;
        if (distance > context.AttackDistance * context.AttackDistance)
            context.SetState(context._chase);
        Debug.Log("Enemy attacks player!");
    }
}