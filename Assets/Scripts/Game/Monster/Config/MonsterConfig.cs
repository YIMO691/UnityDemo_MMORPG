using System;

[Serializable]
public class MonsterConfig
{
    public int id;
    public string name;
    public int maxHp;
    public float moveSpeed;
    public float detectRange;
    public float attackRange;
    public float attackInterval;
    public string prefabPath;

    public int attack;
    public int defense;

    public float critRate;
    public float critDamage;
    public float hitRate;
    public float dodgeRate;

    public int dropTableId;
    public int expReward;
}
