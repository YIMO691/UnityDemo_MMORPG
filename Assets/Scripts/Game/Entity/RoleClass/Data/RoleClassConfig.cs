using System;
using System.Collections.Generic;

[Serializable]
public class RoleClassConfig
{
    public int id;
    public string className;
    public string displayName;
    public string description;
    public string roleType;

    public int baseLevel;
    public int baseExp;
    public int baseExpToLevel;
    public float expGrowthRate;

    public int maxHp;
    public int maxStamina;
    public int maxMp;
    public int attack;
    public int defense;
    public int speed;

    public float critRate;
    public float critDamage;
    public float hitRate;
    public float dodgeRate;

    public float hpGrowth;
    public float mpGrowth;
    public float attackGrowth;
    public float defenseGrowth;
    public float speedGrowth;

    public string defaultPortraitId;
    public string mainHeadId;
    public List<int> starterSkillIds = new List<int>();
}
