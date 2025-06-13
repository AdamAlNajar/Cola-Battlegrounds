using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunCamRotRel : MonoBehaviour
{
    public Transform cameraTransform;

    private Quaternion initialLocalRotation;

    void Start()
    {
        initialLocalRotation = transform.localRotation;
        if (cameraTransform == null)
        {
            Debug.LogWarning("[GunCamRotRel] cameraTransform is not assigned!");
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;
        Quaternion camRot = cameraTransform.rotation;

        transform.rotation = camRot * initialLocalRotation;
    }
}
