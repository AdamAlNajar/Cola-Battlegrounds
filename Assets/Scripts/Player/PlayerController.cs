using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("Movement Settings")]
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    [Header("Camera & Look")]
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    [Header("Health & UI")]
    public float maxHealth = 100f;
    public Image healthBarImage;
    public GameObject gameplayCanvas;

    [Header("Gun Objects")]
    public GameObject kalashnikovOBJ;
    public GameObject shotGunOBJ;
    public GunSwitch gunSwitch;

    [Header("Other")]
    public Renderer objRenderer;
    public GameObject cameraHolder;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool canMove = true;
    [HideInInspector] public Team team;

    private CharacterController characterController;
    private PhotonView photonView;
    private PlayerManager playerManager;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        photonView = GetComponent<PhotonView>();

        //Remote Player
        if (!photonView.IsMine)
        {
            Camera cam = cameraHolder.GetComponentInChildren<Camera>();
            PostProcessLayer ppLayer = cameraHolder.GetComponentInChildren<PostProcessLayer>();
            Destroy(ppLayer);
            Destroy(cam);
            ////////////////////////
            Destroy(gameplayCanvas);
        }

        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            playerManager = PhotonView.Find((int)photonView.InstantiationData[0])?.GetComponent<PlayerManager>();
        }
        else
        {
            Debug.LogWarning("[PlayerController] InstantiationData is null. Probably a late joiner.");
        }

        currentHealth = maxHealth;

        if (!photonView.IsMine) return;

        GameMode currentMode = GameModeManager.Instance.GetCurrentGameMode();

        switch (currentMode)
        {
            case GameMode.FFA:
                int randomColorCode = Random.Range(1, 3);
                photonView.RPC(nameof(RPC_SetTeamColor), RpcTarget.AllBuffered, randomColorCode);
            break;

            case GameMode.TDM:
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    Team myTeam = (Team)System.Enum.Parse(typeof(Team), teamValue.ToString());
                    this.team = myTeam;
                    photonView.RPC(nameof(RPC_SetTeamColor), RpcTarget.AllBuffered, (int)myTeam);
                    Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Assigned to team : " + myTeam);
                }
                else
                {
                    Debug.LogWarning("[PlayerController] Team not set in CustomProperties for TDM.");
                }
            break;
        }

        // Longer delay for safety on late joiners
        Invoke(nameof(SyncTeamFromCustomProperties), 1f);
    }
    void Update()
    {
        if (!photonView.IsMine)
            return;
        if (!MatchTimer.Instance.isMatchActive)
            return;
        if (transform.position.y < -30f)
        {
            playerManager.Die(PhotonNetwork.LocalPlayer.NickName);
        }
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }


        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
    public void SetTeamColor(Team team)
    {
        Color teamColor = Color.red; // default color

        switch (team)
        {
            case Team.Cola:
                teamColor = Color.red;
                break;
            case Team.Pepsi:
                teamColor = Color.blue;
                break;
        }

        if (objRenderer != null)
        {
            objRenderer.material.color = teamColor;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        else
        {
            if (other.CompareTag("AK47_Ammo"))
            {
                var akGun = kalashnikovOBJ.GetComponent<Gun>();
                akGun.addedAmmo += Random.Range(10, 80); // Random Value cuz why not lol
                PhotonView otherView = other.GetComponent<PhotonView>();
                if (otherView != null)
                {
                    if (otherView.IsMine || PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(otherView.gameObject);
                    }
                    else
                    {
                        photonView.RPC("RPC_RequestDestroyAmmo", RpcTarget.MasterClient, otherView.ViewID);
                    }
                }
            }
            if (other.CompareTag("Shotgun_Ammo"))
            {
                var shotgunGun = shotGunOBJ.GetComponent<Gun>();
                shotgunGun.addedAmmo += Random.Range(10, 80); // Random Value cuz why not lol
                PhotonView otherView = other.GetComponent<PhotonView>();
                if (otherView != null)
                {
                    if (otherView.IsMine || PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(otherView.gameObject);
                    }
                    else
                    {
                        photonView.RPC("RPC_RequestDestroyAmmo", RpcTarget.MasterClient, otherView.ViewID);
                    }
                }
            }
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (photonView == null || gunSwitch == null)
            return;
        // Only apply to remote players
        if (!photonView.IsMine && targetPlayer == photonView.Owner)
        {
            if (changedProps.ContainsKey("currentWeaponIndex"))
            {
                gunSwitch.currentWeaponIndex = (int)changedProps["currentWeaponIndex"];
                gunSwitch.SelectWeapon();
            }
        }
        if (photonView.IsMine && targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey("Team"))
        {
            Team updatedTeam = (Team)System.Enum.Parse(typeof(Team), changedProps["Team"].ToString());
            photonView.RPC(nameof(RPC_SetTeamColor), RpcTarget.AllBuffered, (int)updatedTeam);
        }
        if (targetPlayer == photonView.Owner && changedProps.ContainsKey("Team"))
        {
            SyncTeamFromCustomProperties();
        }
    }
    public void TakeDamage(float damage, string attackerName)
    {
        // Proceed with damage if enemy
        photonView.RPC(nameof(RPC_TakeDamage), photonView.Owner, damage, attackerName);
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage, string attackerName)
    {
        currentHealth -= damage;
        SFXManager.Instance.PlaySFX("Take Damage");
        if (currentHealth <= 0)
        {
            playerManager.Die(attackerName);
        }
        healthBarImage.fillAmount = currentHealth / maxHealth;
    }
    [PunRPC]
    public void RPC_SetTeamColor(int teamInt)
    {
        Team team = (Team)teamInt;

        if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
            SetTeamColor(team);
        this.team = team;
    }
    [PunRPC]
    void RPC_RequestDestroyAmmo(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);

        if (targetView != null)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
    }

    void SyncTeamFromCustomProperties()
    {
        if (photonView.Owner.CustomProperties.TryGetValue("Team", out object teamValue))
        {
            Team newTeam = (Team)System.Enum.Parse(typeof(Team), teamValue.ToString());
            team = newTeam;  // ✅ Set local team
            SetTeamColor(newTeam);  // Optional
        }
    }
}
