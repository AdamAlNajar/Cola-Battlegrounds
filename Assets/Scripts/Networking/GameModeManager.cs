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
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "AK47_Ammo"), akAmmoSpawnPos.position, akAmmoSpawnPos.rotation);
            PhotonNetwork.InstantiateRoomObject(Path.Combine("PhotonPrefabs", "Shotgun_Ammo"), shotAmmoSpawnPos.position, shotAmmoSpawnPos.rotation);
        }
    }
    

    public void DetermineGameMode()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Map 1":
                currentGameMode = GameMode.FFA;
                break;
            case "Map 2":
                currentGameMode = GameMode.TDM;
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
}