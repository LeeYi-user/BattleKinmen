using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainScene : MonoBehaviour
{
    public static ulong LocalObjectId;

    // Start is called before the first frame update
    void Start()
    {
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
