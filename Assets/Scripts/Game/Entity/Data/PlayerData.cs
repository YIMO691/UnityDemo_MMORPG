using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string roleId;
    public string roleName;

    public int classId;
    public int genderId;
    public int appearanceId;

    public int level;
    public int currentExp;
    public int expToNextLevel;

    public int maxHp;
    public int maxMp;
    public int attack;
    public int defense;
    public int speed;

    public float critRate;
    public float critDamage;
    public float hitRate;
    public float dodgeRate;

    public List<int> skillIds = new List<int>();
}
