using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public PlayerBaseData baseData;
    public PlayerProgressData progressData;
    public PlayerAttributeData attributeData;
    public PlayerRuntimeData runtimeData;
    public List<MonsterSaveData> monsterData;

    public string saveTime;
}
