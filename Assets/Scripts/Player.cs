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

        JoinGame_ServerRpc(NetworkManager.LocalClientId);

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

        startButton.onClick.AddListener(StartGame_ClientRpc);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && Cursor.lockState == CursorLockMode.Locked)
        {
            NetworkManager.Shutdown();

            Cursor.lockState = CursorLockMode.None;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void JoinGame_ServerRpc(ulong playerId)
    {
        if (MainScene.start)
        {
            KickPlayer_ClientRpc(playerId);
        }
    }

    [ClientRpc]
    void KickPlayer_ClientRpc(ulong playerId)
    {
        if (playerId == NetworkManager.LocalClientId)
        {
            NetworkManager.Shutdown();
        }
    }

    [ClientRpc]
    void StartGame_ClientRpc()
    {
        MainScene.start = true;

        GameObject.Find("Panel").SetActive(false);
        StartCoroutine(NetworkManager.LocalClient.PlayerObject.gameObject.GetComponent<PlayerHealth>().Respawn(0f));
    }

    [ClientRpc]
    void UpdateCounter_ClientRpc(int count)
    {
        GameObject.Find("Counter").GetComponent<TextMeshProUGUI>().text = count.ToString();
    }
}
