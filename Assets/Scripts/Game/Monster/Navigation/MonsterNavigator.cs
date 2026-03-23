using UnityEngine;

public class MonsterNavigator : MonoBehaviour, INavigationAgent
{
    public string AgentId { get; private set; }
    public Transform AgentTransform => transform;
    public bool IsNavigating => navigating;

    private Vector3[] path;
    private int index;
    private float stopDistance;
    private bool navigating;
    private MonsterEntity entity;

    public void SetAgentId(string id)
    {
        AgentId = id;
    }

    public void SetPath(Vector3[] pathPoints, float stopDist)
    {
        path = pathPoints;
        stopDistance = stopDist;
        index = 0;
        navigating = path != null && path.Length > 0;
    }

    public void StopNavigation()
    {
        navigating = false;
        path = null;
        index = 0;
    }

    private void Awake()
    {
        entity = GetComponent<MonsterEntity>();
    }

    private void Update()
    {
        if (!navigating || path == null || index >= path.Length) return;
        float speed = entity != null ? entity.GetMoveSpeed() : 3f;
        Vector3 target = path[index];
        Vector3 dir = target - transform.position;
        float dist = dir.magnitude;
        if (dist <= 0.01f)
        {
            index++;
            if (index >= path.Length)
            {
                navigating = false;
                return;
            }
            return;
        }
        dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;
        float endDist = Vector3.Distance(transform.position, path[path.Length - 1]);
        if (endDist <= stopDistance)
        {
            navigating = false;
        }
    }
}
