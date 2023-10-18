using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponSway : NetworkBehaviour {

    public GameObject gun;

    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;
    [SerializeField] private float originalRotationY;

    private bool live = true;

    private void Update()
    {
        if (!IsOwner || Cursor.lockState == CursorLockMode.None || !live)
        {
            return;
        }

        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * multiplier + originalRotationY;
        float mouseY = Input.GetAxisRaw("Mouse Y") * multiplier;

        // calculate target rotation
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        // rotate 
        gun.transform.localRotation = Quaternion.Slerp(gun.transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }

    public void Despawn()
    {
        live = false;
    }

    public void Respawn()
    {
        live = true;
    }
}
