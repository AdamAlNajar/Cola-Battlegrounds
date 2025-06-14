using UnityEngine;
using UnityEngine.SceneManagement;
public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;
    public GameMode currentGameMode;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DetermineGameMode();
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

        Debug.Log("[GameModeManager] Game Mode Set To: " + currentGameMode);
    }

    public GameMode GetCurrentGameMode()
    {
        return currentGameMode;
    }
}