using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MonsterSpawnEntry
{
    public int monsterId;
    public int weight = 1;
}

public class MonsterSpawnPoint : MonoBehaviour
{
    [Header("配置")]
    public int monsterId = 1;
    public bool spawnOnStart = true;
    public float respawnTime = 5f;
    public int maxAliveCount = 1;
    public string spawnPointId;
    public float spawnRadius = 0f;
    public bool useActivationDistance = false;
    public float activationDistance = 25f;
    [Header("多怪配置")]
    public List<MonsterSpawnEntry> spawnEntries = new List<MonsterSpawnEntry>();
    [Header("默认权重（当 spawnEntries 为空时生效）")]
    public bool useDefaultWeights = true;

    private readonly List<MonsterEntity> aliveEntities = new List<MonsterEntity>();
    private float respawnTimer;
    private bool initialized;

    public void Init()
    {
        if (initialized) return;
        initialized = true;

        EnsureDefaultWeights();
        BindRestoredMonsters();

        if (spawnOnStart)
        {
            TryFillToMaxAlive();
        }
    }

    private void Update()
    {
        if (!initialized) return;

        CleanupDeadRefs();

        if (useActivationDistance)
        {
            var player = PlayerLocator.Instance != null ? PlayerLocator.Instance.GetPlayerTransform() : null;
            if (player == null) return;
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist > activationDistance) return;
        }

        if (aliveEntities.Count >= maxAliveCount) return;

        respawnTimer += Time.deltaTime;
        if (respawnTimer >= respawnTime)
        {
            respawnTimer = 0f;
            TryFillToMaxAlive();
        }
    }

    private bool TrySpawnOne()
    {
        Vector3 pos = GetSpawnPosition();
#if UNITY_EDITOR || UNITY_STANDALONE
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            pos = hit.position;
        }
#endif
        int selectedId = ResolveMonsterId();
        var go = MonsterFactory.CreateNew(selectedId, pos, spawnPointId);
        if (go == null) return false;
        var entity = go.GetComponent<MonsterEntity>();
        if (entity == null) return false;
        aliveEntities.Add(entity);
        entity.OnDead += OnMonsterDead;
        return true;
    }

    private void OnMonsterDead(MonsterEntity entity)
    {
        if (entity == null) return;
        aliveEntities.Remove(entity);
    }

    private void TryFillToMaxAlive()
    {
        int guard = 0;
        while (aliveEntities.Count < maxAliveCount)
        {
            if (!TrySpawnOne()) break;
            guard++;
            if (guard > 32) break;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 basePos = transform.position;
        if (spawnRadius <= 0f) return basePos;
        Vector2 rand = Random.insideUnitCircle * spawnRadius;
        return basePos + new Vector3(rand.x, 0f, rand.y);
    }

    private void BindRestoredMonsters()
    {
        var all = MonsterRuntimeRegistry.Instance.GetAll();
        for (int i = 0; i < all.Count; i++)
        {
            var m = all[i];
            if (m == null) continue;
            if (m.IsDead) continue;
            if (m.SpawnPointId != spawnPointId) continue;
            if (!aliveEntities.Contains(m))
            {
                aliveEntities.Add(m);
                m.OnDead += OnMonsterDead;
            }
        }
    }

    private void CleanupDeadRefs()
    {
        aliveEntities.RemoveAll(e => e == null || e.IsDead);
    }

    private int ResolveMonsterId()
    {
        if (spawnEntries == null || spawnEntries.Count == 0) return monsterId;
        int totalWeight = 0;
        for (int i = 0; i < spawnEntries.Count; i++)
        {
            totalWeight += Mathf.Max(1, spawnEntries[i].weight);
        }
        int rand = Random.Range(0, totalWeight);
        int cumulative = 0;
        for (int i = 0; i < spawnEntries.Count; i++)
        {
            cumulative += Mathf.Max(1, spawnEntries[i].weight);
            if (rand < cumulative)
            {
                return spawnEntries[i].monsterId;
            }
        }
        return spawnEntries[spawnEntries.Count - 1].monsterId;
    }

    private void EnsureDefaultWeights()
    {
        if (!useDefaultWeights) return;
        if (spawnEntries != null && spawnEntries.Count > 0) return;
        spawnEntries = new List<MonsterSpawnEntry>
        {
            new MonsterSpawnEntry{ monsterId = 1, weight = 10 },
            new MonsterSpawnEntry{ monsterId = 2, weight = 45 },
            new MonsterSpawnEntry{ monsterId = 3, weight = 45 }
        };
    }
}
