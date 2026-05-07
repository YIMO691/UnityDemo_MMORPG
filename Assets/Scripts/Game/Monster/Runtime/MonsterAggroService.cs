using System.Collections.Generic;
using UnityEngine;

public static class MonsterAggroService
{
    public static void ClearAllTargets()
    {
        List<MonsterEntity> monsters = MonsterRuntimeRegistry.Instance.GetAll();
        if (monsters == null) return;
        for (int i = 0; i < monsters.Count; i++)
        {
            var m = monsters[i];
            if (m == null || m.IsDead) continue;
            m.ClearTarget();
            m.SetState(MonsterStateType.Return);
            var nav = m.GetNavigator();
            if (nav != null) nav.StopNavigation();
        }
        Debug.Log("[MonsterAggroService] All targets cleared");
    }
}
