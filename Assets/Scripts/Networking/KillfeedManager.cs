using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Linq;
public class KillfeedManager : MonoBehaviourPun
{
    public static KillfeedManager Instance;
    //UI Links
    public GameObject killfeedItemPrefab;
    public Transform killfeedItemParent;
    void Awake()
    {
        Instance = this;
    }

    [PunRPC]
    public void RPC_GetKill(string _killer, string _attakced)
    {
        GameObject item = Instantiate(killfeedItemPrefab, killfeedItemParent);
        item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{_killer} Killed {_attakced}";
        Destroy(item, 2f);

        Photon.Realtime.Player killer = PhotonNetwork.PlayerList.FirstOrDefault(p => p.NickName == _killer);
        if (killer != null && killer == PhotonNetwork.LocalPlayer)
        {
            AddKillToPlayer(killer);
        }
    }
    void AddKillToPlayer(Photon.Realtime.Player killer)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();

        int currentKills = 0;
        if (killer.CustomProperties.ContainsKey("Kills"))
        {
            currentKills = (int)killer.CustomProperties["Kills"];
        }

        props["Kills"] = currentKills + 1;
        killer.SetCustomProperties(props);
    }
}