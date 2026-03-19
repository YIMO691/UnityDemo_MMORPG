using UnityEngine;
using UnityEngine.AI;

public class NavigationPathSolver
{
    public bool TryBuildPath(Vector3 rawStartPos, Vector3 rawTargetPos, out Vector3 sampledTargetPos, out Vector3[] corners)
    {
        corners = null;
        sampledTargetPos = rawTargetPos;

        if (!TrySampleToNavMesh(rawStartPos, out Vector3 startPos))
        {
            Debug.LogWarning($"[NavigationPathSolver] 起点采样失败 rawStartPos={rawStartPos}");
            return false;
        }

        if (!TrySampleToNavMesh(rawTargetPos, out Vector3 targetPos))
        {
            Debug.LogWarning($"[NavigationPathSolver] 终点采样失败 rawTargetPos={rawTargetPos}");
            return false;
        }

        sampledTargetPos = targetPos;

        NavMeshPath path = new NavMeshPath();
        bool ok = NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path);

        Debug.Log(
            $"[NavigationPathSolver] CalculatePath ok={ok}, status={path.status}, " +
            $"start={startPos}, target={targetPos}, corners={(path.corners == null ? 0 : path.corners.Length)}"
        );

        if (!ok || path.status != NavMeshPathStatus.PathComplete || path.corners == null || path.corners.Length == 0)
            return false;

        corners = path.corners;
        return true;
    }

    private bool TrySampleToNavMesh(Vector3 rawPos, out Vector3 sampledPos)
    {
        Vector3 probePos = rawPos + Vector3.up * 2f;

        if (NavMesh.SamplePosition(probePos, out NavMeshHit hit, 20f, NavMesh.AllAreas))
        {
            sampledPos = hit.position;
            return true;
        }

        sampledPos = rawPos;
        return false;
    }
}
