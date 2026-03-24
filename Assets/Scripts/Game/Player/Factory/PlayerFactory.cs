using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerFactory
{
    public static PlayerData CreatePlayerData(CreateRoleRequest request)
    {
        if (request == null)
        {
            Debug.LogError("[PlayerFactory] CreateRoleRequest is null.");
            return null;
        }

        RoleClassConfig classConfig = RoleDataManager.Instance.GetClassConfig(request.classId);
        if (classConfig == null)
        {
            Debug.LogError($"[PlayerFactory] RoleClassConfig not found: {request.classId}");
            return null;
        }

        PlayerBaseData baseData = new PlayerBaseData
        {
            roleId = Guid.NewGuid().ToString(),
            roleName = request.playerName,
            classId = request.classId,
            genderId = request.genderId,
            appearanceId = request.appearanceId
        };

        PlayerProgressData progressData = new PlayerProgressData
        {
            level = classConfig.baseLevel,
            currentExp = classConfig.baseExp,
            expToNextLevel = classConfig.baseExpToLevel,
            skillIds = classConfig.starterSkillIds != null
                ? new List<int>(classConfig.starterSkillIds)
                : new List<int>()
        };

        PlayerAttributeData attributeData = new PlayerAttributeData
        {
            maxHp = classConfig.maxHp,
            maxStamina = classConfig.maxStamina,
            attack = classConfig.attack,
            defense = classConfig.defense,
            speed = classConfig.speed,
            critRate = classConfig.critRate,
            critDamage = classConfig.critDamage,
            hitRate = classConfig.hitRate,
            dodgeRate = classConfig.dodgeRate
        };

        PlayerRuntimeData runtimeData = new PlayerRuntimeData
        {
            currentHp = classConfig.maxHp,
            currentStamina = classConfig.maxStamina,
            isDead = false,
            hasValidPosition = false,
            posX = 0f,
            posY = 0f,
            posZ = 0f,
            rotY = 0f
        };

        PlayerData playerData = new PlayerData
        {
            baseData = baseData,
            progressData = progressData,
            attributeData = attributeData,
            runtimeData = runtimeData
        };

        return playerData;
    }

    public static GameObject CreatePlayerObject(PlayerData playerData, Vector3 position, Quaternion rotation)
    {
        if (playerData == null)
        {
            Debug.LogError("[PlayerFactory] PlayerData is null.");
            return null;
        }

        string prefabPath = GetPlayerPrefabPath(playerData.baseData.classId);
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError($"[PlayerFactory] Invalid prefab path for classId: {playerData.baseData.classId}");
            return null;
        }

        GameObject prefab = ResourceManager.Instance.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[PlayerFactory] Prefab not found at path: {prefabPath}");
            return null;
        }

        GameObject playerObj = UnityEngine.Object.Instantiate(prefab, position, rotation);
        playerObj.name = $"Player_{playerData.baseData.roleName}_{playerData.baseData.classId}";

        PlayerVisualAssembler.AttachRoleVisual(playerObj, playerData);

        var entity = playerObj.GetComponent<PlayerEntity>();
        if (entity == null) entity = playerObj.AddComponent<PlayerEntity>();
        string runtimeId = $"Player_{playerData.baseData.roleId}";
        entity.Init(playerData, runtimeId);
        entity.ApplyRuntimeSnapshot();

        return playerObj;
    }

    private static string GetPlayerPrefabPath(int classId)
    {
        return AssetPaths.PlayerArmature;
    }
}
