using UnityEngine;

public interface INavigationAgent
{
    string AgentId { get; }
    Transform AgentTransform { get; }
    bool IsNavigating { get; }
    void SetPath(Vector3[] pathPoints, float stopDistance);
    void StopNavigation();
}
