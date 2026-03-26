using System;
using System.Collections.Generic;

[Serializable]
public class ItemConfig
{
    public int id;
    public string name;
    public string iconPath;
    public int maxStack;
}

[Serializable]
public class ItemConfigList
{
    public List<ItemConfig> list = new List<ItemConfig>();
}
