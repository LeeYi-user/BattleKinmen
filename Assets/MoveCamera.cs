using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    // 該檔案是用來讓攝影機移動到玩家位置, 請把它放在 SampleScene 的 CameraHolder 之下
    public Transform cameraPosition;

    // Update is called once per frame
    void Update()
    {
        // 要先等 MouseLook 腳本抓到攝影機之後才能開始執行
        if (!cameraPosition)
        {
            return;
        }

        transform.position = cameraPosition.position;
    }
}
