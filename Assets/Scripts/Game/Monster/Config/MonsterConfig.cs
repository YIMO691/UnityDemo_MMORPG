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

    // 战斗属性（第五阶段第四步骤补充）
    public int attack;
    public int defense;

    public float critRate;
    public float critDamage;
    public float hitRate;
    public float dodgeRate;
}
