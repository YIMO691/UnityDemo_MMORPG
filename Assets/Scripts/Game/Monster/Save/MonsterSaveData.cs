using System;

[Serializable]
public class MonsterSaveData
{
    public string runtimeId;
    public int configId;
    public string sceneName;
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
    public int currentHp;
    public bool isDead;
    public string spawnPointId;
}
