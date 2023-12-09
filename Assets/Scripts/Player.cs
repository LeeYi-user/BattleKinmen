using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class Player : NetworkBehaviour
{
    private Button startButton;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            return;
        }

        JoinGame_ServerRpc();
        NetworkManager.OnClientStopped += NetworkManager_OnClientStopped;

        if (!IsHost)
        {
            return;
        }

        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientStopped(bool obj)
    {
        SceneManager.LoadScene("MenuScene");
        NetworkManager.OnClientStopped -= NetworkManager_OnClientStopped;

        if (!IsHost)
        {
            return;
        }

        NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (MainScene.start)
        {
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        if (MainScene.start)
        {
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count - 1);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner || !IsHost)
        {
            return;
        }

        startButton = GameObject.Find("Button").GetComponent<Button>();
        startButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "START";
        startButton.onClick.AddListener(StartButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && (!MainScene.start || Cursor.lockState == CursorLockMode.Locked))
        {
            NetworkManager.Shutdown();
            Cursor.lockState = CursorLockMode.None;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void JoinGame_ServerRpc()
    {
        if (MainScene.start)
        {
            KickPlayer_ClientRpc();
        }
    }

    [ClientRpc]
    void KickPlayer_ClientRpc()
    {
        if (IsOwner)
        {
            NetworkManager.Shutdown();
        }
    }

    [ClientRpc]
    void UpdateCounter_ClientRpc(int count)
    {
        GameObject.Find("Counter").GetComponent<TextMeshProUGUI>().text = count.ToString();
    }

    void StartButtonClick()
    {
        MainScene.start = true;

        startButton.onClick.RemoveListener(StartButtonClick);

        foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
        {
            StartCoroutine(player.PlayerObject.GetComponent<PlayerHealth>().Respawn());
        }

        StartGame_ClientRpc();
    }

    [ClientRpc]
    void StartGame_ClientRpc()
    {
        GameObject.Find("Panel").SetActive(false);
    }
}
