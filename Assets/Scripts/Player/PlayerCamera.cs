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

    public float xRotation;
    public float yRotation;

    public bool disable;

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
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)) && Cursor.lockState == CursorLockMode.None && !disable)
        {
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * MenuManager.sens * 10;
        float mouseY = Input.GetAxis("Mouse Y") * MenuManager.sens * 10;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void Spawn()
    {
        if (GameManager.gameOver)
        {
            return;
        }

        xRotation = 0;
        yRotation = 90;
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
