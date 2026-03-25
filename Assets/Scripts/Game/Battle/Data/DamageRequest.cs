using UnityEngine;

public class DamageRequest
{
    public ICombatSource attacker;
    public IDamageReceiver target;
    public int rawDamage;
    public DamageSourceType sourceType;
    public Vector3 hitWorldPosition;
}
