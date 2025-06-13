using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
public class Gun : MonoBehaviour
{
    // Basic Properties
    public float damage = 5f;
    public bool isAuto;
    public float range = 100f;
    float nextTimeToFire = 0f;
    public float fireRate;
    //Extra Properties
    public ParticleSystem shootParticleEffect;
    public ScreenShake scShake;
    public Camera gunCam;
    PhotonView pv; // Networking
    // Ammo Vars
    [SerializeField] int currentAmmo;
    [SerializeField] int maxAmmo;
    public float reloadTime;
    public int addedAmmo;
    bool isReloading;
    //GUI
    public TMP_Text ammoText;
    public TMP_Text reserveAmmoText;
    void Awake()
    {
        pv = GetComponentInParent<PhotonView>();
        currentAmmo = maxAmmo;
    }
    void Update()
    {
        if (!pv.IsMine)
            return;
        UpdateAmmoUI();
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
    private IEnumerator Reload()
    {
        isReloading = true; // Set to true to prevent reloading while already reloading.

        yield return new WaitForSeconds(reloadTime);

        // Add ammo to currentAmmo and subtract from addedAmmo.
        currentAmmo += addedAmmo;
        addedAmmo -= addedAmmo;

        isReloading = false;
    }

    void Shoot()
    {
        currentAmmo--;
        scShake.TriggerShake(0.09f);
        shootParticleEffect.Play();
        if (!pv.IsMine)
            return;
        RaycastHit hit;
        if (Physics.Raycast(gunCam.transform.position, gunCam.transform.forward, out hit, range) && pv.IsMine)
        {
            PhotonView pvHit = hit.collider.GetComponent<PhotonView>();
            // Prevent hitting yourself
            if (pvHit != null && pvHit.IsMine)
            {
                Debug.Log("[Gun] Hit self — ignoring.");
                return;
            }

            // Safely attempt to damage the target
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, PhotonNetwork.LocalPlayer.NickName);
            }
            else
            {
                Debug.Log("[Gun] Hit non-damageable object: " + hit.collider.name);
            }
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
}