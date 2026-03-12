using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProgressData
{
    public int level;
    public int currentExp;
    public int expToNextLevel;
    public List<int> skillIds = new List<int>();
}
