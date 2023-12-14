using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class MainSceneManager : NetworkBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI playerCounter;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI startButtonText;

    public static bool start;
    public static bool gameover;
    public static int playerLives;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

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

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        if (start)
        {
            NetworkManager.DisconnectClient(clientId);
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (start)
        {
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count - 1);
    }

    [ClientRpc]
    private void UpdateCounter_ClientRpc(int count)
    {
        playerCounter.text = count.ToString();
    }

    private void Start()
    {
        start = false;
        gameover = false;
        playerLives = 4;

        if (MenuSceneManager.host)
        {
            NetworkManager.StartHost();
            startButtonText.text = "START";
        }
        else
        {
            NetworkManager.StartClient();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && (!start || gameover || Cursor.lockState == CursorLockMode.Locked))
        {
            NetworkManager.Shutdown();
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (!IsHost || gameover)
        {
            return;
        }

        if (playerLives <= 0)
        {
            gameover = true;

            foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU LOSE");
            }
        }
    }

    public void StartButtonClick()
    {
        if (!IsHost)
        {
            return;
        }

        start = true;

        foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
        {
            StartCoroutine(player.PlayerObject.GetComponent<Player>().Respawn());
        }

        StartGame_ClientRpc();
    }

    [ClientRpc]
    private void StartGame_ClientRpc()
    {
        panel.SetActive(false);
    }
}
