using System.IO;
using UnityEngine;
using Photon.Pun;
using System.Collections;
// The class that manages player data, respawning and death, and RPC
public class PlayerManager : MonoBehaviour
{
    PhotonView photonView;
    GameObject playerController;
    public float spawnDelay = 3f;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(CreateControllerWithDelay(spawnDelay));
        }
    }

    IEnumerator CreateControllerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Transform spawnpoint = SpawnManager.Instance.getSpawnPoint();

        playerController = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "PlayerController"),
            spawnpoint.position,
            Quaternion.identity,
            0,
            new object[] { photonView.ViewID });

        if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
        {
            Team myTeam = TeamManager.Instance.GetPlayerTeam(PhotonNetwork.LocalPlayer);
            Debug.Log("[PlayerManager] Player assigned to team: " + myTeam);

            PhotonView controllerPhotonView = playerController.GetComponent<PhotonView>();
            if (controllerPhotonView != null)
            {
                controllerPhotonView.RPC("RPC_SetTeamColor", RpcTarget.AllBuffered, (int)myTeam);
            }
        }
        Debug.Log("[PlayerManager] Player spawned.");
    }
    public void Die(string _victimName)
    {
        //Step 1 : Show death screen
        if (photonView.IsMine)
        {
            var deathUI = DeathController.Instance;
            if (deathUI != null)
            {
                deathUI.ShowDeathCanvas();
            }
            else
            {
                Debug.Log("DeathController NULL");
            }
        }
        //Step 2 : Delete player
        PhotonNetwork.Destroy(playerController);
        //Step 3 : Show kill on Killfeed
        KillfeedManager.Instance.photonView.RPC(
        "RPC_GetKill",
        RpcTarget.All,
        _victimName,
        PhotonNetwork.LocalPlayer.NickName
        );
        //Step 4 : Spawn Player again
        StartCoroutine(CreateControllerWithDelay(spawnDelay));
    }
    
    public Team GetTeam()
    {
        return TeamManager.Instance.GetPlayerTeam(PhotonNetwork.LocalPlayer);
    }
}