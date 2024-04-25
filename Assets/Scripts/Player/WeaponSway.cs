using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;
    [SerializeField] private float originalRotationZ;

    private void Update()
    {
        float mouseX = 0;
        float mouseY = 0;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseX += Input.GetAxisRaw("Mouse X") * multiplier;
            mouseY += Input.GetAxisRaw("Mouse Y") * multiplier;
        }

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion rotationZ = Quaternion.AngleAxis(originalRotationZ, Vector3.forward);
        Quaternion targetRotation = rotationX * rotationY * rotationZ;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}
