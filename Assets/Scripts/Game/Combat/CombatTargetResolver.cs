using UnityEngine;

public static class CombatTargetResolver
{
    public static IDamageReceiver ResolveDamageReceiver(Component target)
    {
        if (target == null) return null;
        return target.GetComponent<IDamageReceiver>();
    }

    public static ICombatSource ResolveCombatSource(Component source)
    {
        if (source == null) return null;
        return source.GetComponent<ICombatSource>();
    }

    public static bool IsValidHostileTarget(ICombatSource attacker, IDamageReceiver target)
    {
        if (attacker == null || target == null) return false;
        if (target.IsDead) return false;

        if (attacker is IFactionProvider attackerFaction &&
            target is IFactionProvider targetFaction)
        {
            if (attackerFaction.FactionId == targetFaction.FactionId)
                return false;
        }

        return true;
    }
}
