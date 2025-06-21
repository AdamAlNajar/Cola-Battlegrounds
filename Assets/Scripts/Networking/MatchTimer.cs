using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
public class MatchTimer : MonoBehaviourPunCallbacks,IPunObservable
{
    public static MatchTimer Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Avoid duplicate MatchTimer instances
            
        PV = GetComponent<PhotonView>(); 
    }
    public float matchTime = 60f; // in SEC. 
    public TMP_Text matchTimerText;
    public TMP_Text winner;
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
            PV.RPC(nameof(RPC_StartMatch), RpcTarget.AllBuffered);
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
    [PunRPC]
    void RPC_StartMatch()
    {
        isMatchActive = true;
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
        base.OnLeftRoom();
        SceneManager.LoadScene("Menu");
        PhotonNetwork.JoinLobby();
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
        if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.FFA)
        {
            Player topPlayer = MatchStatsManager.Instance.GetTopKiller();
            winner.text = "Winner of match - " + topPlayer.NickName;
        }
        else if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
        {
            Team topTeam = MatchStatsManager.Instance.GetTopTeam();
            winner.text = "Winning Team of match - " + topTeam;
        }
    }
    //This method is better for syncing frequent updates to not flood photons servers
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            // MASTER IS WRITING ON BOARD, TELLING STUDENTS TO COPY
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