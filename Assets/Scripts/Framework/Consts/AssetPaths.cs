public static class AssetPaths
{
    // Root UI
    public const string PanelCanvas = "UI/Root/PanelCanvas";
    public const string UIMask = "UI/Root/UIMask";

    // UI Windows
    public static string Window(string panelName) => $"UI/Windows/{panelName}";

    // Config
    public const string RoleClassConfig = "Config/RoleClassConfig";
    public const string MapConfig = "Config/MapConfig";

    // Player & Camera
    public const string PlayerArmature = "Role/PlayerAmature/PlayerArmature";
    public const string MainCamera = "Role/PlayerAmature/MainCamera";
    public const string PlayerFollowCamera = "Role/PlayerAmature/PlayerFollowCamera";

    // Role visuals
    public const string EngineerVisual = "Role/Role_NoWeapon/Engineer";
    public const string InfantryVisual = "Role/Role_NoWeapon/Infantry";
    public const string MedicVisual = "Role/Role_NoWeapon/Medic";
    public const string SniperVisual = "Role/Role_NoWeapon/Sniper";
}
