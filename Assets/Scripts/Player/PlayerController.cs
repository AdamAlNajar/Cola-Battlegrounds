using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    PhotonView photonView;
    [HideInInspector]
    public bool canMove = true;
    PlayerManager playerManager;
    public GunSwitch gunSwitch;
    public float currentHealth;
    public float maxHealth;
    [SerializeField] Image healthBarImage;
    [SerializeField] GameObject gameplayCanvas;
    [SerializeField] GameObject kalashnikovOBJ;
    [SerializeField] GameObject shotGunOBJ;
    [SerializeField] Renderer objRenderer;
    public GameObject cameraHolder;
    [SerializeField] Team team;
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        photonView = GetComponent<PhotonView>();

        if (!photonView.IsMine)
        {
            Destroy(cameraHolder);// Destroys all cameras
            Destroy(gameplayCanvas);// Destroys gameplay canvas
        }

        playerManager = PhotonView.Find((int)photonView.InstantiationData[0]).GetComponent<PlayerManager>();

        currentHealth = maxHealth;
        // If game mode is FFA, set random color to each player to give effect of Pepsi and cola teams even though its FFA
        if (photonView.IsMine && GameModeManager.Instance.GetCurrentGameMode() == GameMode.FFA)
        {
            int randomNum = Random.Range(1, 3);
            photonView.RPC(nameof(RPC_SetTeamColor), RpcTarget.AllBuffered, randomNum);
        }
        else if (photonView.IsMine && GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
        {
            Team myTeam = playerManager.GetTeam();
            photonView.RPC(nameof(RPC_SetTeamColor), RpcTarget.AllBuffered, (int)myTeam);
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
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
                PhotonNetwork.Destroy(other.gameObject);
            }
            if (other.CompareTag("Shotgun_Ammo"))
            {
                var shotgunGun = shotGunOBJ.GetComponent<Gun>();
                shotgunGun.addedAmmo += Random.Range(10, 80); // Random Value cuz why not lol
                PhotonNetwork.Destroy(other.gameObject);
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
        SetTeamColor(team);
    }
}
