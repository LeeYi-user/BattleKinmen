using System.Collections;
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
    public int maxDefense = 3;
    public int currentDefense = 3;

    [Header("Enemy Manager")]
    public GameObject enemy;
    public Transform enemyTarget;
    public List<Transform> enemySpawnArea;
    public float enemyDelay = 1f;
    public bool enemyDisable = true;

    [Header("Shop Manager")]
    public float cashBonus = 1f;
    public bool cashDisable = false;
    public bool skillDisable = false;
    public bool teamDisable = false;

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

        Popup("按下 Backspace 退出");
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

    public void Popup(string msg)
    {
        bool exist = false;

        foreach (string str in popups)
        {
            if (str == msg)
            {
                exist = true;
                break;
            }
        }

        if (!exist)
        {
            popups.Add(msg);
        }

        GameObject popGO = Instantiate(popup, popup.transform.position - popups.IndexOf(msg) * new Vector3(0, 25, 0), Quaternion.identity);

        popGO.transform.SetParent(canvas.transform, false);
        popGO.GetComponent<TextMeshProUGUI>().text = msg;
    }
}
