using UnityEngine;

public abstract class BaseNavigator : MonoBehaviour, INavigationAgent
{
    [SerializeField] private string agentId;
    [SerializeField] protected float cornerReachDistance = 0.35f;

    protected Vector3[] pathPoints;
    protected int currentIndex;
    protected float stopDistance = 0.25f;

    public string AgentId => agentId;
    public Transform AgentTransform => transform;
    public bool IsNavigating { get; protected set; }

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(agentId))
        {
            agentId = gameObject.name;
        }
    }

    protected virtual void OnEnable()
    {
        NavigationRegistry.Instance.Register(this);
        Debug.Log("[BaseNavigator] 注册导航对象: " + AgentId);
    }

    protected virtual void OnDisable()
    {
        NavigationRegistry.Instance.Unregister(this);
        Debug.Log("[BaseNavigator] 注销导航对象: " + AgentId);
    }

    public void SetAgentId(string id)
    {
        agentId = id;
    }

    public virtual void SetPath(Vector3[] newPath, float newStopDistance)
    {
        if (newPath == null || newPath.Length == 0)
        {
            StopNavigation();
            return;
        }

        pathPoints = newPath;
        currentIndex = 0;
        stopDistance = newStopDistance;
        IsNavigating = true;
    }

    public virtual void StopNavigation()
    {
        pathPoints = null;
        currentIndex = 0;
        IsNavigating = false;
        OnNavigationStopped();
    }

    protected virtual void Update()
    {
        if (!IsNavigating || pathPoints == null || currentIndex >= pathPoints.Length)
            return;

        TickNavigation();
    }

    protected void AdvanceIfReached(float distance)
    {
        float reachDistance = currentIndex == pathPoints.Length - 1 ? stopDistance : cornerReachDistance;

        if (distance <= reachDistance)
        {
            currentIndex++;
            if (currentIndex >= pathPoints.Length)
            {
                StopNavigation();
            }
        }
    }

    protected abstract void TickNavigation();
    protected abstract void OnNavigationStopped();

    public Vector3[] GetPathPoints()
    {
        return pathPoints;
    }

    public int GetCurrentPathIndex()
    {
        return currentIndex;
    }

    public bool HasPath()
    {
        return IsNavigating && pathPoints != null && currentIndex < pathPoints.Length;
    }
}
