using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
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
    public TMP_Text powerUpText;
    public TMP_Text teamText;
    public GameObject teamTextOBJ;

    [Header("Gun Objects")]
    public GameObject kalashnikovOBJ;
    public GameObject shotGunOBJ;
    public GunSwitch gunSwitch;

    [Header("Other")]
    public Renderer objRenderer;
    public GameObject cameraHolder;
    public float originalWalkingSpeed;
    public float originalRunningSpeed;
    private Coroutine colaSpeedBoostRoutine;

    [HideInInspector] public float currentHealth;
    [HideInInspector] public bool canMove = true;
    public Team team;
    private CharacterController characterController;
    private new PhotonView photonView;
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
            Destroy(GetComponentInChildren<Camera>().gameObject);
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
                SetRandomColor_FFA();
                break;

            case GameMode.TDM:
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
                {
                    Team myTeam = (Team)System.Enum.Parse(typeof(Team), teamValue.ToString());
                    this.team = myTeam;
                    photonView.RPC(nameof(RPC_SetTeamColor), RpcTarget.AllBuffered, (int)myTeam);
                    // Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Assigned to team : " + myTeam);
                }
                else
                {
                    Debug.LogWarning("[PlayerController] Team not set in CustomProperties for TDM.");
                }
                break;
        }

        // Longer delay for safety on late joiners
        Invoke(nameof(SyncTeamFromCustomProperties), 2f);
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
        if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team", out object teamValue))
            {
                Team myTeam = (Team)System.Enum.Parse(typeof(Team), teamValue.ToString());
                teamTextOBJ.SetActive(true);
                teamText.text = "TEAM: " + myTeam;
            }
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
            // Vertical rotation (look up/down)
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            cameraHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

            // Horizontal rotation (player turns left/right)
            float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
            transform.Rotate(Vector3.up * mouseX);
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
    public void SetRandomColor_FFA()
    {
        // This method makes it so that players get either red or blue color in ffa mode
        // Only owner should generate and sync color
        // the below runs locally, but the rpc runs remotely
        if (!photonView.IsMine) return;
        Color finalColor = Color.red;
        int magicNum = Random.Range(1, 3);
        switch (magicNum)
        {
            case 1:
                if (objRenderer != null)
                    finalColor = Color.red;
                break;
            case 2:
                if (objRenderer != null)
                    finalColor = Color.blue;
                break;
        }
        if (objRenderer != null)
            objRenderer.material.color = finalColor;
        photonView.RPC(nameof(RPC_SetFFAColor), RpcTarget.OthersBuffered, finalColor.r, finalColor.g, finalColor.b);
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
                int ammoToAdd = Random.Range(10, 81); // Random Value cuz why not lol
                akGun.addedAmmo += ammoToAdd;
                SFXManager.Instance.PlaySFX("Power Up");
                powerUpText.gameObject.SetActive(true);
                powerUpText.text = "Added " + ammoToAdd + " AK bullets";
                Helper_HideGameObjectAfterDelay(powerUpText.gameObject, 3f);
                PhotonView otherView = other.GetComponent<PhotonView>();
                if (otherView != null)
                {
                    if (otherView.IsMine || PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(otherView.gameObject);
                    }
                    else
                    {
                        photonView.RPC("RPC_RequestDestroyObject", RpcTarget.MasterClient, otherView.ViewID);
                    }
                }
            }
            if (other.CompareTag("Shotgun_Ammo"))
            {
                var shotGun = shotGunOBJ.GetComponent<Gun>();
                int ammoToAdd = Random.Range(10, 81); // Random Value cuz why not lol
                shotGun.addedAmmo += ammoToAdd;
                SFXManager.Instance.PlaySFX("Power Up");
                powerUpText.gameObject.SetActive(true);
                powerUpText.text = "Added " + ammoToAdd + " Shotty bullets";
                Helper_HideGameObjectAfterDelay(powerUpText.gameObject, 3f);
                PhotonView otherView = other.GetComponent<PhotonView>();
                if (otherView != null)
                {
                    if (otherView.IsMine || PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(otherView.gameObject);
                    }
                    else
                    {
                        photonView.RPC("RPC_RequestDestroyObject", RpcTarget.MasterClient, otherView.ViewID);
                    }
                }
            }
            if (other.CompareTag("Cola"))
            {
                float boostAmount = 5f;
                float boostDuration = 10f;

                if (colaSpeedBoostRoutine != null)
                    StartCoroutine(ColaSpeedBoost(boostAmount, boostDuration));
                colaSpeedBoostRoutine = StartCoroutine(ColaSpeedBoost(boostAmount, boostDuration));
                powerUpText.gameObject.SetActive(true);
                powerUpText.text = "Speed Boost for 30 SEC";
                Helper_HideGameObjectAfterDelay(powerUpText.gameObject, 3f);
                PhotonView otherView = other.GetComponent<PhotonView>();
                if (otherView != null)
                {
                    if (otherView.IsMine || PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(otherView.gameObject);
                    }
                    else
                    {
                        photonView.RPC("RPC_RequestDestroyObject", RpcTarget.MasterClient, otherView.ViewID);
                    }
                }
            }
            if (other.CompareTag("Pepsi"))
            {
                float damageBoost = 20f; // Increase damage by 20
                float duration = 30f;

                // Apply power-up only to the currently active weapon
                if (kalashnikovOBJ.activeSelf)
                {
                    kalashnikovOBJ.GetComponent<Gun>().Helper_ActivateDamageBoost(damageBoost, duration);
                }
                else if (shotGunOBJ.activeSelf)
                {
                    shotGunOBJ.GetComponent<Gun>().Helper_ActivateDamageBoost(damageBoost, duration);
                }
                powerUpText.gameObject.SetActive(true);
                powerUpText.text = "+20 Damage for 30 SEC";
                Helper_HideGameObjectAfterDelay(powerUpText.gameObject, 3f);
                PhotonView otherView = other.GetComponent<PhotonView>();
                if (otherView != null)
                {
                    if (otherView.IsMine || PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Destroy(otherView.gameObject);
                    }
                    else
                    {
                        photonView.RPC("RPC_RequestDestroyObject", RpcTarget.MasterClient, otherView.ViewID);
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
        photonView.RPC(nameof(RPC_TakeDamage), photonView.Owner, damage, attackerName);
        SFXManager.Instance.PlaySFX("Take Damage");
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage, string attackerName)
    {
        if (!photonView.IsMine)
            return;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            SFXManager.Instance.PlaySFX("Death");
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
    void RPC_RequestDestroyObject(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);

        if (targetView != null)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
    }
    [PunRPC]
    public void RPC_SetFFAColor(float r, float g, float b)
    {
        Color color = new Color(r, g, b);
        if (objRenderer != null)
        {
            objRenderer.material.color = color;
        }
    }
    [PunRPC]
    public void RPC_GiveKillAmmo()
    {
        if (!photonView.IsMine) return;

        if (kalashnikovOBJ.activeSelf)
        {
            var gun = kalashnikovOBJ.GetComponent<Gun>();
            gun.addedAmmo += 15;
        }
        else if (shotGunOBJ.activeSelf)
        {
            var gun = shotGunOBJ.GetComponent<Gun>();
            gun.addedAmmo += 5;
        }

        powerUpText.gameObject.SetActive(true);
        powerUpText.text = "+Ammo for Kill!";
        Helper_HideGameObjectAfterDelay(powerUpText.gameObject, 3f);
        SFXManager.Instance.PlaySFX("Power Up");
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
    private IEnumerator ColaSpeedBoost(float boostAmount, float duration)
    {
        // Save original speeds
        originalWalkingSpeed = walkingSpeed;
        originalRunningSpeed = runningSpeed;

        // Apply boosted speed
        walkingSpeed += boostAmount;
        runningSpeed += boostAmount;

        //Add visual feedback or sound
        SFXManager.Instance.PlaySFX("Power Up");

        yield return new WaitForSeconds(duration);

        // Revert to original speeds
        walkingSpeed = originalWalkingSpeed;
        runningSpeed = originalRunningSpeed;
    }
    public void Helper_HideGameObjectAfterDelay(GameObject gObject, float delay)
    {
        StartCoroutine(HideAfterDelay(gObject, delay));
    }

    IEnumerator HideAfterDelay(GameObject gObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        gObject.SetActive(false);
    }
}