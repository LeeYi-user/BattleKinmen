using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainScene : MonoBehaviour
{
    public static bool start;

    // Start is called before the first frame update
    void Start()
    {
        start = false;

        if (MenuScene.host)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
