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
        playerCounter.text = count.ToString() + " / " + UnityLobby.Instance.joinedLobby.Players.Count;

        if (IsHost && gameStart == 0 && count == UnityLobby.Instance.joinedLobby.Players.Count)
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
        if (UnityLobby.Instance.hostLobby != null)
        {
            UnityRelay.Instance.CreateRelay(UnityLobby.Instance.hostLobby.MaxPlayers);
        }

        playerCounter.text = "0 / " + UnityLobby.Instance.joinedLobby.Players.Count;

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
        if (UnityRelay.disconnecting)
        {
            return true;
        }

        UnityRelay.disconnecting = true;
        Cursor.lockState = CursorLockMode.None;

        if (UnityLobby.Instance.hostLobby == null)
        {
            UnityLobby.Instance.QuitLobby();
        }
        else
        {
            UnityLobby.Instance.DeleteLobby();
        }

        SceneManager.LoadScene("SampleScene");

        return false;
    }
}
