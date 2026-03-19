using TMPro;
using UnityEngine;

public class PoolMonitorItem : MonoBehaviour
{
    [SerializeField] private TMP_Text txtPoolKey;
    [SerializeField] private TMP_Text txtTotal;
    [SerializeField] private TMP_Text txtActive;
    [SerializeField] private TMP_Text txtInactive;
    [SerializeField] private TMP_Text txtCreate;
    [SerializeField] private TMP_Text txtSpawn;
    [SerializeField] private TMP_Text txtRecycle;

    public void Bind(PoolStats stats)
    {
        if (txtPoolKey != null) txtPoolKey.text = stats.PoolKey;
        if (txtTotal != null) txtTotal.text = stats.Total.ToString();
        if (txtActive != null) txtActive.text = stats.Active.ToString();
        if (txtInactive != null) txtInactive.text = stats.Inactive.ToString();
        if (txtCreate != null) txtCreate.text = stats.Create.ToString();
        if (txtSpawn != null) txtSpawn.text = stats.Spawn.ToString();
        if (txtRecycle != null) txtRecycle.text = stats.Recycle.ToString();

        // 简单高亮：有活跃对象时高亮
        if (txtActive != null)
            txtActive.color = stats.Active > 0 ? Color.yellow : Color.white;
    }
}
