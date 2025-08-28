using System.IO;
using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Linq;
// The class that manages player data, respawning and death, and RPC
public class PlayerManager : MonoBehaviour
{
    PhotonView photonView;
    GameObject playerController;
    public float spawnDelay = 3f;
    private int savedCurrentAmmo_AK = 55; // Default starting ammo
    private int savedReserveAmmo_AK = 75;
    private int savedCurrentAmmo_Shotgun = 50;
    private int savedReserveAmmo_Shotgun = 10;
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

        if (MatchTimer.Instance == null || !MatchTimer.Instance.isMatchActive)
        {
            Debug.Log("[PlayerManager] Respawn aborted — match is not active.");
            yield break;
        }
        Transform spawnpoint = SpawnManager.Instance.getSpawnPoint();
        if (MatchTimer.Instance.isMatchActive)
        {
            playerController = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "PlayerController"),
            spawnpoint.position,
            Quaternion.identity,
            0,
            new object[] { photonView.ViewID });
        }
        else
        {
            yield break;
        }
       
        var playerControllerSc = playerController.GetComponent<PlayerController>();
        if (playerControllerSc != null)
        {
            var ak = playerControllerSc.kalashnikovOBJ.GetComponent<Gun>();
            var sg = playerControllerSc.shotGunOBJ.GetComponent<Gun>();

            if (ak != null)
                ak.InitializeAmmo(savedCurrentAmmo_AK, savedReserveAmmo_AK);
            if (sg != null)
                sg.InitializeAmmo(savedCurrentAmmo_Shotgun, savedReserveAmmo_Shotgun);
        }
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
        var playerControllerSc = playerController.GetComponent<PlayerController>();
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
        
        // Step 1.5 : Save Ammo
        if (playerController != null)
        {
            var gunSwitch = playerControllerSc.GetComponentInChildren<GunSwitch>();
            if (gunSwitch != null)
            {
                var ak = playerControllerSc.kalashnikovOBJ.GetComponent<Gun>();
                var sg = playerControllerSc.shotGunOBJ.GetComponent<Gun>();

                savedCurrentAmmo_AK = ak.currentAmmo;
                savedReserveAmmo_AK = ak.addedAmmo;

                savedCurrentAmmo_Shotgun = sg.currentAmmo;
                savedReserveAmmo_Shotgun = sg.addedAmmo;
            }
        }
        //Step 2 : Delete player
        PhotonNetwork.Destroy(playerController);
        //Step 3 : Show kill on Killfeed
        KillfeedManager.Instance.photonView.RPC(
        "RPC_GetKill",
        RpcTarget.All,
        _victimName,
        PhotonNetwork.LocalPlayer.NickName// attacker
        );
        // Record Deaths
        Photon.Realtime.Player victim = PhotonNetwork.PlayerList.FirstOrDefault(p => p.NickName == _victimName);
        if (victim != null)
        {
            KillfeedManager.Instance.AddDeathToPlayer(PhotonNetwork.LocalPlayer);
        }
        //Step 3.5 : Reward killer with ammo on kill
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == _victimName)
            {
                GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
                foreach (var obj in playerObjects)
                {
                    PhotonView view = obj.GetComponent<PhotonView>();
                    if (view != null && view.Owner == player)
                    {
                        view.RPC("RPC_GiveKillAmmo", player); // call ammo RPC on killer
                        break;
                    }
                }
                break;
            }
        }
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