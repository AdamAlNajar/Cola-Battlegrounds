using System.Collections;
using TMPro;
using UnityEngine;

public class DeathController : MonoBehaviour
{
    public static DeathController Instance { get; private set; }
    public GameObject deathCanvas;
    public TMP_Text countdownText;
    public float countdown = 3f;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void ShowDeathCanvas()
    {
        deathCanvas.SetActive(true);
        Debug.Log("Death canvas shown.");
        Invoke(nameof(HideDeathCanvas), 3f);
    }

    public void HideDeathCanvas()
    {
        deathCanvas.SetActive(false);
        Debug.Log("Death canvas hidden.");
    }
}