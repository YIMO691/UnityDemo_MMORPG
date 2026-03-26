using System;
using System.Collections.Generic;

[Serializable]
public class InventoryData
{
    public int slotCount;
    public List<InventorySlotData> slots = new List<InventorySlotData>();
}

[Serializable]
public class InventorySlotData
{
    public int slotIndex;
    public int itemId;
    public int count;
}
