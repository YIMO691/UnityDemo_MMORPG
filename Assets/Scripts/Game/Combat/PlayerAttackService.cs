using UnityEngine;

public static class PlayerAttackService
{
    public static void Attack(PlayerEntity attacker, Component targetComponent, int rawDamage, Vector3 hitWorldPosition)
    {
        Attack(attacker, targetComponent, rawDamage, hitWorldPosition, DamageSourceType.NormalAttack);
    }

    public static void Attack(
        PlayerEntity attacker,
        Component targetComponent,
        int rawDamage,
        Vector3 hitWorldPosition,
        DamageSourceType sourceType)
    {
        if (attacker == null) return;

        IDamageReceiver target = CombatTargetResolver.ResolveDamageReceiver(targetComponent);
        attacker.SetTarget(targetComponent);
        if (!CombatTargetResolver.IsValidHostileTarget(attacker, target)) return;

        var request = CombatRequestFactory.CreateBasicDamage(
            attacker,
            target,
            rawDamage,
            hitWorldPosition,
            sourceType);

        BattleDamageService.Instance.ApplyDamage(request);
    }
}
