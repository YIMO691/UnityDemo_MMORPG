using UnityEngine;

public class PlayerEntity : MonoBehaviour
{
    public string RuntimeId { get; private set; }
    public PlayerData Data { get; private set; }

    public void Init(PlayerData data, string runtimeId)
    {
        Data = data;
        RuntimeId = runtimeId;
    }

    // 最小快照能力：与现有保存链对齐（不改链路，只提供便捷入口）
    public void CaptureRuntimeSnapshot()
    {
        if (Data == null) return;
        if (Data.runtimeData == null) Data.runtimeData = new PlayerRuntimeData();
        var t = transform;
        Vector3 pos = t.position;
        Vector3 rot = t.eulerAngles;
        Data.runtimeData.hasValidPosition = true;
        Data.runtimeData.posX = pos.x;
        Data.runtimeData.posY = pos.y;
        Data.runtimeData.posZ = pos.z;
        Data.runtimeData.rotY = rot.y;
        Data.runtimeData.Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    public void ApplyRuntimeSnapshot()
    {
        if (Data == null || Data.runtimeData == null || !Data.runtimeData.hasValidPosition) return;
        transform.position = new Vector3(Data.runtimeData.posX, Data.runtimeData.posY, Data.runtimeData.posZ);
        transform.rotation = Quaternion.Euler(0f, Data.runtimeData.rotY, 0f);
    }
}
