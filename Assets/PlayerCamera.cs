using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    // 該檔案是用來控制相機旋轉的
    [SerializeField] private Transform orientation; // 這個變數會用來記錄旋轉角度, 在計算玩家移動時會用到
    [SerializeField] private Transform mainCam;
    [SerializeField] private Transform weaponCam;

    [SerializeField] private float sensX; // X 軸靈敏度
    [SerializeField] private float sensY; // Y 軸靈敏度

    float xRotation; // X 軸旋轉角
    float yRotation; // Y 軸旋轉角
    bool live;

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

        InitScene.cam = mainCam;
        live = true;
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

        // 取得滑鼠移動輸入
        float mouseX = Input.GetAxis("Mouse X") * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * sensY;
        // 計算旋轉後的角度
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 這行是要避免轉過頭
        // 更新攝影機的實際旋轉角度
        mainCam.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); // orientation 只要記錄玩家在 xz 平面的面向就好, 這樣就能計算移動
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
        mainCam.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
