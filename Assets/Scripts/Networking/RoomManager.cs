using System.IO;
using Photon.Pun;
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
        /* if (scene.buildIndex == (Random.Range(1, 5)))
        {
        } */

        if (scene.buildIndex == 2)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);
            GameModeManager.Instance.DetermineGameMode();

            if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM && PhotonNetwork.IsMasterClient)
                TeamManager.Instance.AssignTeams();
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("RoomManager detected OnLeftRoom");
        Destroy(gameObject);
        SceneManager.LoadScene("Menu");
    }
}