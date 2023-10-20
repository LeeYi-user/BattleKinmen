using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SampleScene : NetworkBehaviour
{
    // 該檔案是用來做場景控制的腳本
    // 請把它放在同名場景(SampleScene)底下的 For Script 物件
    // Start is called before the first frame update

    bool disconnect;

    void Start()
    {
        disconnect = false;

        if (InitScene.host) // 如果自已是房間主持人 (host)
        {
            NetworkManager.Singleton.StartHost(); // 就主持遊戲
        }
        else // 否則
        {
            NetworkManager.Singleton.StartClient(); // 就加入遊戲
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && Cursor.lockState == CursorLockMode.Locked)
        {
            NetworkManager.Singleton.Shutdown();
        }

        NetworkManager.OnClientStopped += (bool _) =>
        {
            if (!disconnect)
            {
                disconnect = true;
                Cursor.lockState = CursorLockMode.None;
                InitScene.playerTeam = new Dictionary<ulong, int>();
                SceneManager.LoadScene("StartScene");
            }
        };
    }
}
