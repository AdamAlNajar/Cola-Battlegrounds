using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;


public class TeamManager : MonoBehaviourPunCallbacks
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
        List<Player> players = new List<Player>(PhotonNetwork.PlayerList);

        foreach (Player player in players)
        {
            if (!player.CustomProperties.ContainsKey("Team"))
            {
                Team teamToAssign = GetBalancedTeam();
                PhotonHashtable teamProp = new PhotonHashtable { { "Team", teamToAssign.ToString() } };
                player.SetCustomProperties(teamProp);
                Debug.Log($"[TeamManager] Assigned team {teamToAssign} to player {player.NickName}");
            }
        }
    }

    private Team GetBalancedTeam()
    {
        // Your balancing logic here. Example: alternate teams.
        int colaCount = 0;
        int pepsiCount = 0;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue("Team", out object teamVal))
            {
                if (teamVal.ToString() == Team.Cola.ToString()) colaCount++;
                else if (teamVal.ToString() == Team.Pepsi.ToString()) pepsiCount++;
            }
        }

        return colaCount <= pepsiCount ? Team.Cola : Team.Pepsi;
    }

    public Team GetPlayerTeam(Player player)
    {
        if (player.CustomProperties.TryGetValue("Team", out object teamValue))
        {
            return (Team)System.Enum.Parse(typeof(Team), teamValue.ToString());
        }
        return Team.None; // or default team
    }
}