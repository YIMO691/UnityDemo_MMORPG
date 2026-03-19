using UnityEngine;

public struct NavigationVisualState
{
    public bool hasPath;
    public bool isReachable;
    public Vector3[] pathPoints;
    public Vector3 destination;
}
