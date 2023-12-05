using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    // 元件用途: 操控玩家攝影機
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform weaponCam;
    [SerializeField] private Transform model;

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
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
            weaponCam.gameObject.SetActive(false);
            return;
        }

        Billboard.cam = transform;
        Despawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            model.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
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
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);
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
        yRotation = 0;
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
