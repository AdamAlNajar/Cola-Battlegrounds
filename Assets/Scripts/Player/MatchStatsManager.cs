using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
public class MatchStatsManager : MonoBehaviour
{
    public static MatchStatsManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public Player GetTopKiller()
    {
        Player topPlayer = null;
        int highestKills = -1;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int kills = player.CustomProperties.ContainsKey("Kills") ? (int)player.CustomProperties["Kills"] : 0;
            if (kills > highestKills)
            {
                highestKills = kills;
                topPlayer = player;
            }
        }

        return topPlayer;
    }

    public Team GetTopTeam()
    {
        Dictionary<Team, int> teamKills = new Dictionary<Team, int>
        {
            { Team.Cola, 0 },
            { Team.Pepsi, 0 }
        };

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int kills = player.CustomProperties.ContainsKey("Kills") ? (int)player.CustomProperties["Kills"] : 0;

            if (player.CustomProperties.TryGetValue("Team", out object teamObj))
            {
                Team team = (Team)System.Enum.Parse(typeof(Team), teamObj.ToString());
                teamKills[team] += kills;
            }
        }

        return teamKills[Team.Cola] >= teamKills[Team.Pepsi] ? Team.Cola : Team.Pepsi;
    }
}