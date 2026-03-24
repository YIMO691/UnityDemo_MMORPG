using UnityEngine;

public class DamageRequest
{
    public GameObject attacker;
    public MonsterEntity target;
    public int rawDamage;
    public DamageSourceType sourceType;
    public Vector3 hitWorldPosition;
}
