using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Transform orientation;

    [SerializeField] private Camera mainCam;
    [SerializeField] private Camera weaponCam;
    [SerializeField] private AudioListener audioListener;

    [SerializeField] private float sensX; // 5
    [SerializeField] private float sensY; // 5

    private float xRotation;
    private float yRotation;
    private bool live;

    private void Start()
    {
        if (!IsOwner)
        {
            mainCam.enabled = false;
            weaponCam.enabled = false;
            audioListener.enabled = false;
            return;
        }

        Billboard.cam = transform;
        Despawn();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (!live || Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 80f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void Despawn()
    {
        live = false;
    }

    public void Respawn()
    {
        live = true;
        xRotation = 0;
        yRotation = 90;
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
