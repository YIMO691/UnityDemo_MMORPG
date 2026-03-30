using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class RuntimeLifecycleRegistry
{
    private sealed class Entry
    {
        public string Name;
        public Action Init;
        public Action ResetSession;
        public Action Shutdown;
    }

    private static readonly RuntimeLifecycleRegistry instance = new RuntimeLifecycleRegistry();
    public static RuntimeLifecycleRegistry Instance => instance;

    private readonly List<Entry> entries = new List<Entry>();
    private readonly HashSet<string> registeredNames = new HashSet<string>();

    private RuntimeLifecycleRegistry() { }

    public void Register(string name, Action init, Action resetSession, Action shutdown)
    {
        if (registeredNames.Contains(name))
            return;

        entries.Add(new Entry
        {
            Name = name,
            Init = init,
            ResetSession = resetSession,
            Shutdown = shutdown
        });

        registeredNames.Add(name);
    }

    public void InitAll()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            Invoke("Init", entries[i].Name, entries[i].Init);
        }
    }

    public void ResetSessionAll()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            Invoke("ResetSession", entries[i].Name, entries[i].ResetSession);
        }
    }

    public void ShutdownAll()
    {
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            Invoke("Shutdown", entries[i].Name, entries[i].Shutdown);
        }
    }

    private static void Invoke(string phase, string name, Action action)
    {
        if (action == null)
            return;

        try
        {
            action();
            Debug.Log($"[RuntimeLifecycle] {phase} -> {name}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RuntimeLifecycle] {phase} failed -> {name}\n{ex}");
        }
    }
}

public static class RuntimeLifecycleBootstrap
{
    private static bool registered;

    public static void RegisterDefaults()
    {
        if (registered)
            return;

        var registry = RuntimeLifecycleRegistry.Instance;

        registry.Register(
            nameof(DataManager),
            () => DataManager.Instance.Init(),
            () => DataManager.Instance.ClearCurrentSlotId(),
            () => DataManager.Instance.Clear());

        registry.Register(
            nameof(UIManager),
            () => UIManager.Instance.Init(),
            null,
            () => UIManager.Instance.Clear());

        registry.Register(
            nameof(RoleDataManager),
            () => RoleDataManager.Instance.Init(),
            null,
            null);

        registry.Register(
            nameof(MapDataManager),
            () => MapDataManager.Instance.Init(),
            null,
            null);

        registry.Register(
            nameof(CreateRoleFlowController),
            () => CreateRoleFlowController.Instance.Init(),
            null,
            () => CreateRoleFlowController.Instance.Clear());

        registry.Register(
            nameof(RoleUIController),
            () => RoleUIController.Instance.Init(),
            null,
            null);

        registry.Register(
            nameof(NavigationService),
            () => NavigationService.Instance.Init(),
            null,
            () => NavigationService.Instance.Dispose());

        registry.Register(
            nameof(ItemConfigManager),
            () => ItemConfigManager.Instance.Init(),
            null,
            null);

        registry.Register(
            nameof(DropTableConfigManager),
            () => DropTableConfigManager.Instance.Init(),
            null,
            null);

        registry.Register(
            nameof(LootRuntimeService),
            () => LootRuntimeService.Init(),
            null,
            null);

        registry.Register(
            nameof(DeathRuntimeService),
            () => DeathRuntimeService.Init(),
            null,
            null);

        registry.Register(
            nameof(PlayerExpRuntimeService),
            () => PlayerExpRuntimeService.Init(),
            null,
            null);

        registry.Register(
            nameof(PlayerDeathUIController),
            () => PlayerDeathUIController.Init(),
            null,
            null);

        registered = true;
    }
}
