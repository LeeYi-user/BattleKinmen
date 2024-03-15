using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class MainSceneManager : NetworkBehaviour
{
    public static MainSceneManager Instance;

    public GameObject canvas;

    [Header("Screen")]
    public GameObject startMenu;
    public GameObject gamingScreen;
    public GameObject deathScreen;
    public GameObject gameoverScreen;

    [Header("Sub Screen")]
    public GameObject roomInfo;
    public GameObject storyInfo;

    [Header("On Screen")]
    public TextMeshProUGUI playerCounter;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI enemyCounter;
    public TextMeshProUGUI waveCounter;
    public TextMeshProUGUI cashCounter;
    public GameObject crosshair;
    public TextMeshProUGUI healthBar;
    public TextMeshProUGUI ammoBar;
    public TextMeshProUGUI deathMessage;
    public TextMeshProUGUI gameoverMessage;

    [Header("On Screen Prefab")]
    public GameObject popup;

    [Header("On Map")]
    public Transform playerSpawn;

    [Header("On Map Variable")]
    public int mapHealth;

    [HideInInspector] public int start;
    [HideInInspector] public NetworkVariable<float> breakTime = new NetworkVariable<float>(30.99f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [HideInInspector] public bool gameover;
    [HideInInspector] public List<string> popups;

    private int phase;
    private bool pause;
    private float timeLeft;
    private Color targetColor = new Color(1, 1, 1, 0);

    public static bool disconnecting;

    private void Awake()
    {
        Instance = this;
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
        if (start > 0)
        {
            NetworkManager.DisconnectClient(clientId);
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (start > 0)
        {
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count - 1);
    }

    [ClientRpc]
    private void UpdateCounter_ClientRpc(int count)
    {
        playerCounter.text = count.ToString() + " / " + UnityLobby.Instance.joinedLobby.Players.Count;

        if (IsHost && start == 0 && count == UnityLobby.Instance.joinedLobby.Players.Count)
        {
            StartCoroutine(StartGame(1f));
        }
    }

    private void Start()
    {
        playerCounter.text = "0 / " + UnityLobby.Instance.joinedLobby.Players.Count;

        if (UnityLobby.Instance.hostLobby != null)
        {
            UnityRelay.Instance.CreateRelay(UnityLobby.Instance.hostLobby.MaxPlayers);
        }

        Popup("按下 Backspace 退出");
    }

    private void Update()
    {
        Counter2();
        Counter1();
        TextFade2();
        TextFade1();

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Back();
            return;
        }

        if (!IsHost || gameover)
        {
            return;
        }

        //if (Input.GetKeyDown(KeyCode.P) && start == 2 && Cursor.lockState == CursorLockMode.Locked)
        //{
        //    playerLives = 0;
        //}

        if (mapHealth <= 0)
        {
            GameOver_ClientRpc();

            foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU LOSE");
            }
        }
    }

    private void Counter2()
    {
        if (start < 2 || gameover || disconnecting)
        {
            return;
        }

        if (breakTime.Value > 0)
        {
            int minutes = (int)breakTime.Value / 60;
            int seconds = (int)breakTime.Value % 60;

            string minstr = minutes.ToString();

            if (minstr.Length == 1)
            {
                minstr = "0" + minstr;
            }

            string secstr = seconds.ToString();

            if (secstr.Length == 1)
            {
                secstr = "0" + secstr;
            }

            enemyCounter.text = minstr + ":" + secstr;
        }
        else
        {
            enemyCounter.text = EnemySpawn.Instance.enemies.Value.ToString();
        }

        waveCounter.text = "第 " + EnemySpawn.Instance.waves.Value.ToString() + " 波";
        cashCounter.text = "$ " + (Shop.Instance.teamCash.Value - Shop.Instance.cashSpent.Value).ToString();
    }

    private void Counter1()
    {
        if (!IsHost || start < 2 || gameover || disconnecting)
        {
            return;
        }

        breakTime.Value = Mathf.Max(breakTime.Value - Time.deltaTime, 0f);
    }

    private void TextFade2()
    {
        if (!gameover || pause || disconnecting)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            if (phase == 0)
            {
                gamingScreen.SetActive(false);
                gameoverScreen.SetActive(true);
                gameoverScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                gameoverMessage.color = new Color(1, 1, 1, 0);
                targetColor = new Color(0, 0, 0, 1);
                timeLeft = 2f;
                StartCoroutine(Pause(2f));
            }
            else if (phase == 1)
            {
                gameoverScreen.GetComponent<Image>().color = targetColor;
                targetColor = new Color(1, 1, 1, 1);
                timeLeft = 3f;
                StartCoroutine(Pause(1f));
            }
            else if (phase == 2)
            {
                gameoverMessage.color = targetColor;
                targetColor = new Color(1, 1, 1, 0);
                timeLeft = 3f;
                StartCoroutine(Pause(5f));
            }
            else if (phase == 3)
            {
                StartCoroutine(Pause(1f));
            }
            else if (phase == 4)
            {
                Back();
            }

            phase++;
        }
        else
        {
            if (phase == 1)
            {
                gameoverScreen.GetComponent<Image>().color = Color.Lerp(gameoverScreen.GetComponent<Image>().color, targetColor, Time.deltaTime / timeLeft);
            }
            else if (phase > 1)
            {
                gameoverMessage.color = Color.Lerp(gameoverMessage.color, targetColor, Time.deltaTime / timeLeft);
            }
            
            timeLeft -= Time.deltaTime;
        }
    }

    private void TextFade1()
    {
        if (start != 1 || pause || disconnecting)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            storyText.color = targetColor;
            
            if (phase == 0)
            {
                roomInfo.SetActive(false);
                storyInfo.SetActive(true);
            }
            else if (phase == 1)
            {
                targetColor = new Color(1, 1, 1, 1);
                timeLeft = 3f;
                StartCoroutine(Pause(1f));
            }
            else if (phase == 2)
            {
                targetColor = new Color(1, 1, 1, 0);
                timeLeft = 3f;
                StartCoroutine(Pause(5f));
            }
            else if (phase == 3)
            {
                StartCoroutine(Pause(1f));
            }
            else if (phase == 4)
            {
                start = 2;

                if (IsHost)
                {
                    foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
                    {
                        StartCoroutine(player.PlayerObject.GetComponent<Player>().Respawn());
                    }
                }

                startMenu.SetActive(false);
                gamingScreen.SetActive(true);
            }

            phase++;
        }
        else
        {
            storyText.color = Color.Lerp(storyText.color, targetColor, Time.deltaTime / timeLeft);
            timeLeft -= Time.deltaTime;
        }
    }

    private IEnumerator Pause(float seconds)
    {
        pause = true;
        yield return new WaitForSeconds(seconds);
        pause = false;
    }

    private IEnumerator StartGame(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        StartGame_ClientRpc();
    }

    [ClientRpc]
    private void StartGame_ClientRpc()
    {
        start = 1;
    }

    [ClientRpc]
    private void GameOver_ClientRpc()
    {
        gameover = true;
        phase = 0;
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

    private void Back()
    {
        if (Disconnect())
        {
            return;
        }

        NetworkManager.Shutdown();
    }

    private bool Disconnect()
    {
        if (disconnecting)
        {
            return true;
        }

        disconnecting = true;
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
