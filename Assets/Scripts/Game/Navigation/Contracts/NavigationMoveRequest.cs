using UnityEngine;

public struct NavigationMoveRequest
{
    public string agentId;
    public Vector3 targetPosition;
    public float stopDistance;

    public NavigationMoveRequest(string agentId, Vector3 targetPosition, float stopDistance = 0.25f)
    {
        this.agentId = agentId;
        this.targetPosition = targetPosition;
        this.stopDistance = stopDistance;
    }
}
