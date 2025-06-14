using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;
    private Dictionary<Player, Team> teamAssignments = new Dictionary<Player, Team>();
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AssignTeams()
    {
        if (GameModeManager.Instance.GetCurrentGameMode() != GameMode.TDM) return;

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Team assignedTeam = (i % 2 == 0) ? Team.Cola : Team.Pepsi;
            teamAssignments[players[i]] = assignedTeam;
            ExitGames.Client.Photon.Hashtable teamProp = new ExitGames.Client.Photon.Hashtable
            {
                { "Team", assignedTeam.ToString() }
            };
            players[i].SetCustomProperties(teamProp);
        }

        Debug.Log("[TeamManager] Teams assigned.");
    }
    
    public Team GetPlayerTeam(Player player)
    {
        if (player.CustomProperties.TryGetValue("Team", out object teamValue))
        {
            return (Team)System.Enum.Parse(typeof(Team), teamValue.ToString());
        }

        return Team.None;
    }
}