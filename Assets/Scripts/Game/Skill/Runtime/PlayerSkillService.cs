using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerSkillService
{
    private static readonly PlayerSkillService instance = new PlayerSkillService();
    public static PlayerSkillService Instance => instance;

    private readonly Dictionary<string, float> nextCastTimeDict = new Dictionary<string, float>();

    private PlayerSkillService() { }

    public SkillCastResult CastSkill(PlayerEntity caster, int skillId, Component targetComponent, Vector3 hitWorldPosition)
    {
        var result = new SkillCastResult
        {
            success = false,
            skillId = skillId,
            failReason = SkillCastFailReason.None
        };

        if (caster == null)
        {
            result.failReason = SkillCastFailReason.InvalidCaster;
            return result;
        }

        SkillConfig cfg = SkillConfigManager.Instance.GetConfig(skillId);
        if (cfg == null)
        {
            result.failReason = SkillCastFailReason.ConfigNotFound;
            return result;
        }

        PlayerData playerData = GamePlayerDataService.Instance.GetCurrentPlayerData();
        if (playerData == null || playerData.progressData == null || playerData.progressData.skillIds == null)
        {
            result.failReason = SkillCastFailReason.NotUnlocked;
            return result;
        }

        if (!playerData.progressData.skillIds.Contains(skillId))
        {
            result.failReason = SkillCastFailReason.NotUnlocked;
            return result;
        }

        string cdKey = BuildCooldownKey(caster, skillId);
        if (IsOnCooldown(cdKey))
        {
            result.failReason = SkillCastFailReason.OnCooldown;
            return result;
        }

        if (cfg.effects == null || cfg.effects.Count == 0)
        {
            result.failReason = SkillCastFailReason.NoEffects;
            return result;
        }

        if (!ValidateTarget(caster, cfg, targetComponent))
        {
            result.failReason = SkillCastFailReason.InvalidTarget;
            return result;
        }

        if (!ValidateRange(caster, cfg, targetComponent))
        {
            result.failReason = SkillCastFailReason.OutOfRange;
            return result;
        }

        for (int i = 0; i < cfg.effects.Count; i++)
        {
            ExecuteEffect(caster, cfg, cfg.effects[i], targetComponent, hitWorldPosition);
        }

        nextCastTimeDict[cdKey] = Time.time + Mathf.Max(0f, cfg.cooldown);

        Debug.Log("[PlayerSkillService] Cast success -> skillId=" + skillId + ", name=" + cfg.name);

        result.success = true;
        return result;
    }

    public float GetRemainingCooldown(PlayerEntity caster, int skillId)
    {
        if (caster == null) return 0f;

        string key = BuildCooldownKey(caster, skillId);
        if (!nextCastTimeDict.TryGetValue(key, out float nextTime))
            return 0f;

        return Mathf.Max(0f, nextTime - Time.time);
    }

    private void ExecuteEffect(PlayerEntity caster, SkillConfig cfg, SkillEffectData effect, Component targetComponent, Vector3 hitWorldPosition)
    {
        if (effect == null) return;

        switch (effect.effectType)
        {
            case SkillEffectType.Damage:
                PlayerAttackService.Attack(
                    caster,
                    targetComponent,
                    Mathf.Max(0, effect.value),
                    hitWorldPosition,
                    DamageSourceType.Skill);
                break;

            case SkillEffectType.Heal:
                Debug.LogWarning("[PlayerSkillService] Heal effect reserved for future implementation.");
                break;

            case SkillEffectType.RestoreStamina:
                Debug.LogWarning("[PlayerSkillService] RestoreStamina effect reserved for future implementation.");
                break;

            default:
                Debug.LogWarning("[PlayerSkillService] Unsupported effectType=" + effect.effectType);
                break;
        }
    }

    private bool ValidateTarget(PlayerEntity caster, SkillConfig cfg, Component targetComponent)
    {
        switch (cfg.targetType)
        {
            case SkillTargetType.Self:
                return caster != null;

            case SkillTargetType.Enemy:
                if (targetComponent == null) return false;
                IDamageReceiver target = CombatTargetResolver.ResolveDamageReceiver(targetComponent);
                return CombatTargetResolver.IsValidHostileTarget(caster, target);

            default:
                return false;
        }
    }

    private bool ValidateRange(PlayerEntity caster, SkillConfig cfg, Component targetComponent)
    {
        if (caster == null) return false;
        if (cfg.targetType == SkillTargetType.Self) return true;
        if (targetComponent == null) return false;

        float sqrDistance = (targetComponent.transform.position - caster.transform.position).sqrMagnitude;
        float sqrRange = cfg.castRange * cfg.castRange;
        return sqrDistance <= sqrRange;
    }

    private bool IsOnCooldown(string key)
    {
        if (!nextCastTimeDict.TryGetValue(key, out float nextTime))
            return false;

        return Time.time < nextTime;
    }

    private string BuildCooldownKey(PlayerEntity caster, int skillId)
    {
        return caster.GetInstanceID() + "_" + skillId;
    }
}
