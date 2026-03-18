public class NavigationService
{
    private static readonly NavigationService instance = new NavigationService();
    public static NavigationService Instance => instance;

    private readonly NavigationPathSolver pathSolver = new NavigationPathSolver();
    private bool initialized;

    public void Init()
    {
        if (initialized) return;
        initialized = true;
        EventBus.Subscribe<NavigationMoveRequestEvent>(OnMoveRequest);
        EventBus.Subscribe<NavigationStopRequestEvent>(OnStopRequest);
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
        if (!NavigationRegistry.Instance.TryGet(req.agentId, out INavigationAgent agent)) return;
        if (!pathSolver.TryBuildPath(agent.AgentTransform.position, req.targetPosition, out var corners)) return;
        agent.SetPath(corners, req.stopDistance);
    }

    private void OnStopRequest(NavigationStopRequestEvent e)
    {
        if (NavigationRegistry.Instance.TryGet(e.agentId, out INavigationAgent agent))
            agent.StopNavigation();
    }
}
