using UnityEngine;
using Photon.Pun;
using TMPro;
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
    }
}