using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public static Transform cam;

    void LateUpdate()
    {
        if (cam)
        {
            transform.LookAt(transform.position + cam.forward);
        }
    }
}
