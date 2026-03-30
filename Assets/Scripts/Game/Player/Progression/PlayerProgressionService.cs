using UnityEngine;

public sealed class PlayerProgressionService
{
    private static readonly PlayerProgressionService instance = new PlayerProgressionService();
    public static PlayerProgressionService Instance => instance;

    private const int HpPerLevel = 20;
    private const int StaminaPerLevel = 5;
    private const int AttackPerLevel = 3;
    private const int DefensePerLevel = 2;
    private const int SpeedPerLevel = 0;

    private const float CritRatePerLevel = 0.005f;
    private const float CritDamagePerLevel = 0.01f;
    private const float HitRatePerLevel = 0.005f;
    private const float DodgeRatePerLevel = 0.005f;

    private PlayerProgressionService() { }

    public bool AddExpToCurrentPlayer(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("[Progression] 经验值必须大于 0。");
            return false;
        }

        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null)
        {
            Debug.LogWarning("[Progression] 当前没有玩家数据，无法加经验。");
            return false;
        }

        GamePlayerDataService.Instance.NormalizePlayerData(playerData);

        playerData.progressData.currentExp += amount;

        Debug.Log("[Progression] GainExp +" + amount + " -> " + playerData.progressData.currentExp + "/" + playerData.progressData.expToNextLevel);

        bool leveledUp = false;

        while (playerData.progressData.currentExp >= playerData.progressData.expToNextLevel)
        {
            playerData.progressData.currentExp -= playerData.progressData.expToNextLevel;
            LevelUp(playerData);
            leveledUp = true;
        }

        return leveledUp;
    }

    public int GetCurrentLevel()
    {
        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null || playerData.progressData == null)
            return 1;
        return Mathf.Max(1, playerData.progressData.level);
    }

    public int GetCurrentExp()
    {
        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null || playerData.progressData == null)
            return 0;
        return Mathf.Max(0, playerData.progressData.currentExp);
    }

    public int GetCurrentExpToNextLevel()
    {
        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null || playerData.progressData == null)
            return PlayerProgressionFormula.GetExpToNextLevel(1);
        return Mathf.Max(1, playerData.progressData.expToNextLevel);
    }

    private void LevelUp(PlayerData playerData)
    {
        playerData.progressData.level += 1;

        ApplyLevelGrowth(playerData);

        playerData.progressData.expToNextLevel =
            PlayerProgressionFormula.GetExpToNextLevel(playerData.progressData.level);

        if (playerData.runtimeData != null && playerData.attributeData != null)
        {
            playerData.runtimeData.currentHp = playerData.attributeData.maxHp;
            playerData.runtimeData.currentStamina = playerData.attributeData.maxStamina;
            EventBus.Publish(new PlayerHpChangedEvent(playerData.runtimeData.currentHp, playerData.attributeData.maxHp));
            EventBus.Publish(new PlayerStaminaChangedEvent(playerData.runtimeData.currentStamina, playerData.attributeData.maxStamina));
        }

        TryUnlockSkills(playerData);

        Debug.Log("[Progression] LevelUp -> Lv." + playerData.progressData.level + ", nextExp=" + playerData.progressData.expToNextLevel);
    }

    private void ApplyLevelGrowth(PlayerData playerData)
    {
        if (playerData.attributeData == null)
        {
            playerData.attributeData = new PlayerAttributeData();
        }

        playerData.attributeData.maxHp += HpPerLevel;
        playerData.attributeData.maxStamina += StaminaPerLevel;
        playerData.attributeData.attack += AttackPerLevel;
        playerData.attributeData.defense += DefensePerLevel;
        playerData.attributeData.speed += SpeedPerLevel;

        playerData.attributeData.critRate += CritRatePerLevel;
        playerData.attributeData.critDamage += CritDamagePerLevel;
        playerData.attributeData.hitRate += HitRatePerLevel;
        playerData.attributeData.dodgeRate += DodgeRatePerLevel;
    }

    private void TryUnlockSkills(PlayerData playerData)
    {
        if (playerData == null || playerData.progressData == null)
            return;

        if (playerData.progressData.skillIds == null)
            playerData.progressData.skillIds = new System.Collections.Generic.List<int>();

        var allSkills = SkillConfigManager.Instance.GetAllConfigs();
        if (allSkills == null) return;

        for (int i = 0; i < allSkills.Count; i++)
        {
            var cfg = allSkills[i];
            if (cfg == null) continue;

            if (cfg.unlockLevel <= playerData.progressData.level &&
                !playerData.progressData.skillIds.Contains(cfg.id))
            {
                playerData.progressData.skillIds.Add(cfg.id);
                Debug.Log("[Progression] Unlock skill -> " + cfg.id + " / " + cfg.name);
            }
        }
    }

    public void RefreshUnlockedSkillsForCurrentPlayer()
    {
        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null) return;

        GamePlayerDataService.Instance.NormalizePlayerData(playerData);
        TryUnlockSkills(playerData);
    }
}
