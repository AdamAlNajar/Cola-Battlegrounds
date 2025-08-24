using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.PlayerLoop;
public class Leaderboard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] GameObject leaderboardItemPrefab;
    Dictionary<Player, LeaderboardItem> leaderboardItems = new Dictionary<Player, LeaderboardItem>();
    [SerializeField] CanvasGroup canvasGroup;
    void Start()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AddLeaderboardItem(player);
        }
    }
    void AddLeaderboardItem(Player player)
    {
        LeaderboardItem item = Instantiate(leaderboardItemPrefab, container).GetComponent<LeaderboardItem>();
        item.initialize(player);
        leaderboardItems[player] = item;
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddLeaderboardItem(newPlayer);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveLeaderboardItem(otherPlayer);
    }
    public void RemoveLeaderboardItem(Player player)
    {
        Destroy(leaderboardItems[player].gameObject);
        leaderboardItems.Remove(player);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            canvasGroup.alpha = 1;
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            canvasGroup.alpha = 0;
        }
    }
}