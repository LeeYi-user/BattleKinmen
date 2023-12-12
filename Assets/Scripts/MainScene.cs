using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainScene : MonoBehaviour
{
    public static bool start;
    public static bool gameover;
    public static int lives;

    // Start is called before the first frame update
    void Start()
    {
        start = false;
        gameover = false;
        lives = 4;

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
