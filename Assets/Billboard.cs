using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    // 該檔案是用來讓 render mode 為 world space 的 canvas 可以永遠朝向攝影機
    // 以便讓浮空文字可以正常顯示
    // 請把他丟在有浮空文字的物件的 canvas 之下
    // 例如: Sample Scene 底下的 Instructor 底下的 Canvas
    public Transform cam;

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
