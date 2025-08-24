using TMPro;
using UnityEngine;
using Photon.Realtime;
public class LeaderboardItem : MonoBehaviour
{
    public TMP_Text usernameTXT;
    public TMP_Text killsTXT;
    public TMP_Text deathsTXT;
    public void initialize(Player player)
    {
        usernameTXT.text = player.NickName;
    }
}