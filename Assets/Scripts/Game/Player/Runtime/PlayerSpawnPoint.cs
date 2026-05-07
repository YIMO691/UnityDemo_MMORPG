using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    private static PlayerSpawnPoint instance;

    private void Awake()
    {
        instance = this;
    }

    public static Vector3 GetSpawnPosition()
    {
        if (instance == null) return Vector3.zero;
        return instance.transform.position;
    }
}
