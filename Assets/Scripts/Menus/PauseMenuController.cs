using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenuController : MonoBehaviourPunCallbacks
{
    public GameObject pauseCanvas;
    public GameObject gameCanvas;
    public bool isPaused = false;
    bool isLeaving = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseCanvas.SetActive(true);
        gameCanvas.SetActive(false);
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LeaveMatch()
    {
        if (isLeaving)
        {
            return;
            //Prevents spam
        }
        isLeaving = true;
        Time.timeScale = 1f;
        PhotonNetwork.LeaveRoom();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeMatch()
    {
        isPaused = false;
        pauseCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}