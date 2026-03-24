public static class MonsterFactory
{
    public static MonsterConfig GetConfig(int configId)
    {
        return MonsterConfigManager.Instance.GetConfig(configId);
    }
}
