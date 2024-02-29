using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class MainSceneManager : NetworkBehaviour
{
    [SerializeField] private GameObject startMenu;

    [SerializeField] private GameObject roomInfo;
    [SerializeField] private TextMeshProUGUI playerCounter;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI startButtonText;

    [SerializeField] private GameObject storyInfo;
    [SerializeField] private TextMeshProUGUI storyText;

    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private TextMeshProUGUI gameoverMessage;

    public static int start;
    public static bool gameover;
    public static int playerLives;

    private int phase;
    private bool pause;
    private float timeLeft;
    private Color targetColor = new Color(1, 1, 1, 0);

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
        playerCounter.text = count.ToString();
    }

    private void Start()
    {
        start = 0;
        gameover = false;
        playerLives = 5;

        if (UnityLobby.Instance.hostLobby != null)
        {
            UnityRelay.Instance.CreateRelay(UnityLobby.Instance.hostLobby.MaxPlayers);
            startButtonText.text = "START";
        }
    }

    private void Update()
    {
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

        if (playerLives <= 0)
        {
            GameOver_ClientRpc();

            foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU LOSE");
            }
        }
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

    public void StartButtonClick()
    {
        if (!IsHost)
        {
            return;
        }

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
}
