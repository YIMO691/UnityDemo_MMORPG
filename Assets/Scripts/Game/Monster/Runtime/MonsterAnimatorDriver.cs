using UnityEngine;

public class MonsterAnimatorDriver : MonoBehaviour
{
    public string paramSpeed = "Speed";
    public string paramIsChasing = "IsChasing";
    public string paramDead = "Dead";
    public string triggerAttack = "Attack";

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetIdle()
    {
        if (animator == null) return;
        animator.SetBool(paramIsChasing, false);
        animator.SetFloat(paramSpeed, 0f);
    }

    public void SetChase(float speed)
    {
        if (animator == null) return;
        animator.SetBool(paramIsChasing, true);
        animator.SetFloat(paramSpeed, speed);
    }

    public void TriggerAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(triggerAttack);
    }

    public void SetDead()
    {
        if (animator == null) return;
        animator.SetTrigger(paramDead);
        animator.SetFloat(paramSpeed, 0f);
    }
}
