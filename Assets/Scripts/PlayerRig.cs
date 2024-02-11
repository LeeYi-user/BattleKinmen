using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerRig : NetworkBehaviour
{
    [SerializeField] private PlayerCamera playerCamera;

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        transform.rotation = Quaternion.Euler(Mathf.Clamp(playerCamera.xRotation, -90f, 75f), playerCamera.yRotation, 0);
    }
}
