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
    private void Start() {
        if (photonView.IsMine)
            StartCoroutine(CreateControllerWithDelay(spawnDelay));
    }
    IEnumerator CreateControllerWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // ✅ Check again after the delay
        if (MatchTimer.Instance == null || !MatchTimer.Instance.isMatchActive)
        {
            Debug.Log("[PlayerManager] Respawn aborted — match is not active.");
            yield break;
        }
        Transform spawnpoint = SpawnManager.Instance.getSpawnPoint();

        playerController = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "PlayerController"),
            spawnpoint.position,
            Quaternion.identity,
            0,
            new object[] { photonView.ViewID });

        if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                Team myTeam = (Team)System.Enum.Parse(typeof(Team), teamValue.ToString());

                PhotonView controllerPhotonView = playerController.GetComponent<PhotonView>();
                if (controllerPhotonView != null)
                {
                    controllerPhotonView.RPC("RPC_SetTeamColor", RpcTarget.AllBuffered, (int)myTeam);
                }
            }
            else
            {
                Debug.LogWarning("[PlayerManager] No team found in CustomProperties — fallback needed?");
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
            SFXManager.Instance.PlaySFX("Death");
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
        if (MatchTimer.Instance != null && MatchTimer.Instance.isMatchActive)
            StartCoroutine(CreateControllerWithDelay(spawnDelay));
        else
        {
            Debug.Log("[PlayerManager] Will Not spawn Player!");
        }
    }

    public Team GetTeam()
    {
        return TeamManager.Instance.GetPlayerTeam(PhotonNetwork.LocalPlayer);
    }
    void OnDisable()
    {
        if (playerController != null && playerController.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(playerController);
        }
    }
}