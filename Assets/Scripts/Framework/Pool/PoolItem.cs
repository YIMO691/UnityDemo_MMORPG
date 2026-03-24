using UnityEngine;

public class PoolItem : MonoBehaviour
{
    public string PoolKey;

    public void Recycle()
    {
        PoolManager.Instance.Recycle(gameObject);
    }
}