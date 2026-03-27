using UnityEngine;

public class MonsterAnimationEvents : MonoBehaviour
{
    private MonsterBrain brain;

    private void Awake()
    {
        brain = GetComponent<MonsterBrain>();
        Debug.Log($"[MonsterAnimationEvents] Awake, brain={(brain != null ? brain.name : "null")}, self={name}");
    }

    public void BornOver()
    {
        Debug.Log("[MonsterAnimationEvents] BornOver");
        if (brain != null) brain.OnBornOver();
    }

    public void AtkEvent()
    {
        Debug.Log($"[MonsterAnimationEvents] AtkEvent, brain={(brain != null ? brain.name : "null")}");
        if (brain != null) brain.OnAttackEvent();
    }

    public void AttackOver()
    {
        Debug.Log("[MonsterAnimationEvents] AttackOver");
        if (brain != null) brain.OnAttackOver();
    }
}
