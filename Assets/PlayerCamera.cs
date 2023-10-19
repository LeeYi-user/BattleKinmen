using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    // 該檔案是用來控制相機旋轉的, 請把它放在 SampleScene 的 CameraHolder 的 MainCamera 之下
    [SerializeField] private Transform orientation; // 這個變數會用來記錄旋轉角度, 在計算玩家移動時會用到
    [SerializeField] private Transform cam;

    [SerializeField] private float sensX; // X 軸靈敏度
    [SerializeField] private float sensY; // Y 軸靈敏度

    float xRotation; // X 軸旋轉角
    float yRotation; // Y 軸旋轉角

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            cam.GetComponent<Camera>().enabled = false;
            cam.GetComponent<AudioListener>().enabled = false;
            return;
        }

        InitScene.cam = cam;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        // 要先等 MouseLook 腳本抓到 orientation 後才能開始執行
        if (!orientation)
        {
            return;
        }
        // 取得滑鼠移動輸入
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;
        // 計算旋轉後的角度
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 這行是要避免轉過頭
        // 更新攝影機的實際旋轉角度
        cam.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); // orientation 只要記錄玩家在 xz 平面的面向就好, 這樣就能計算移動
    }
}
