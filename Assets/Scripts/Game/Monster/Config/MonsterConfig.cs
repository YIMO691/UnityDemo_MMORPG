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
}
