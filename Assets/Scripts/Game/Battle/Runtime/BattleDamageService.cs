using UnityEngine;

public class BattleDamageService
{
    private static readonly BattleDamageService instance = new BattleDamageService();
    public static BattleDamageService Instance => instance;

    private BattleDamageService() { }

    public DamageResult ApplyDamage(DamageRequest request)
    {
        Debug.Log($"[BattleDamageService] ApplyDamage attacker={request.attacker}, target={request.target}, raw={request.rawDamage}");

        var result = new DamageResult
        {
            success = false,
            attacker = request != null ? request.attacker : null,
            target = request != null ? request.target : null,
            rawDamage = request != null ? request.rawDamage : 0,
            finalDamage = 0,
            isKilled = false,
            isCritical = false,
            isDodged = false,
            critMultiplierApplied = 1f,
            defenseReducedValue = 0,
            hitWorldPosition = request != null ? request.hitWorldPosition : Vector3.zero,
            sourceType = request != null ? request.sourceType : DamageSourceType.NormalAttack
        };

        if (request == null)
        {
            Debug.LogWarning("[BattleDamageService] request is null.");
            return result;
        }

        if (request.attacker == null)
        {
            Debug.LogWarning("[BattleDamageService] attacker is null.");
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

        // 同阵营忽略
        if (request.target is IFactionProvider tf && request.attacker is IFactionProvider af)
        {
            if (tf.FactionId == af.FactionId)
            {
                Debug.Log("[BattleDamageService] same faction, ignore damage.");
                return result;
            }
        }

        // 读取统一属性
        int attack = request.attacker.Attack;
        int defense = 0;
        float critRate = request.attacker.CritRate;
        float critDmg = Mathf.Max(1f, request.attacker.CritDamage);
        float hitRate = request.attacker.HitRate;
        float dodgeRate = 0f;

        if (request.target is IAttributeProvider tAttr)
        {
            defense = tAttr.Defense;
            dodgeRate = tAttr.DodgeRate;
        }

        result.attackerAttack = attack;
        result.targetDefense = defense;
        result.attackerCritRate = critRate;
        result.attackerCritDamage = critDmg;
        result.attackerHitRate = hitRate;
        result.targetDodgeRate = dodgeRate;

        // 简单闪避
        if (Random.value < dodgeRate)
        {
            result.success = true;
            result.isDodged = true;
            result.finalDamage = 0;
            EventBus.Publish(new DamageAppliedEvent(result));
            return result;
        }

        // 暴击
        if (Random.value < critRate)
        {
            result.isCritical = true;
            result.critMultiplierApplied = critDmg;
        }

        // 基础伤害：raw + attack - defense
        int baseDamage = request.rawDamage + attack;
        int afterDefense = baseDamage - defense;
        result.defenseReducedValue = Mathf.Max(0, baseDamage - afterDefense);

        int finalDamage = Mathf.Max(1, afterDefense);
        if (result.isCritical)
        {
            finalDamage = Mathf.Max(1, Mathf.FloorToInt(finalDamage * result.critMultiplierApplied));
        }

        request.target.ReceiveDamage(finalDamage);

        result.success = true;
        result.finalDamage = finalDamage;
        result.isKilled = request.target.IsDead;
        result.target = request.target;
        result.hitWorldPosition = request.hitWorldPosition;

        EventBus.Publish(new DamageAppliedEvent(result));
        if (result.isKilled)
        {
            EventBus.Publish(new DeathEvent(result.target, result.attacker));
        }

        Debug.Log(
    $"[BattleDamageService] raw={request.rawDamage}, " +
    $"attack={attack}, defense={defense}, " +
    $"base={baseDamage}, final={finalDamage}");

        return result;
    }
}
