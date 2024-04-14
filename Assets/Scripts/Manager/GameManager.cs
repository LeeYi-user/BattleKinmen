﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<float> breakTime = new NetworkVariable<float>(30.99f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> waves = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> enemies = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> teamCash = new NetworkVariable<int>(1000, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static GameManager Instance;

    [Header("Parent UI")]
    public GameObject canvas;
    public GameObject popup;
    public List<string> popups = null;

    [Header("Child UI")]
    public GameObject loadingScreen;
    public GameObject gamingScreen;
    public GameObject deathScreen;
    public GameObject gameoverScreen;

    [Header("Player UI")]
    public TextMeshProUGUI playerCounter;
    public TextMeshProUGUI healthBar;
    public TextMeshProUGUI ammoBar;
    public TextMeshProUGUI deathMessage;

    [Header("Player Spawn")]
    public List<Transform> playerDespawnArea;
    public List<Transform> playerRespawnArea;
    public float respawnCooldown = 10f;

    [Header("Team Mode Manager")]
    public int waveLimit = -1;
    public float timeLimit = 30.99f;
    public int maxDefense = 3;
    public int currentDefense = 3;

    [Header("Enemy Manager")]
    public GameObject enemy;
    public Transform enemyTarget;
    public List<Transform> enemySpawnArea;
    public float enemyDelay = 1f;
    public CustomVariable<bool> enemyDisable = new CustomVariable<bool>(true);

    [Header("Shop Manager")]
    public float cashBonus = 1f;
    public bool cashDisable = false;
    public bool skillDisable = false;
    public bool teamDisable = false;

    [Header("Scoreboard Manager")]
    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

    [Header("Game State")]
    public int gameStart = 0;
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
        if (RelayManager.disconnecting)
        {
            return;
        }

        RemovePlayer_ClientRpc(NetworkManager.ConnectedClients[clientId].PlayerObject.NetworkObjectId);

        if (gameStart > 0)
        {
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count - 1);
    }

    [ClientRpc]
    private void RemovePlayer_ClientRpc(ulong objectId)
    {
        players.Remove(objectId);
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
        else
        {
            StartCoroutine(JoinRelay());
        }

        playerCounter.text = "0 / " + LobbyManager.Instance.joinedLobby.Players.Count;

        Popup("按下 Backspace 退出", Color.yellow);
    }

    private IEnumerator JoinRelay()
    {
        yield return new WaitUntil(() => LobbyManager.Instance.joinedLobby.Data["code"].Value != "");
        RelayManager.Instance.JoinRelay(LobbyManager.Instance.joinedLobby.Data["code"].Value);
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

        if (LobbyManager.Instance.hostLobby != null)
        {
            LobbyManager.Instance.DeleteLobby();
        }
        else
        {
            LobbyManager.Instance.QuitLobby();
        }

        SceneManager.LoadScene("MenuScene");

        return false;
    }

    public void Popup(string text, Color color)
    {
        bool exist = false;

        foreach (string msg in popups)
        {
            if (msg == text)
            {
                exist = true;
                break;
            }
        }

        if (!exist)
        {
            popups.Add(text);
        }

        GameObject popGO = Instantiate(popup, popup.transform.position - popups.IndexOf(text) * new Vector3(0, 25, 0), Quaternion.identity);
        popGO.transform.SetParent(canvas.transform, false);
        popGO.GetComponent<TextMeshProUGUI>().color = color;
        popGO.GetComponent<TextMeshProUGUI>().text = text;
    }

    [ClientRpc]
    public void Popup_ClientRpc(string text, Color color, bool dm = false, ulong id = 0)
    {
        if (dm && NetworkManager.LocalClient.PlayerObject.NetworkObjectId != id)
        {
            return;
        }

        Popup(text, color);
    }
}
