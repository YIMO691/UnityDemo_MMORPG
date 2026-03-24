using UnityEngine;

public class MonsterNavigator : MonoBehaviour, INavigationAgent
{
    public string AgentId { get; private set; }
    public Transform AgentTransform => transform;
    public bool IsNavigating => navigating;
    public float CurrentSpeed => currentSpeed;

    private readonly NavigationPathSolver pathSolver = new NavigationPathSolver();

    private Vector3[] path;
    private int index;
    private float stopDistance;
    private bool navigating;
    private Vector3 lastPosition;
    private float currentSpeed;
    private float turnSpeed = 8f;
    private MonsterEntity entity;

    public void SetAgentId(string id)
    {
        AgentId = id;
    }

    public void MoveTo(Vector3 destination, float stopDist)
    {
        if (!pathSolver.TryBuildPath(transform.position, destination, out Vector3 sampledTarget, out Vector3[] corners))
        {
            StopNavigation();
            return;
        }

        SetPath(corners, stopDist);
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
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (entity != null && entity.IsDead) return;
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
        Vector3 planar = new Vector3(dir.x, 0f, dir.z);
        if (planar.sqrMagnitude > 0f)
        {
            Vector3 forwardTarget = planar.normalized;
            transform.forward = Vector3.Slerp(transform.forward, forwardTarget, Time.deltaTime * turnSpeed);
            transform.position += forwardTarget * speed * Time.deltaTime;
        }
        else
        {
            index++;
            if (index >= path.Length)
            {
                navigating = false;
                currentSpeed = 0f;
                return;
            }
        }
        currentSpeed = (transform.position - lastPosition).magnitude / Mathf.Max(Time.deltaTime, 1e-6f);
        lastPosition = transform.position;
        float endDist = Vector3.Distance(transform.position, path[path.Length - 1]);
        if (endDist <= stopDistance)
        {
            navigating = false;
            currentSpeed = 0f;
        }
    }
}
