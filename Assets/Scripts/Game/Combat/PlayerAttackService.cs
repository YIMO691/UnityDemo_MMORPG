using UnityEngine;

public static class PlayerAttackService
{
    public static void Attack(PlayerEntity attacker, Component targetComponent, int rawDamage, Vector3 hitWorldPosition)
    {
        if (attacker == null) return;
        IDamageReceiver target = CombatTargetResolver.ResolveDamageReceiver(targetComponent);
        if (!CombatTargetResolver.IsValidHostileTarget(attacker, target)) return;
        var request = CombatRequestFactory.CreateBasicDamage(attacker, target, rawDamage, hitWorldPosition, DamageSourceType.NormalAttack);
        BattleDamageService.Instance.ApplyDamage(request);
    }
}
