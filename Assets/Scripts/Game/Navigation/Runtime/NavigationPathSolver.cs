using UnityEngine;
using UnityEngine.AI;

public class NavigationPathSolver
{
    public bool TryBuildPath(Vector3 startPos, Vector3 rawTargetPos, out Vector3[] corners)
    {
        corners = null;
        Vector3 targetPos = rawTargetPos;
        if (!NavMesh.SamplePosition(rawTargetPos, out NavMeshHit hit, 6f, NavMesh.AllAreas))
            return false;
        targetPos = hit.position;
        NavMeshPath path = new NavMeshPath();
        bool ok = NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path);
        if (!ok || path.status != NavMeshPathStatus.PathComplete || path.corners == null || path.corners.Length == 0)
            return false;
        corners = path.corners;
        return true;
    }
}
