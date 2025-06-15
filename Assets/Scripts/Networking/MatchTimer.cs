using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class MatchTimer : MonoBehaviourPunCallbacks
{
    public float matchTime = 60f; // in SEC. 
    public TMP_Text matchTimerText;
    public float timer;
    public bool isMatchActive;
    public PhotonView PV;
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            timer = matchTime;
            isMatchActive = true;
        }
    }
    private void Update()
    {
        if (!isMatchActive)
            return;
        if (PhotonNetwork.IsMasterClient)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                isMatchActive = false;
                PV.RPC(nameof(RPC_EndMatch), RpcTarget.All);
            }
        }
        UpdateTimerUI();
    }
    [PunRPC]
    public void RPC_EndMatch()
    {
        // Later
        Debug.Log("Match Ended!");
    }
    void UpdateTimerUI()
    {
        if (matchTimerText == null) return;

        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        matchTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}