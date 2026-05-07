using UnityEngine;

public static class PlayerSkillTargetResolver
{
    public static Component ResolvePrimaryEnemyTarget(PlayerEntity player, float detectRadius)
    {
        if (player == null) return null;

        if (player.CurrentTarget != null)
        {
            var monster = player.CurrentTarget.GetComponent<MonsterEntity>();
            if (monster != null && !monster.IsDead)
            {
                return player.CurrentTarget;
            }
        }

        Collider[] hits = Physics.OverlapSphere(player.transform.position, detectRadius);
        return FindNearestMonster(player.transform.position, hits);
    }

    private static Component FindNearestMonster(Vector3 origin, Collider[] hits)
    {
        Component best = null;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var monster = hits[i].GetComponentInParent<MonsterEntity>();
            if (monster == null) continue;
            if (monster.IsDead) continue;

            float sqr = (monster.transform.position - origin).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = monster;
            }
        }

        return best;
    }
}
