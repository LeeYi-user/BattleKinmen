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

    [SerializeField] private GameObject startMenu;

    [SerializeField] private GameObject roomInfo;
    [SerializeField] private TextMeshProUGUI playerCounter;

    [SerializeField] private GameObject storyInfo;
    [SerializeField] private TextMeshProUGUI storyText;

    [SerializeField] private GameObject counters;
    [SerializeField] private TextMeshProUGUI enemyCounter;
    [SerializeField] private TextMeshProUGUI waveCounter;

    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private TextMeshProUGUI gameoverMessage;

    [SerializeField] private GameObject popup;

    public int start;
    public NetworkVariable<float> breakTime = new NetworkVariable<float>(30, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public bool gameover;
    public int playerLives = 5;

    private int phase;
    private bool pause;
    private float timeLeft;
    private Color targetColor = new Color(1, 1, 1, 0);

    public List<string> popups;
    private int count;

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
        UnityLobby.Instance.QuitLobby();

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
    }

    private void Update()
    {
        Counter2();
        Counter1();
        TextFade2();
        TextFade1();

        if (Input.GetKeyDown(KeyCode.Backspace) && playerCounter.text != "0" && (start < 2 || gameover || Cursor.lockState == CursorLockMode.Locked))
        {
            NetworkManager.Shutdown();
            Cursor.lockState = CursorLockMode.None;
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

        if (Input.GetKeyDown(KeyCode.K) && start == 2 && Cursor.lockState == CursorLockMode.Locked)
        {
            Popup("測試成功");
        }

        if (Input.GetKeyDown(KeyCode.L) && start == 2 && Cursor.lockState == CursorLockMode.Locked)
        {
            count++;
            Popup("測試" + count.ToString());
        }

        if (playerLives <= 0)
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
        if (start < 2 || gameover)
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

        waveCounter.text = "Wave " + EnemySpawn.Instance.waves.Value.ToString();
    }

    private void Counter1()
    {
        if (!IsHost || start < 2 || gameover)
        {
            return;
        }

        breakTime.Value = Mathf.Max(breakTime.Value - Time.deltaTime, 0f);
    }

    private void TextFade2()
    {
        if (!gameover || pause)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            if (phase == 0)
            {
                counters.SetActive(false);
                gameoverScreen.SetActive(true);
                gameoverScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                targetColor = new Color(0, 0, 0, 1);
                StartCoroutine(Pause(2f));
                timeLeft = 2f;
            }
            else if (phase == 1)
            {
                gameoverScreen.GetComponent<Image>().color = targetColor;
                gameoverMessage.gameObject.SetActive(true);
                gameoverMessage.color = new Color(1, 1, 1, 0);
                targetColor = new Color(1, 1, 1, 1);
                StartCoroutine(Pause(1f));
                timeLeft = 3f;
            }
            else if (phase == 2)
            {
                gameoverMessage.color = targetColor;
                targetColor = new Color(1, 1, 1, 0);
                StartCoroutine(Pause(5f));
                timeLeft = 3f;
            }
            else if (phase == 3)
            {
                StartCoroutine(Pause(1f));
            }
            else if (phase == 4)
            {
                NetworkManager.Shutdown();
                Cursor.lockState = CursorLockMode.None;
                pause = true;
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
        if (start != 1 || pause)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            storyText.color = targetColor;

            if (phase == 1)
            {
                StartCoroutine(Pause(1f));
                targetColor = new Color(1, 1, 1, 1);
                timeLeft = 3f;
            }
            else if (phase == 2)
            {
                StartCoroutine(Pause(5f));
                targetColor = new Color(1, 1, 1, 0);
                timeLeft = 3f;
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
                counters.SetActive(true);
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
        roomInfo.SetActive(false);
        storyInfo.SetActive(true);

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

        popGO.transform.SetParent(GameObject.Find("Canvas").transform, false);
        popGO.GetComponent<TextMeshProUGUI>().text = msg;
    }
}
