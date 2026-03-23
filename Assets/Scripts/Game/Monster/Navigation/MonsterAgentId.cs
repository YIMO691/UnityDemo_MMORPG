public static class MonsterAgentId
{
    public static string Create(int configId, int index)
    {
        return $"Monster_{configId}_{index}";
    }
}
