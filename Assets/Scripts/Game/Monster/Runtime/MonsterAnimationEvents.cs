using UnityEngine;

public class MonsterAnimationEvents : MonoBehaviour
{
    private MonsterEntity entity;

    private void Awake()
    {
        entity = GetComponent<MonsterEntity>();
    }

    // Called by animation event "BornOver"
    public void BornOver()
    {
        if (entity != null) entity.OnBornOver();
    }

    // Called by animation event "AtkEvent"
    public void AtkEvent()
    {
        if (entity != null) entity.OnAttackEvent();
    }
}
