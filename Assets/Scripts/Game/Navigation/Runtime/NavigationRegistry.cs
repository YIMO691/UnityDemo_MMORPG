using System.Collections.Generic;

public class NavigationRegistry
{
    private static readonly NavigationRegistry instance = new NavigationRegistry();
    public static NavigationRegistry Instance => instance;

    private readonly Dictionary<string, INavigationAgent> agents = new Dictionary<string, INavigationAgent>();

    public void Register(INavigationAgent agent)
    {
        if (agent == null || string.IsNullOrEmpty(agent.AgentId)) return;
        agents[agent.AgentId] = agent;
    }

    public void Unregister(INavigationAgent agent)
    {
        if (agent == null || string.IsNullOrEmpty(agent.AgentId)) return;
        if (agents.TryGetValue(agent.AgentId, out var current) && current == agent)
            agents.Remove(agent.AgentId);
    }

    public bool TryGet(string agentId, out INavigationAgent agent)
    {
        return agents.TryGetValue(agentId, out agent);
    }
}
