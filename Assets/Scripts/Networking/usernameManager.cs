using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class usernameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameField;

   private void Start()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            // Load and apply saved username
            string savedUsername = PlayerPrefs.GetString("username");
            usernameField.text = savedUsername;
            PhotonNetwork.NickName = savedUsername;
        }
        else
        {
            // Generate a random username for first-time players
            string randomUsername = "Player: " + Random.Range(1, 1000).ToString("0000");
            usernameField.text = randomUsername;
            PhotonNetwork.NickName = randomUsername;
            PlayerPrefs.SetString("username", randomUsername);
        }
    }

    // Call this when the user manually updates their username input
    public void OnUsernameInputValueChange()
    {
        string newUsername = usernameField.text;

        if (!string.IsNullOrWhiteSpace(newUsername))
        {
            PhotonNetwork.NickName = newUsername;
            PlayerPrefs.SetString("username", newUsername);
        }
    }
}