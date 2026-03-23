using UnityEngine;

public static class NavigationAgentAssembler
{
    public static PlayerNavigator EnsurePlayerNavigator(GameObject player, string agentId = NavigationConsts.PlayerAgentId)
    {
        if (player == null)
        {
            Debug.LogWarning("[NavigationAgentAssembler] player 为 null。");
            return null;
        }
        var nav = player.GetComponent<PlayerNavigator>();
        if (nav == null) nav = player.AddComponent<PlayerNavigator>();
        nav.SetAgentId(agentId);
        return nav;
    }
}
