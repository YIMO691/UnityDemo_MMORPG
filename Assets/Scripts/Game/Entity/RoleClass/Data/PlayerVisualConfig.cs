using UnityEngine;

public struct RoleVisualSetting
{
    public string path;
    public Vector3 localPosition;
    public Vector3 localRotation;
    public Vector3 localScale;
}
public static class PlayerVisualConfig
{
    public static RoleVisualSetting GetRoleVisualSetting(int classId)
    {
        switch (classId)
        {
            case 1:
                return new RoleVisualSetting
                {
                    path = "Role/Role_NoWeapon/Engineer",
                    localPosition = Vector3.zero,
                    localRotation = Vector3.zero,
                    localScale = Vector3.one
                };
            case 2:
                return new RoleVisualSetting
                {
                    path = "Role/Role_NoWeapon/Infantry",
                    localPosition = Vector3.zero,
                    localRotation = Vector3.zero,
                    localScale = Vector3.one
                };
            case 3:
                return new RoleVisualSetting
                {
                    path = "Role/Role_NoWeapon/Medic",
                    localPosition = Vector3.zero,
                    localRotation = Vector3.zero,
                    localScale = Vector3.one
                };
            case 4:
                return new RoleVisualSetting
                {
                    path = "Role/Role_NoWeapon/Sniper",
                    localPosition = Vector3.zero,
                    localRotation = Vector3.zero,
                    localScale = Vector3.one
                };
            default:
                return default;
        }
    }
}
