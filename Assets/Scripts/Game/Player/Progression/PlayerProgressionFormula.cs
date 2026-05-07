using UnityEngine;

public static class PlayerProgressionFormula
{
    private const int BaseExpToNextLevel = 100;
    private const int ExpStepPerLevel = 50;

    public static int GetExpToNextLevel(int level)
    {
        level = Mathf.Max(1, level);
        return BaseExpToNextLevel + (level - 1) * ExpStepPerLevel;
    }
}
