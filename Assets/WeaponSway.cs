using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponSway : NetworkBehaviour
{
    // 元件用途: 操控武器擺動
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private GameObject gun;

    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;
    [SerializeField] private float originalRotationY;

    private bool live = true;

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner || Cursor.lockState == CursorLockMode.None || !live)
        {
            return;
        }

        float mouseX = Input.GetAxisRaw("Mouse X") * multiplier + originalRotationY;
        float mouseY = Input.GetAxisRaw("Mouse Y") * multiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetRotation = rotationX * rotationY;

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
