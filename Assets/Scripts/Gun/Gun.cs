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
    public int currentAmmo;
    [SerializeField] int maxAmmo;
    public float reloadTime;
    public int addedAmmo;
    bool isReloading;
    [Header("UI")]
    public TMP_Text ammoText;
    public TMP_Text reserveAmmoText;
    public GameObject friendlyFireWarning;
    [Header("Other")]
    Coroutine damageBoostRoutine;
    float originalDamage;
    Coroutine currentReloadRoutine;
    public GameObject model;
    public bool canShoot = true;
    public bool inEnv = false;
    void Awake()
    {
        pv = GetComponent<PhotonView>(); // The guns pv
        playerPV = GetComponentInParent<PhotonView>(); // The guns pv
        if (pv == null)
            Debug.LogError("[Gun] PhotonView is NULL! RPCs won't work.");
        originalDamage = damage;
        if(model != null)
        {
            model.SetActive(true);
        }
    }
    public void InitializeAmmo(int ammo, int reserve)
    {
        currentAmmo = ammo;
        addedAmmo = reserve;
    }
    void Update()
    {
        if (inEnv)
            return;
        if (!playerPV.IsMine)
            return;
        UpdateAmmoUI();
        if (isReloading)
            return;
        if (currentAmmo <= 10 && Input.GetKeyDown(KeyCode.R))
        {
            if (currentReloadRoutine == null)
                currentReloadRoutine = StartCoroutine(Reload());
        }
        if (currentAmmo <= 0)
            return;
        //Auto Guns
        if (Input.GetMouseButton(0) && isAuto && Time.time >= nextTimeToFire && canShoot)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        //Manual Guns
        if (Input.GetMouseButtonDown(0) && Time.time >= nextTimeToFire && !isAuto && canShoot)
        {
            Shoot();
        }
    }
    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, addedAmmo);

        currentAmmo += ammoToReload;
        addedAmmo -= ammoToReload;

        isReloading = false;
        currentReloadRoutine = null;
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

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "env" && model != null)
        {
            model.SetActive(false);
            canShoot = false;
            inEnv = true;
            Debug.Log("hit env");
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "env" && model != null)
        {
            model.SetActive(true);
            canShoot = true;
            inEnv = false;
            Debug.Log("left env");
        }
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
    void OnDisable()
    {
        if (currentReloadRoutine != null)
        {
            StopCoroutine(currentReloadRoutine);
            currentReloadRoutine = null;
            isReloading = false; // Reset state
        }
    }
}