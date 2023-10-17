using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SampleScene : MonoBehaviour
{
    // 該檔案是用來做場景控制的腳本
    // 請把它放在同名場景(SampleScene)底下的 For Script 物件
    // Start is called before the first frame update
    void Start()
    {
        if (InitScene.host) // 如果自已是房間主持人 (host)
        {
            NetworkManager.Singleton.StartHost(); // 就主持遊戲
        }
        else // 否則
        {
            NetworkManager.Singleton.StartClient(); // 就加入遊戲
        }
    }
}
