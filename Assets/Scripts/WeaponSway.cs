using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;
    [SerializeField] private float baseRotationX;
    [SerializeField] private float baseRotationZ;

    private void Update()
    {
        float mouseX = 0f;
        float mouseY = baseRotationX;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseX += Input.GetAxisRaw("Mouse X") * multiplier;
            mouseY += Input.GetAxisRaw("Mouse Y") * multiplier;
        }

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion rotationZ = Quaternion.AngleAxis(baseRotationZ, Vector3.forward);
        Quaternion targetRotation = rotationX * rotationY * rotationZ;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}
