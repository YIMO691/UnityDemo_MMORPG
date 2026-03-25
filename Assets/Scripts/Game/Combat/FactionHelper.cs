public static class FactionHelper
{
    public static bool AreFriendly(IFactionProvider a, IFactionProvider b)
    {
        if (a == null || b == null) return false;
        return a.FactionId == b.FactionId;
    }

    public static bool AreHostile(IFactionProvider a, IFactionProvider b)
    {
        if (a == null || b == null) return false;
        return a.FactionId != b.FactionId;
    }
}
