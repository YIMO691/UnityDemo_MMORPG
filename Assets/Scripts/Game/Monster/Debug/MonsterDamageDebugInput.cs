using UnityEngine;

public class MonsterDamageDebugInput : MonoBehaviour
{
    public float attackRange = 3f;
    public int damage = 10;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TryAttackNearestMonster();
        }
    }

    private void TryAttackNearestMonster()
    {
        Transform player = PlayerLocator.Instance?.GetPlayerTransform();
        if (player == null)
        {
            Debug.LogWarning("[DebugAttack] Player not found");
            return;
        }

        MonsterEntity[] monsters = GameObject.FindObjectsOfType<MonsterEntity>();
        if (monsters == null || monsters.Length == 0)
        {
            Debug.Log("[DebugAttack] No monsters in scene");
            return;
        }

        MonsterEntity nearest = null;
        float minDist = float.MaxValue;
        for (int i = 0; i < monsters.Length; i++)
        {
            var m = monsters[i];
            if (m == null || m.IsDead) continue;
            float dist = Vector3.Distance(player.position, m.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = m;
            }
        }

        if (nearest == null)
        {
            Debug.Log("[DebugAttack] No valid target");
            return;
        }

        if (minDist > attackRange)
        {
            Debug.Log($"[DebugAttack] Target too far: {minDist:F2}");
            return;
        }

        Debug.Log($"[DebugAttack] Hit monster id={nearest.ConfigId}, dist={minDist:F2}");
        var req = new DamageRequest
        {
            attacker = gameObject,
            target = nearest,
            rawDamage = damage,
            sourceType = DamageSourceType.Debug,
            hitWorldPosition = nearest.transform.position + Vector3.up * 2f
        };
        BattleDamageService.Instance.ApplyDamage(req);
    }
}
