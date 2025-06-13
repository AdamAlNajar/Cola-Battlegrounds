using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;
using System.Collections;

public class GunSwitch : MonoBehaviour
{
    public int currentWeaponIndex = 0;
    private PhotonView photonView;
    public Camera playerCam;
    public float[] weaponFOVs;
    public float fovTransitionSpeed = 5f;
    private Coroutine fovCoroutine;
    private void Start()
    {
        photonView = GetComponentInParent<PhotonView>();
        SelectWeapon();
    }

    private void Update()
    {
        if (photonView == null || !photonView.IsMine)
            return; // Only allow input on the local player
        int prevSelectedWeapon = currentWeaponIndex;
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (currentWeaponIndex >= transform.childCount - 1)
                currentWeaponIndex = 0;
            else
                currentWeaponIndex++;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (currentWeaponIndex <= 0)
                currentWeaponIndex = transform.childCount - 1;
            else
                currentWeaponIndex--;
        }
        if (prevSelectedWeapon != currentWeaponIndex)
        {
            SelectWeapon();
            // 🔄 Sync weapon index across network
            Hashtable hash = new Hashtable();
            hash.Add("currentWeaponIndex", currentWeaponIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
    public void SelectWeapon()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == currentWeaponIndex);
        }
        UpdateCameraFOV();
    }
    private void UpdateCameraFOV()
    {
        if (playerCam != null && currentWeaponIndex < weaponFOVs.Length)
        {
            if (fovCoroutine != null)
                StopCoroutine(fovCoroutine);
            fovCoroutine = StartCoroutine(ChangeFOV(weaponFOVs[currentWeaponIndex]));
        }
    }

    private IEnumerator ChangeFOV(float targetFOV)
    {
        while (Mathf.Abs(playerCam.fieldOfView - targetFOV) > 0.1f)
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
            yield return null;
        }
        playerCam.fieldOfView = targetFOV;
    }
}