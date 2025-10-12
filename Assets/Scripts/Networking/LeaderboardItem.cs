using TMPro;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
public class LeaderboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameTXT;
    public TMP_Text killsTXT;
    public TMP_Text deathsTXT;
    Player player;
    public void initialize(Player p)
    {
        player = p;
        usernameTXT.text = player.NickName;
        UpdateStats();
    }
    void UpdateStats()
    {
        int kills = player.CustomProperties.ContainsKey("Kills") ? (int)player.CustomProperties["Kills"] : 0;
        int deaths = player.CustomProperties.ContainsKey("Deaths") ? (int)player.CustomProperties["Deaths"] : 0;

        killsTXT.text = kills.ToString();
        deathsTXT.text = deaths.ToString();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == player && (changedProps.ContainsKey("Kills") || changedProps.ContainsKey("Deaths")))
        {
            UpdateStats();
        }
    }
}