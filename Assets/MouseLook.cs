using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MouseLook : NetworkBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        GameObject.Find("MainCamera").GetComponent<PlayerCamera>().orientation = orientation;
        GameObject.Find("CameraHolder").GetComponent<MoveCamera>().cameraPosition = cameraPosition;
    }
}
