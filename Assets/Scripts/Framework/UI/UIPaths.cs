using UnityEngine;

// 路径与名称常量汇总（后续可逐步替换散落的魔法字符串）
public static class UIPaths
{
    // Root
    public const string PanelCanvas = "UI/Root/PanelCanvas";
    public const string UIMask = "UI/Root/UIMask";

    // Windows（面板名需与预制及脚本类一致）
    public const string BeginPanel = "BeginPanel";
    public const string SettingPanel = "SettingPanel";
    public const string CreateRolePanel = "CreateRolePanel";
    public const string RoleInfoPanel = "RoleInfoPanel";
    public const string MainPanel = "MainPanel";
    public const string ContinuePanel = "ContinuePanel";
    public const string MessageTipPanel = "MessageTipPanel";
    public const string ConfirmPanel = "ConfirmPanel";
    public const string AboutPanel = "AboutPanel";
    public const string MapPanel = "MapPanel";

    // Portrait
    public const string PortraitCreateRoleRoot = "Portrait/CreateRolePanel/";
    public const string PortraitMainRoot = "Portrait/MainPanel/";
    public const string PortraitRoleHeadRoot = "Portrait/RoleHead/";

    // Role prefab paths
    public const string PlayerArmature = "Role/PlayerAmature/PlayerArmature";
    public const string MainCamera = "Role/PlayerAmature/MainCamera";
    public const string PlayerFollowCamera = "Role/PlayerAmature/PlayerFollowCamera";

    // Role visuals
    public const string RoleEngineer = "Role/Role_NoWeapon/Engineer";
    public const string RoleInfantry = "Role/Role_NoWeapon/Infantry";
    public const string RoleMedic = "Role/Role_NoWeapon/Medic";
    public const string RoleSniper = "Role/Role_NoWeapon/Sniper";
}

