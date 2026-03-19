using UnityEngine;

public class NavigationService
{
    private static readonly NavigationService instance = new NavigationService();
    public static NavigationService Instance => instance;

    private readonly NavigationPathSolver pathSolver = new NavigationPathSolver();
    private bool initialized;
    private NavigationVisualState currentPlayerVisualState;

    public void Init()
    {
        if (initialized) return;
        initialized = true;
        EventBus.Subscribe<NavigationMoveRequestEvent>(OnMoveRequest);
        EventBus.Subscribe<NavigationStopRequestEvent>(OnStopRequest);

        Debug.Log("[NavigationService] Init 完成，已订阅事件。");
    }

    public NavigationVisualState GetPlayerVisualState()
    {
        return currentPlayerVisualState;
    }

    public void Dispose()
    {
        if (!initialized) return;
        initialized = false;
        EventBus.Unsubscribe<NavigationMoveRequestEvent>(OnMoveRequest);
        EventBus.Unsubscribe<NavigationStopRequestEvent>(OnStopRequest);
    }

    private void OnMoveRequest(NavigationMoveRequestEvent e)
    {
        var req = e.request;
        Debug.Log($"[NavigationService] 收到移动请求 agentId={req.agentId}, target={req.targetPosition}");

        if (!NavigationRegistry.Instance.TryGet(req.agentId, out INavigationAgent agent))
        {
            Debug.LogWarning("[NavigationService] 未找到导航对象: " + req.agentId);
            if (req.agentId == "Player")
            {
                currentPlayerVisualState = new NavigationVisualState
                {
                    hasPath = false,
                    isReachable = false,
                    pathPoints = null,
                    destination = req.targetPosition
                };
            }
            return;
        }

        Debug.Log("[NavigationService] 已找到导航对象: " + agent.AgentId);

        if (!pathSolver.TryBuildPath(agent.AgentTransform.position, req.targetPosition, out Vector3 sampledTarget, out Vector3[] corners))
        {
            Debug.LogWarning("[NavigationService] 路径计算失败。");
            if (req.agentId == "Player")
            {
                currentPlayerVisualState = new NavigationVisualState
                {
                    hasPath = false,
                    isReachable = false,
                    pathPoints = null,
                    destination = req.targetPosition
                };
            }
            return;
        }

        Debug.Log("[NavigationService] 路径计算成功，角点数 = " + corners.Length);

        if (req.agentId == "Player")
        {
            currentPlayerVisualState = new NavigationVisualState
            {
                hasPath = true,
                isReachable = true,
                pathPoints = corners,
                destination = sampledTarget
            };
        }

        agent.SetPath(corners, req.stopDistance);

    }

    private void OnStopRequest(NavigationStopRequestEvent e)
    {
        if (NavigationRegistry.Instance.TryGet(e.agentId, out INavigationAgent agent))
            agent.StopNavigation();

        if (e.agentId == "Player")
        {
            currentPlayerVisualState = new NavigationVisualState
            {
                hasPath = false,
                isReachable = true,
                pathPoints = null,
                destination = Vector3.zero
            };
        }
    }
}
