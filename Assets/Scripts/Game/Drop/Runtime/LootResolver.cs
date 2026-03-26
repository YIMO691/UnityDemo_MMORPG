using System.Collections.Generic;
using UnityEngine;

public static class LootResolver
{
    public static List<LootDropResult> Resolve(int dropTableId)
    {
        var results = new List<LootDropResult>();

        var table = DropTableConfigManager.Instance.GetConfig(dropTableId);
        if (table == null || table.entries == null || table.entries.Count == 0)
        {
            return results;
        }

        int totalWeight = 0;
        for (int i = 0; i < table.entries.Count; i++)
        {
            var e = table.entries[i];
            if (e != null && e.weight > 0) totalWeight += e.weight;
        }
        if (totalWeight <= 0) return results;

        int roll = Random.Range(1, totalWeight + 1);
        int acc = 0;
        for (int i = 0; i < table.entries.Count; i++)
        {
            var entry = table.entries[i];
            if (entry == null || entry.weight <= 0) continue;
            acc += entry.weight;
            if (roll > acc) continue;

            int min = Mathf.Max(1, entry.minCount);
            int max = Mathf.Max(min, entry.maxCount);
            int count = Random.Range(min, max + 1);
            results.Add(new LootDropResult { itemId = entry.itemId, count = count });
            break;
        }
        return results;
    }
}
