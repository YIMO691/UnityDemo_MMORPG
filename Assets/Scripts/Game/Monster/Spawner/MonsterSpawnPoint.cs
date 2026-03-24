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

    private float respawnTimer;
    private bool initialized;

    private void Awake()
    {
        EnsureSpawnPointId();
    }

    public void Init()
    {
        if (initialized) return;
        initialized = true;

        EnsureSpawnPointId();
        EnsureDefaultWeights();
        respawnTimer = 0f;

        if (spawnOnStart)
        {
            TryFillToMaxAlive();
        }
    }

    private void Update()
    {
        if (!initialized) return;

        if (string.IsNullOrWhiteSpace(spawnPointId))
        {
            Debug.LogError($"[MonsterSpawnPoint] spawnPointId is empty on {gameObject.name}");
            enabled = false;
            return;
        }

        if (useActivationDistance)
        {
            var player = PlayerLocator.Instance != null ? PlayerLocator.Instance.GetPlayerTransform() : null;
            if (player == null) return;

            float dist = Vector3.Distance(player.position, transform.position);
            if (dist > activationDistance) return;
        }

        int aliveCount = MonsterRuntimeRegistry.Instance.CountAliveBySpawnPoint(spawnPointId);
        if (aliveCount >= maxAliveCount)
        {
            respawnTimer = 0f;
            return;
        }

        respawnTimer += Time.deltaTime;
        if (respawnTimer >= respawnTime)
        {
            respawnTimer = 0f;
            TryFillToMaxAlive();
        }
    }

    private void TryFillToMaxAlive()
    {
        int aliveCount = MonsterRuntimeRegistry.Instance.CountAliveBySpawnPoint(spawnPointId);
        int guard = 0;

        while (aliveCount < maxAliveCount)
        {
            if (!TrySpawnOne()) break;

            aliveCount++;
            guard++;
            if (guard > 32) break;
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
        var go = MonsterRuntimeService.CreateFromSpawnPoint(selectedId, pos, spawnPointId);
        if (go == null) return false;

        var entity = go.GetComponent<MonsterEntity>();
        if (entity == null) return false;

        return true;
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 basePos = transform.position;
        if (spawnRadius <= 0f) return basePos;

        Vector2 rand = Random.insideUnitCircle * spawnRadius;
        return basePos + new Vector3(rand.x, 0f, rand.y);
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
            new MonsterSpawnEntry { monsterId = 1, weight = 10 },
            new MonsterSpawnEntry { monsterId = 2, weight = 45 },
            new MonsterSpawnEntry { monsterId = 3, weight = 45 }
        };
    }

    private void EnsureSpawnPointId()
    {
        if (!string.IsNullOrWhiteSpace(spawnPointId)) return;

        spawnPointId = gameObject.scene.name + "_" + gameObject.name;
    }
}
