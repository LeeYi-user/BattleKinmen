using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SampleScene : NetworkBehaviour
{
    // 元件用途: 開始連線/結束連線
    // 元件位置: SampleScene 的 For Script 之下

    private bool disconnect;

    // Start is called before the first frame update
    void Start()
    {
        disconnect = false;

        if (InitScene.host)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    // Update is called once per frame
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
