using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance;

    [Header("Parent UI")]
    public GameObject loadingScreen;
    public GameObject gamingScreen;
    public GameObject deathScreen;
    public GameObject gameoverScreen;

    [Header("Child UI")]
    public TextMeshProUGUI playerCounter;
    public TextMeshProUGUI healthBar;
    public TextMeshProUGUI ammoBar;
    public TextMeshProUGUI deathMessage;

    [Header("Respawn")]
    public List<Transform> spectatorArea;
    public List<Transform> respawnArea;
    public float respawnCooldown;

    [Header("State")]
    public int gameStart;
    public static bool gameOver;

    private void Awake()
    {
        Instance = this;
        gameOver = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Instance = null;
    }

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
        Disconnect();

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
        if (gameStart > 0)
        {
            NetworkManager.DisconnectClient(clientId);
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (gameStart > 0)
        {
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count - 1);
    }

    [ClientRpc]
    private void UpdateCounter_ClientRpc(int count)
    {
        playerCounter.text = count.ToString() + " / " + LobbyManager.Instance.joinedLobby.Players.Count;

        if (IsHost && gameStart == 0 && count == LobbyManager.Instance.joinedLobby.Players.Count)
        {
            StartCoroutine(GameStart(1f));
        }
    }

    private IEnumerator GameStart(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        GameStart_ClientRpc();
    }

    [ClientRpc]
    private void GameStart_ClientRpc()
    {
        gameStart = 1;
    }

    [ClientRpc]
    public void GameOver_ClientRpc()
    {
        gameOver = true;
    }

    private void Start()
    {
        if (LobbyManager.Instance.hostLobby != null)
        {
            RelayManager.Instance.CreateRelay(LobbyManager.Instance.hostLobby.MaxPlayers);
        }

        playerCounter.text = "0 / " + LobbyManager.Instance.joinedLobby.Players.Count;

        GameManager.Instance.Popup("按下 Backspace 退出");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Back();
            return;
        }
    }

    public void Back()
    {
        if (Disconnect())
        {
            return;
        }

        NetworkManager.Shutdown();
    }

    private bool Disconnect()
    {
        if (RelayManager.disconnecting)
        {
            return true;
        }

        RelayManager.disconnecting = true;
        Cursor.lockState = CursorLockMode.None;

        if (LobbyManager.Instance.hostLobby == null)
        {
            LobbyManager.Instance.QuitLobby();
        }
        else
        {
            LobbyManager.Instance.DeleteLobby();
        }

        SceneManager.LoadScene("MenuScene");

        return false;
    }
}
