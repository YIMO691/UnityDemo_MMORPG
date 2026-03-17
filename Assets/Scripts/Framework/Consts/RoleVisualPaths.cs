public static class RoleVisualPaths
{
    public static string GetPath(int classId)
    {
        switch (classId)
        {
            case 1: return AssetPaths.EngineerVisual;
            case 2: return AssetPaths.InfantryVisual;
            case 3: return AssetPaths.MedicVisual;
            case 4: return AssetPaths.SniperVisual;
            default: return null;
        }
    }
}

