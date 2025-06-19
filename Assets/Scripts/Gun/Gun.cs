using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Properties")]
    public float damage = 5f;
    public bool isAuto;
    public float range = 100f;
    float nextTimeToFire = 0f;
    public float fireRate;
    [Header("Effects")]
    public ParticleSystem shootParticleEffect;
    public ScreenShake scShake;
    public Camera gunCam;
    public GameObject bulletImpactPrefab;
    PhotonView pv, playerPV; // Networking
    [Header("Ammo")]
    [SerializeField] int currentAmmo;
    [SerializeField] int maxAmmo;
    public float reloadTime;
    public int addedAmmo;
    bool isReloading;
    [Header("UI")]
    public TMP_Text ammoText;
    public TMP_Text reserveAmmoText;
    public GameObject friendlyFireWarning;
    [Header("Anti-Clipping")]
    public Transform weaponModel; // Assign the part of the gun mesh to hide
    public float clipCheckDistance = 0.5f; // Distance from camera to check wall
    Vector3 defaultLocalPos;

    [Header("Other")]
    Coroutine damageBoostRoutine;
    float originalDamage;
    void Awake()
    {
        pv = GetComponent<PhotonView>(); // The guns pv
        playerPV = GetComponentInParent<PhotonView>(); // The guns pv
        if (pv == null)
            Debug.LogError("[Gun] PhotonView is NULL! RPCs won't work.");
        currentAmmo = maxAmmo;
        defaultLocalPos = weaponModel.localPosition;
        damage = originalDamage;
    }
    void Update()
    {
        if (!playerPV.IsMine)
            return;
        UpdateAmmoUI();
        HandleClipping();
        if (currentAmmo <= 10 && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }
        if (currentAmmo <= 0)
            return;
        //Auto Guns
        if (Input.GetMouseButton(0) && isAuto && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        //Manual Guns
        if (Input.GetMouseButtonDown(0) && Time.time >= nextTimeToFire && !isAuto)
        {
            Shoot();
        }
    }
    void HandleClipping()
    {
        RaycastHit hit;
        Vector3 origin = gunCam.transform.position;
        Vector3 direction = gunCam.transform.forward;

        if (Physics.Raycast(origin, direction, out hit, clipCheckDistance))
        {
            // Push weapon back
            weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, defaultLocalPos - new Vector3(0, 0, 0.2f), Time.deltaTime * 10f);
        }
        else
        {
            // Reset to original position
            weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, defaultLocalPos, Time.deltaTime * 10f);
        }
    }
    private IEnumerator Reload()
    {
        isReloading = true; // Set to true to prevent reloading while already reloading.
        yield return new WaitForSeconds(reloadTime);
        // Add ammo to currentAmmo and subtract from addedAmmo.
        currentAmmo += addedAmmo;
        addedAmmo = 0;
        isReloading = false;
    }

    void Shoot()
    {
        if (!playerPV.IsMine)
            return;
        if (!MatchTimer.Instance.isMatchActive)
            return;
        currentAmmo--;
        scShake.TriggerShake(0.09f);
        pv.RPC(nameof(RPC_ShootEffects), RpcTarget.All);
        SFXManager.Instance.PlaySFX("Shoot");
        RaycastHit hit;
        if (Physics.Raycast(gunCam.transform.position, gunCam.transform.forward, out hit, range))
        {
            PhotonView hitPv = hit.collider.GetComponentInParent<PhotonView>();

            if (hitPv != null && hitPv.IsMine)
            {
                Debug.Log("[Gun] Hit self — ignoring.");
                return;
            }

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                if (GameModeManager.Instance.GetCurrentGameMode() == GameMode.TDM)
                {
                    var shooter = playerPV.Owner;
                    var target = hitPv?.Owner;

                    if (shooter != null && target != null &&
                        shooter.CustomProperties.TryGetValue("Team", out object shooterTeamObj) &&
                        target.CustomProperties.TryGetValue("Team", out object targetTeamObj))
                    {
                        Team shooterTeam = (Team)System.Enum.Parse(typeof(Team), shooterTeamObj.ToString());
                        Team targetTeam = (Team)System.Enum.Parse(typeof(Team), targetTeamObj.ToString());

                        Debug.Log($"[Gun] Shooter: {shooter.NickName}, Team: {shooterTeam} | Target: {target.NickName}, Team: {targetTeam}");

                        if (shooterTeam == targetTeam)
                        {
                            Debug.Log("[Gun] Friendly fire blocked.");
                            friendlyFireWarning.SetActive(true);
                            Invoke(nameof(Helper_hideFriendlyFireWarning), 3f);
                            return;
                        }
                    }
                }

                damageable.TakeDamage(damage, PhotonNetwork.LocalPlayer.NickName);
            }
            else
            {
                Debug.Log("[Gun] Hit non-damageable object: " + hit.collider.name);
                pv.RPC(nameof(RPC_ShootNonDamageable), RpcTarget.All, hit.point, hit.normal);
            }
        }

    }
    [PunRPC]
    void RPC_ShootNonDamageable(Vector3 hitPos, Vector3 hitNormal)
    {
        GameObject bulletImpact = Instantiate(bulletImpactPrefab, hitPos + hitNormal * 0.01f, Quaternion.LookRotation(-hitNormal));
        Destroy(bulletImpact, 5f);
    }
    [PunRPC]
    void RPC_ShootEffects()
    {
        if (shootParticleEffect != null)
            shootParticleEffect.Play();
    }
    void UpdateAmmoUI()
    {
        ammoText.color = Color.white;
        reserveAmmoText.text = addedAmmo.ToString("0000");

        // Update the ammo text
        ammoText.text = currentAmmo.ToString() + "/" + maxAmmo.ToString();

        // If ammo is low, display "Reload [R]" and change text color to red
        if (addedAmmo != 0 && currentAmmo <= 10)
        {
            ammoText.color = Color.red;
            ammoText.text = currentAmmo.ToString() + "/" + maxAmmo.ToString() + " Reload [R]";
        }
        else if (currentAmmo <= 10)  // This block ensures the color is red only when ammo is low
        {
            ammoText.color = Color.red;
        }

        // If reloading, display "Reloading..." and don't display ammo count
        if (isReloading)
        {
            ammoText.text = "Reloading...";
        }
    }
    void Helper_hideFriendlyFireWarning()
    {
        friendlyFireWarning.SetActive(false);
    }
    public void Helper_ActivateDamageBoost(float boostAmnt, float boostDuration)
    {
        if (damageBoostRoutine != null)
            StopCoroutine(damageBoostRoutine);
        damageBoostRoutine = StartCoroutine(damageBoost(boostAmnt, boostDuration));
    }
    IEnumerator damageBoost(float boostAmnt, float boostDuration)
    {
        damage = originalDamage;
        damage += boostAmnt;
        Debug.Log("Activated");
        SFXManager.Instance.PlaySFX("Power Up");
        yield return new WaitForSeconds(boostDuration);
        damage = originalDamage;
    }
}