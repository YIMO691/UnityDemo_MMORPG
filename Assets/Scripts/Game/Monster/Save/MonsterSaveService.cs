using System.Collections.Generic;

public class MonsterSaveService
{
    public List<MonsterSaveData> CaptureScene()
    {
        var list = new List<MonsterSaveData>();
        var monsters = MonsterRuntimeRegistry.Instance.GetAll();
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] == null) continue;
            list.Add(monsters[i].BuildSaveData());
        }
        return list;
    }

    public void RestoreScene(List<MonsterSaveData> dataList)
    {
        if (dataList == null) return;
        for (int i = 0; i < dataList.Count; i++)
        {
            var data = dataList[i];
            if (data == null) continue;
            MonsterRuntimeService.RestoreFromSave(data);
        }
    }
}
