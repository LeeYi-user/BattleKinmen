using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject pauseScreen;

    [Header("Player UI")]
    public TextMeshProUGUI healthBar;
    public TextMeshProUGUI ammoBar;
    public Image effect;
    public TextMeshProUGUI deathMessage;
    public TextMeshProUGUI pauseScreenResumeButtonText;
    public TextMeshProUGUI pauseScreenQuitButtonText;

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

    [Header("Game State")]
    public bool gameStart = false;
    public static bool gameOver;
    public float timeLeft = 0f;

    private void Awake()
    {
        Instance = this;
        gameOver = false;
    }

    private void OnEnable()
    {
        NetworkManager.OnClientDisconnectCallback += ClientSide_OnClientDisconnectCallback;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsHost)
        {
            return;
        }

        NetworkManager.OnClientConnectedCallback += ServerSide_OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback += ServerSide_OnClientDisconnectCallback;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Instance = null;
        NetworkManager.OnClientDisconnectCallback -= ClientSide_OnClientDisconnectCallback;

        if (!IsHost)
        {
            return;
        }

        NetworkManager.OnClientConnectedCallback -= ServerSide_OnClientConnectedCallback;
        NetworkManager.OnClientDisconnectCallback -= ServerSide_OnClientDisconnectCallback;
    }

    private void ClientSide_OnClientDisconnectCallback(ulong clientId)
    {
        if (IsHost)
        {
            return;
        }

        Disconnect();
    }

    private void ServerSide_OnClientConnectedCallback(ulong clientId)
    {
        if (gameOver || RelayManager.disconnecting)
        {
            NetworkManager.DisconnectClient(clientId);
        }
    }

    private void ServerSide_OnClientDisconnectCallback(ulong clientId)
    {
        if (gameOver || RelayManager.disconnecting)
        {
            return;
        }

        RemovePlayer_ClientRpc(NetworkManager.ConnectedClients[clientId].PlayerObject.NetworkObjectId);
    }

    [ClientRpc]
    private void RemovePlayer_ClientRpc(ulong objectId)
    {
        LobbyManager.Instance.players.Remove(objectId);
    }

    public void GameStart()
    {
        gameStart = true;
        Cursor.lockState = CursorLockMode.Locked;

        foreach (Player player in LobbyManager.Instance.players.Values)
        {
            if (player.NetworkObjectId == NetworkManager.LocalClient.PlayerObject.NetworkObjectId)
            {
                continue;
            }

            player.bodyCollider.enabled = player.currentHealth.Value > 0;

            foreach (SkinnedMeshRenderer bodySkin in player.bodySkins)
            {
                bodySkin.enabled = player.currentHealth.Value > 0;
            }

            player.playerWeapon.SelectWeapon_ServerRpc(player.playerWeapon.selectedWeapon.Value);
        }

        NetworkManager.LocalClient.PlayerObject.GetComponent<Player>().PlayerRespawn_ServerRpc();
    }

    [ClientRpc]
    public void GameOver_ClientRpc()
    {
        gameOver = true;
        gamingScreen.SetActive(false);
        gameoverScreen.SetActive(true);
        gameoverScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);
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
    }

    private IEnumerator JoinRelay()
    {
        yield return new WaitUntil(() => LobbyManager.Instance.joinedLobby.Data["code"].Value != "");
        RelayManager.Instance.JoinRelay(LobbyManager.Instance.joinedLobby.Data["code"].Value);
    }

    private void Update()
    {
        ChangeEffect();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            gamingScreen.SetActive(false);
            pauseScreen.SetActive(true);
            popups.Clear();
            popups.Add(null);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            NetworkManager.LocalClient.PlayerObject.GetComponent<Player>().PlayerDespawn_ServerRpc();
            return;
        }
    }

    private void ChangeEffect()
    {
        if (timeLeft <= Time.deltaTime)
        {
            effect.color = new Color(effect.color.r, effect.color.g, effect.color.b, 0f);
        }
        else
        {
            effect.color = Color.Lerp(effect.color, new Color(effect.color.r, effect.color.g, effect.color.b, 0f), Time.deltaTime / timeLeft);
            timeLeft -= Time.deltaTime;
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

        RelayManager.connected = false;
        RelayManager.disconnecting = true;

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
        if (popups.Contains(null))
        {
            return;
        }

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

    public void PauseScreenResumeButtonClick()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gamingScreen.SetActive(gameStart && !deathScreen.activeSelf);
        pauseScreen.SetActive(false);
        PauseScreenResumeButtonExit();
        popups.Remove(null);
    }

    public void PauseScreenResumeButtonEnter()
    {
        pauseScreenResumeButtonText.text = "<u>繼續</u>";
    }

    public void PauseScreenResumeButtonExit()
    {
        pauseScreenResumeButtonText.text = "繼續";
    }

    public void PauseScreenQuitButtonClick()
    {
        Back();
    }

    public void PauseScreenQuitButtonEnter()
    {
        pauseScreenQuitButtonText.text = "<u>離開</u>";
    }

    public void PauseScreenQuitButtonExit()
    {
        pauseScreenQuitButtonText.text = "離開";
    }
}
