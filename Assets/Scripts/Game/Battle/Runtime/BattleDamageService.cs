using UnityEngine;

public class BattleDamageService
{
    private static readonly BattleDamageService instance = new BattleDamageService();
    public static BattleDamageService Instance => instance;

    private BattleDamageService() { }

    public DamageResult ApplyDamage(DamageRequest request)
    {
        var result = new DamageResult
        {
            success = false,
            target = request != null ? request.target : null,
            finalDamage = 0,
            isKilled = false,
            hitWorldPosition = request != null ? request.hitWorldPosition : Vector3.zero
        };

        if (request == null)
        {
            Debug.LogWarning("[BattleDamageService] request is null.");
            return result;
        }

        if (request.target == null)
        {
            Debug.LogWarning("[BattleDamageService] target is null.");
            return result;
        }

        if (request.target.IsDead)
        {
            Debug.Log("[BattleDamageService] target already dead.");
            return result;
        }

        if (request.rawDamage <= 0)
        {
            Debug.LogWarning("[BattleDamageService] rawDamage <= 0.");
            return result;
        }

        int finalDamage = request.rawDamage;

        request.target.TakeDamage(finalDamage);

        result.success = true;
        result.finalDamage = finalDamage;
        result.isKilled = request.target == null || request.target.IsDead;
        result.target = request.target;
        result.hitWorldPosition = request.hitWorldPosition;

        EventBus.Publish(new DamageAppliedEvent(result));

        return result;
    }
}
