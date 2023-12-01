using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    public bool start;
    private bool stop;

    // Start is called before the first frame update
    void Start()
    {
        start = false;
        stop = false;

        if (MenuScene.host)
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
        if (Input.GetKeyDown(KeyCode.Backspace) && Cursor.lockState == CursorLockMode.Locked)
        {
            NetworkManager.Singleton.Shutdown();
        }

        NetworkManager.Singleton.OnClientStopped += (bool _) =>
        {
            if (!stop)
            {
                stop = true;
                Cursor.lockState = CursorLockMode.None;

                SceneManager.LoadScene("MenuScene");
            }
        };
    }
}
