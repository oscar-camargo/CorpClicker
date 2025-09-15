// SaveData.cs
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string version = "1.0.0";
    public long savedAtUnix;

    public double currentKPI;
    public double allTimeKPI;

    public int morale;
    public int reputation;

    public List<UpgradeState> upgrades = new(); // one per UpgradeData
}

[Serializable]
public class UpgradeState
{
    public string id;      // UpgradeData.Id
    public int level;      // for level-based
    public int purchases;  // for automation
}
