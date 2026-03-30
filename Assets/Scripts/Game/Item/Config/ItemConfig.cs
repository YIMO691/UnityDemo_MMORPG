using System;
using System.Collections.Generic;

[Serializable]
public class ItemConfig
{
    public int id;
    public string name;
    public string iconPath;
    public int maxStack;

    public string itemType;
    public bool canUse;
    public bool canDrop;
    public string desc;
    public int useValue;
}

[Serializable]
public class ItemConfigList
{
    public List<ItemConfig> list = new List<ItemConfig>();
}
