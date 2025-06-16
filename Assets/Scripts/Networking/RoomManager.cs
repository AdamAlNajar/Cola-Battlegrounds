using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;
    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    //*Fun Fact:
    // OnEnable and OnDisable are the only photon methods that need to call the base callback*//

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1 || scene.buildIndex == 2)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
            GameModeManager.Instance.DetermineGameMode();

            if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM && PhotonNetwork.IsMasterClient)
            {
                Invoke(nameof(AssignTeams), 3f);
            }
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("RoomManager detected OnLeftRoom");
        Destroy(gameObject);
        SceneManager.LoadScene("Menu");
    }

    public void AssignTeams()
    {
        TeamManager.Instance.AssignTeams();
        Debug.Log("[RoomManager] Assigned teams (after short delay).");
    }

     // New: Handle late joiners by assigning team immediately
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[RoomManager] New player joined: " + newPlayer.NickName);

        if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
        {
            TeamManager.Instance.AssignTeams(); // Assigns only if team not set
        }
    }
}