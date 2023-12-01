using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    // 元件用途: 旋轉浮空文字
    // 元件位置: 任一浮空文字物件(world space canvas)之下

    public static Transform cam;

    void LateUpdate()
    {
        if (cam)
        {
            transform.LookAt(transform.position + cam.forward);
        }
    }
}
