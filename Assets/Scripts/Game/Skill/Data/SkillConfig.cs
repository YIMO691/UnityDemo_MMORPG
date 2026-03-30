using System;
using System.Collections.Generic;

[Serializable]
public class SkillConfig
{
    public int id;
    public string name;
    public float cooldown;
    public float castRange;
    public int unlockLevel;
    public SkillTargetType targetType;
    public List<SkillEffectData> effects;
}
