using UnityEngine;

public class PlayerSkillDebugController : MonoBehaviour
{
    [SerializeField] private int debugSkillId = 1001;
    [SerializeField] private float detectRadius = 8f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TryCastDebugSkill();
        }
    }

    private void TryCastDebugSkill()
    {
        PlayerEntity player = FindObjectOfType<PlayerEntity>();
        if (player == null)
        {
            Debug.LogWarning("[PlayerSkillDebug] player not found.");
            return;
        }

        PlayerProgressionService.Instance.RefreshUnlockedSkillsForCurrentPlayer();

        Component target = PlayerSkillTargetResolver.ResolvePrimaryEnemyTarget(player, detectRadius);

        if (target == null)
        {
            Debug.LogWarning("[PlayerSkillDebug] no monster target found.");
            return;
        }

        Vector3 hitWorldPos = target.transform.position;
        SkillCastResult result = PlayerSkillService.Instance.CastSkill(player, debugSkillId, target, hitWorldPos);

        if (!result.success && result.failReason == SkillCastFailReason.OnCooldown)
        {
            float remain = PlayerSkillService.Instance.GetRemainingCooldown(player, debugSkillId);
            Debug.Log("[PlayerSkillDebug] cooldown remaining = " + remain.ToString("F2") + "s");
        }

        Debug.Log("[PlayerSkillDebug] cast result -> success=" + result.success + ", reason=" + result.failReason + ", skillId=" + result.skillId);
    }

    
}
