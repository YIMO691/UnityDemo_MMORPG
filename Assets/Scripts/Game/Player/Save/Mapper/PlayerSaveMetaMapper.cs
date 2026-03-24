public static class PlayerSaveMetaMapper
{
    public static PlayerSaveMetaData Map(PlayerData playerData, int slotId)
    {
        if (playerData == null || playerData.baseData == null || playerData.progressData == null)
        {
            return null;
        }

        var meta = new PlayerSaveMetaData
        {
            slotId = slotId,
            roleName = string.IsNullOrEmpty(playerData.baseData.roleName) ? "未命名角色" : playerData.baseData.roleName,
            classId = playerData.baseData.classId,
            level = playerData.progressData.level,
            saveTime = string.IsNullOrEmpty(playerData.saveTime) ? "--" : playerData.saveTime
        };
        return meta;
    }
}
