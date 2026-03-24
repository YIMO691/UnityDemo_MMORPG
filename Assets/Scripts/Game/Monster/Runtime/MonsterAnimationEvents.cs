using UnityEngine;

public class MonsterAnimationEvents : MonoBehaviour
{
    private MonsterBrain brain;

    private void Awake()
    {
        brain = GetComponent<MonsterBrain>();
    }

    public void BornOver()
    {
        if (brain != null) brain.OnBornOver();
    }

    public void AtkEvent()
    {
        if (brain != null) brain.OnAttackEvent();
    }
    public void AttackOver()
    {
        if (brain != null) brain.OnAttackOver();
    }
}
