using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    [Header("配置")]
    public int monsterId = 1;
    public bool spawnOnStart = true;
    public float respawnTime = 5f;
    public int maxAliveCount = 1;

    private int currentAlive;
    private float respawnTimer;
    private bool initialized;

    public void Init()
    {
        if (initialized) return;
        initialized = true;

        if (spawnOnStart)
        {
            TrySpawn();
        }
    }

    private void Update()
    {
        if (!initialized) return;

        if (currentAlive < maxAliveCount)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                respawnTimer = 0f;
                TrySpawn();
            }
        }
    }

    private void TrySpawn()
    {
        Vector3 pos = transform.position;
#if UNITY_EDITOR || UNITY_STANDALONE
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            pos = hit.position;
        }
#endif
        var go = MonsterSpawner.SpawnMonster(monsterId, pos);
        if (go == null) return;
        var entity = go.GetComponent<MonsterEntity>();
        if (entity == null) return;
        currentAlive++;
        entity.OnDead += OnMonsterDead;
    }

    private void OnMonsterDead(MonsterEntity entity)
    {
        currentAlive = Mathf.Max(0, currentAlive - 1);
    }
}
