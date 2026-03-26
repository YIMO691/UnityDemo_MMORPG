using System;
using System.Collections.Generic;

[Serializable]
public class DropTableConfig
{
    public int id;
    public List<DropEntryConfig> entries = new List<DropEntryConfig>();
}

[Serializable]
public class DropEntryConfig
{
    public int itemId;
    public int minCount;
    public int maxCount;
    public int weight;
}

[Serializable]
public class DropTableConfigList
{
    public List<DropTableConfig> list = new List<DropTableConfig>();
}
