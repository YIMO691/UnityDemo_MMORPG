using UnityEngine;

public class PlayerLocator
{
    private static readonly PlayerLocator instance = new PlayerLocator();
    public static PlayerLocator Instance => instance;

    private Transform playerTransform;
    private PlayerEntity playerEntity;

    private PlayerLocator() { }

    public void Register(Transform target)
    {
        playerTransform = target;
    }

    public void Register(PlayerEntity entity)
    {
        playerEntity = entity;
        playerTransform = entity != null ? entity.transform : null;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public PlayerEntity GetPlayerEntity()
    {
        return playerEntity;
    }

    public void Clear()
    {
        playerTransform = null;
        playerEntity = null;
    }
}
