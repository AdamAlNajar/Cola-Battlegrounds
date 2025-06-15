using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;
public class MatchTimer : MonoBehaviourPunCallbacks,IPunObservable
{
    public float matchTime = 60f; // in SEC. 
    public TMP_Text matchTimerText;
    public float timer;
    public bool isMatchActive;
    public PhotonView PV;
    public GameObject gamePlayCanvas;
    public GameObject deathCanvas;
    public GameObject endOfMatchCanvas;
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
        Debug.Log("Match Ended!");
        ShowMatchEndScreen();
    }
    void UpdateTimerUI()
    {
        if (matchTimerText == null) return;

        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        matchTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void LeaveMatch()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }
    public void ShowMatchEndScreen()
    {
        // Step 1 : close all other canvases and show only end of match
        gamePlayCanvas.SetActive(false);
        endOfMatchCanvas.SetActive(true);
        deathCanvas.SetActive(false);
        // Step 2 : Unlock cursor to allow going to next level
        FindObjectOfType<PlayerController>().enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    //This method is better for syncing frequent updates to not flood photons servers
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            // MASTER IS WRITING ON BOARD
            stream.SendNext(timer);
        }
        else
        {
            // STUDENTS COPYING FROM BOARD
            timer = (float)stream.ReceiveNext();
            UpdateTimerUI();
        }
    }
}