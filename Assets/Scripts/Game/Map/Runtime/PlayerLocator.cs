using UnityEngine;

public class PlayerLocator
{
    private static readonly PlayerLocator instance = new PlayerLocator();
    public static PlayerLocator Instance => instance;

    private Transform playerTransform;

    private PlayerLocator() { }

    public void Register(Transform target)
    {
        playerTransform = target;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public void Clear()
    {
        playerTransform = null;
    }
}
