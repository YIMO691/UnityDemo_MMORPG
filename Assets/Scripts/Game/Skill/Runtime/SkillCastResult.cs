public enum SkillCastFailReason
{
    None = 0,
    InvalidCaster = 1,
    ConfigNotFound = 2,
    NotUnlocked = 3,
    OnCooldown = 4,
    InvalidTarget = 5,
    OutOfRange = 6,
    NoEffects = 7
}

public struct SkillCastResult
{
    public bool success;
    public int skillId;
    public SkillCastFailReason failReason;
}
