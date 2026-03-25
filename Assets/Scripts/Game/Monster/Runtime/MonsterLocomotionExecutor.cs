using UnityEngine;

[RequireComponent(typeof(MonsterEntity))]
[RequireComponent(typeof(MonsterNavigator))]
public class MonsterLocomotionExecutor : MonoBehaviour
{
    private MonsterEntity entity;
    private MonsterNavigator navigator;
    private MonsterAnimatorDriver anim;

    private void Awake()
    {
        entity = GetComponent<MonsterEntity>();
        navigator = GetComponent<MonsterNavigator>();
        ResolveAnimator();
    }

    private void ResolveAnimator()
    {
        if (anim == null)
        {
            anim = GetComponent<MonsterAnimatorDriver>();
        }
    }


public void Execute(ActorControlCommand command, Vector3? moveTarget = null, float stopDistance = 0.2f)
{
    ResolveAnimator();
    if (entity == null || entity.IsDead) return;

    if (command.attack)
    {
        navigator.StopNavigation();
        anim?.TriggerAttack();
        return;
    }

    if (command.stop)
    {
        navigator.StopNavigation();
        anim?.SetIdle();
        return;
    }

    if (moveTarget.HasValue)
    {
        navigator.MoveTo(moveTarget.Value, stopDistance);
    }

    float speed = navigator != null ? navigator.CurrentSpeed : 0f;
    if (command.moveDirection.sqrMagnitude > 0.0001f)
    {
        anim?.SetChase(speed);
    }
    else
    {
        anim?.SetIdle();
    }
}

    public void StopAll()
    {
        ResolveAnimator();
        navigator?.StopNavigation();
        anim?.SetIdle();
    }

    public void SetDead()
    {
        ResolveAnimator();
        navigator?.StopNavigation();
        anim?.SetDead();
    }
}
