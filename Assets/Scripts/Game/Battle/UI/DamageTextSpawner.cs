using UnityEngine;

public static class DamageTextSpawner
{
    public static void Show(int value, Vector3 worldPos, Transform parent = null)
    {
        var go = PoolManager.Instance.Spawn(PoolKey.DamageText, parent);
        if (go == null) return;
        var dt = go.GetComponent<DamageText>();
        if (dt != null) dt.Play(value, worldPos);
    }
}
