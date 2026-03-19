using System;

[Serializable]
public class PlayerRuntimeData
{
    public int currentHp;
    public int currentStamina;
    public bool isDead;
    public string Scene;

    public float posX;
    public float posY;
    public float posZ;

    public float rotY;

    public bool hasValidPosition;
}
