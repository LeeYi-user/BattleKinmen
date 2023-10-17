using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MouseLook : NetworkBehaviour // 這個腳本跟網路有關, 所以要用 NetworkBehavior
{
    // 該檔案是用來抓取攝影機, 請把它放在玩家物件之下
    // 因為攝影機物件不是在玩家物件之下, 所以必須在玩家生成後另外抓取
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        // 如果當前的玩家物件不是自己, 就直接 return
        // 如果不加這行, 每當有新玩家進來時, 攝影機都會被重新抓取, 白白浪費效能, 還可能引起 Bug
        if (!IsOwner)
        {
            return;
        }

        GameObject.Find("MainCamera").GetComponent<PlayerCamera>().orientation = orientation;
        GameObject.Find("CameraHolder").GetComponent<MoveCamera>().cameraPosition = cameraPosition;
    }
}
