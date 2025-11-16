using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;
public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode currentGameMode;
    public Transform akAmmoSpawnPos;
    public Transform shotAmmoSpawnPos;
    public Transform pepsiSpawnPos;
    public Transform colaSpawnPos;
    public GameObject gameUI;
    public GameObject deathUI;
    public GameObject loadingUI;
    public bool ammoNeeded;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DetermineGameMode();

        if (PhotonNetwork.IsMasterClient)
        {
            if (ammoNeeded)
            {
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "AK47_Ammo"), akAmmoSpawnPos.position, akAmmoSpawnPos.rotation);
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Shotgun_Ammo"), shotAmmoSpawnPos.position, shotAmmoSpawnPos.rotation);
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Cola"), colaSpawnPos.position, colaSpawnPos.rotation);
                PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Pepsi"), pepsiSpawnPos.position, pepsiSpawnPos.rotation);
            }
        }
        Loading();
    }


    public void DetermineGameMode()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Map 1":
                currentGameMode = GameMode.FFA;
                DiscordRPCManager.Instance.ChangeStatus("Playing a match", "FFA");
                break;
            case "Map 2":
                currentGameMode = GameMode.TDM;
                DiscordRPCManager.Instance.ChangeStatus("Playing a match", "TDM");
                break;
            case "Map 3":
                currentGameMode = GameMode.FFA;
                DiscordRPCManager.Instance.ChangeStatus("Playing a match", "FFA");
                break;
            default:
                Debug.LogWarning("Scene name does not match a known map, defaulting to FFA.");
                currentGameMode = GameMode.FFA;
                break;
        }

    }

    public GameMode GetCurrentGameMode()
    {
        return currentGameMode;
    }
    void Loading()
    {
        gameUI.SetActive(false);
        deathUI.SetActive(false);
        loadingUI.SetActive(true);
        Invoke(nameof(Hide), 3f);
    }
    void Hide()
    {
        gameUI.SetActive(true);
        deathUI.SetActive(true);
        loadingUI.SetActive(false);
    }
}