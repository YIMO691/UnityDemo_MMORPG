using UnityEngine;
using System.Collections.Generic;

public static class DeathRuntimeService
{
    private static bool initialized;
    private static readonly HashSet<int> processed = new HashSet<int>();

    public static void Init()
    {
        if (initialized) return;
        initialized = true;
        EventBus.Subscribe<DeathEvent>(OnDeath);
    }

    private static void OnDeath(DeathEvent evt)
    {
        if (evt == null || evt.deadEntity == null) return;
        var comp = evt.deadEntity as Component;
        if (comp == null) return;
        int id = comp.GetInstanceID();
        if (processed.Contains(id)) return;
        processed.Add(id);

        // Monster cleanup
        var monster = comp.GetComponent<MonsterEntity>();
        if (monster != null)
        {
            // 停止导航并注销
            var nav = monster.GetNavigator();
            if (nav != null)
            {
                nav.StopNavigation();
                NavigationRegistry.Instance.Unregister(nav);
            }
            MonsterRuntimeRegistry.Instance.Unregister(monster);

            // 播放死亡动画
            var anim = monster.GetAnimatorDriver();
            anim?.SetDead();

            // 关闭碰撞，防止尸体干扰
            var col = comp.GetComponent<Collider>();
            if (col) col.enabled = false;

            // 延迟销毁（2秒，可按动画时长调整）
            Object.Destroy(monster.gameObject, 2f);

            Debug.Log("[DeathRuntimeService] Monster cleaned and scheduled for destruction.");
            return;
        }

        // Player cleanup (minimal)
        var player = comp.GetComponent<PlayerEntity>();
        if (player != null)
        {
            Debug.Log("[DeathRuntimeService] Player dead.");
            // Hook for animation/UI lock later
            return;
        }
    }
}
