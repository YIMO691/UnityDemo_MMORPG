using System;
using UnityEngine;

public static class PlayerFactory
{
    public static PlayerData CreatePlayerData(CreateRoleRequest request)
    {
        RoleClassConfig classConfig = RoleDataManager.Instance.GetClassConfig(request.classId);
        if (classConfig == null)
        {
            Debug.LogError($"[PlayerFactory] RoleClassConfig not found: {request.classId}");
            return null;
        }

        PlayerData player = new PlayerData
        {
            roleId = Guid.NewGuid().ToString(),
            roleName = request.playerName,
            classId = request.classId,
            genderId = request.genderId,
            appearanceId = request.appearanceId,

            level = classConfig.baseLevel,
            currentExp = classConfig.baseExp,
            expToNextLevel = classConfig.baseExpToLevel,

            maxHp = classConfig.maxHp,
            maxMp = classConfig.maxMp,
            attack = classConfig.attack,
            defense = classConfig.defense,
            speed = classConfig.speed,

            critRate = classConfig.critRate,
            critDamage = classConfig.critDamage,
            hitRate = classConfig.hitRate,
            dodgeRate = classConfig.dodgeRate,

            skillIds = new System.Collections.Generic.List<int>(classConfig.starterSkillIds)
        };

        return player;
    }
}
