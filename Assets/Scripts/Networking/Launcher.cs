using System.Collections;
using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using Photon.Realtime;
using System.Linq;

// This Manages Connection To Photons Master Server
// Basically, Manages connection to rooms
public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;
    [SerializeField] TMP_InputField roomNameField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //Connect to master Lobby
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to master server");
    }

    public override void OnConnectedToMaster()
    {
        // Join the Master Lobby
        PhotonNetwork.JoinLobby();
        Debug.Log("Connecting to lobby");
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        // Show Master lobby (title screen)
        Debug.Log("Joined Lobby");
        MenuManager.instance.OpenMenu("title menu");
        SFXManager.Instance.PlayMusic("Main Track");
        if (DiscordRPCManager.Instance != null)
        {
            DiscordRPCManager.Instance.ChangeStatus("In Menu", "Browsing Lobbies");
        }
        else
        {
            Debug.Log("DiscordRPCManager is null [Launcher]");
        }
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameField.text);
        MenuManager.instance.OpenMenu("loading");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("title menu");
    }

    public override void OnJoinedRoom()
    {
        // Show room
        MenuManager.instance.OpenMenu("room menu");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        //Clear existing player list UI items to prevent duplicates
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(players[i]);
        }
        startGameButton.SetActive(PhotonNetwork.IsMasterClient); // Only show start game button to room host
        if (DiscordRPCManager.Instance != null)
        {
            DiscordRPCManager.Instance.ChangeStatus("In Room", $"Room: {PhotonNetwork.CurrentRoom.Name}");
        }
        else
        {
            Debug.Log("DiscordRPCManager is null [Launcher]");
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("loading");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // Show Error screen when the room creation is failed
        MenuManager.instance.OpenMenu("error menu");
        errorText.text = "Room Creation failed for the following reason : " + message + returnCode;
    }

    public void QuitApplication()
    {
        Debug.Log("APPLICATION QUIT WITH CODE 0");
        Application.Quit();
        //this closes the game
    }

    public void StartGame()
    {
        // Loads a random map
        PhotonNetwork.LoadLevel(Random.Range(1,3));
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }
}