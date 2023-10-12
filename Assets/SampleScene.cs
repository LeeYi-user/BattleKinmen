using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SampleScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (InitScene.host)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
