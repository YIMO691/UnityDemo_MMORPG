using UnityEngine;

public static class CombatRequestFactory
{
    public static DamageRequest CreateBasicDamage(
        ICombatSource attacker,
        IDamageReceiver target,
        int rawDamage,
        Vector3 hitWorldPosition,
        DamageSourceType sourceType)
    {
        return new DamageRequest
        {
            attacker = attacker,
            target = target,
            rawDamage = rawDamage,
            hitWorldPosition = hitWorldPosition,
            sourceType = sourceType
        };
    }
}
