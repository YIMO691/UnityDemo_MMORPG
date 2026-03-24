using UnityEngine;

public class DamageFeedbackListener : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
    }

    private void OnDamageApplied(DamageAppliedEvent e)
    {
        if (e == null || e.result == null) return;
        if (!e.result.success) return;
        DamageTextSpawner.Show(e.result.finalDamage, e.result.hitWorldPosition, null);
    }
}
