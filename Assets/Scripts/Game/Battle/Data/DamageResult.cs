using UnityEngine;

public class DamageResult
{
    public bool success;

    public ICombatSource attacker;
    public IDamageReceiver target;

    public int rawDamage;
    public int finalDamage;
    public bool isKilled;

    public bool isCritical;
    public bool isDodged;

    public int attackerAttack;
    public int targetDefense;

    public float attackerCritRate;
    public float attackerCritDamage;
    public float attackerHitRate;
    public float targetDodgeRate;

    public float critMultiplierApplied;
    public int defenseReducedValue;

    public DamageSourceType sourceType;
    public Vector3 hitWorldPosition;
}
