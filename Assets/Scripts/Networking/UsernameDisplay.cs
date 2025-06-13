using Photon.Pun;
using TMPro;
using UnityEngine;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] TMP_Text text;

    private void Start()
    {
        if(photonView.IsMine)
            text.gameObject.SetActive(false);
        text.text = photonView.Owner.NickName;
    }
}
