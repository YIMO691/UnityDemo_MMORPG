using System.Collections.Generic;
using UnityEngine;

public static class ItemUseEffectSystem
{
    private static readonly Dictionary<string, IItemUseEffect> dict = new Dictionary<string, IItemUseEffect>();
    private static bool inited;

    public static void Init()
    {
        if (inited) return;
        Register("heal_hp", new HealHpEffect());
        Register("restore_stamina", new RestoreStaminaEffect());
        inited = true;
    }

    public static void Register(string key, IItemUseEffect effect)
    {
        if (string.IsNullOrEmpty(key) || effect == null) return;
        dict[key] = effect;
    }

    public static bool Apply(ItemConfig cfg, PlayerEntity player)
    {
        if (!inited) Init();
        if (cfg == null || player == null) return false;
        if (string.IsNullOrEmpty(cfg.itemType)) return false;
        if (!dict.TryGetValue(cfg.itemType, out var eff)) return false;
        return eff.Apply(player, cfg);
    }

    public interface IItemUseEffect
    {
        bool Apply(PlayerEntity player, ItemConfig cfg);
    }

    private class HealHpEffect : IItemUseEffect
    {
        public bool Apply(PlayerEntity player, ItemConfig cfg)
        {
            if (player == null || player.Data == null) return false;
            var attr = player.Data.attributeData;
            var runtime = player.Data.runtimeData;
            if (attr == null || runtime == null) return false;
            int max = Mathf.Max(0, attr.maxHp);
            if (max <= 0) return false;
            int value = cfg != null && cfg.useValue > 0 ? cfg.useValue : 50;
            int old = runtime.currentHp;
            runtime.currentHp = Mathf.Clamp(old + value, 0, max);
            EventBus.Publish(new PlayerHpChangedEvent(runtime.currentHp, max));
            if (runtime.currentHp > 0) runtime.isDead = false;
            return runtime.currentHp != old;
        }
    }

    private class RestoreStaminaEffect : IItemUseEffect
    {
        public bool Apply(PlayerEntity player, ItemConfig cfg)
        {
            if (player == null || player.Data == null) return false;
            var attr = player.Data.attributeData;
            var runtime = player.Data.runtimeData;
            if (attr == null || runtime == null) return false;
            int max = Mathf.Max(0, attr.maxStamina);
            if (max <= 0) return false;
            int value = cfg != null && cfg.useValue > 0 ? cfg.useValue : 30;
            int old = runtime.currentStamina;
            runtime.currentStamina = Mathf.Clamp(old + value, 0, max);
            EventBus.Publish(new PlayerStaminaChangedEvent(runtime.currentStamina, max));
            return runtime.currentStamina != old;
        }
    }
}
