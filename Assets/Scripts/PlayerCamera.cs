using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    // 元件用途: 操控玩家攝影機
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform mainCam;
    [SerializeField] private Transform weaponCam;

    [SerializeField] private float sensX; // 5
    [SerializeField] private float sensY; // 5

    private float xRotation;
    private float yRotation;
    private bool live;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            mainCam.GetComponent<Camera>().enabled = false;
            mainCam.GetComponent<AudioListener>().enabled = false;
            weaponCam.gameObject.SetActive(false);
            return;
        }

        Billboard.cam = mainCam;

        Despawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Cursor.lockState == CursorLockMode.None || !live)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        mainCam.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void Despawn()
    {
        live = false;
    }

    public void Respawn()
    {
        xRotation = 0;
        yRotation = 0;
        mainCam.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        live = true;
    }
}
