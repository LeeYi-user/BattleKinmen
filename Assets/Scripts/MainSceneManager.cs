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

    [SerializeField] private GameObject room;
    [SerializeField] private TextMeshProUGUI playerCounter;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI startButtonText;

    [SerializeField] private GameObject story;
    [SerializeField] private TextMeshProUGUI storyText;

    public static bool start;
    public static bool gameover;
    public static int playerLives;

    private bool pause;
    private bool showed;
    private float alpha;
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
        if (story.activeSelf || start)
        {
            NetworkManager.DisconnectClient(clientId);
            return;
        }

        UpdateCounter_ClientRpc(NetworkManager.ConnectedClients.Count);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (story.activeSelf || start)
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
        playerLives = 5;

        if (GameObject.Find("NetworkManager").GetComponent<UnityRelay>().enabled)
        {
            if (MenuSceneManager.host)
            {
                UnityRelay.CreateRelay();
            }
            else
            {
                UnityRelay.JoinRelay(MenuSceneManager.code);
            }
        }
        else
        {
            if (MenuSceneManager.host)
            {
                NetworkManager.StartHost();
            }
            else
            {
                NetworkManager.StartClient();
            }
        }

        if (MenuSceneManager.host)
        {
            startButtonText.text = "START";
        }
    }

    private void Update()
    {
        TextFade();

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

    private void TextFade()
    {
        if (!story.activeSelf || pause)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            storyText.color = targetColor;

            if (!showed && alpha == 0) // 第一次: 開始顯示
            {
                StartCoroutine(Pause(1f, showed)); // 先暫停一秒
            }

            if (showed) // 第三次: 消失完畢, 開始遊戲
            {
                StartCoroutine(Pause(1f, showed)); // 先暫停一秒
                return;
            }

            alpha++;

            if (alpha > 1f) // 第二次: 顯示完畢, 開始消失
            {
                StartCoroutine(Pause(5f, showed)); // 先暫停五秒
                showed = true;
                alpha = 0f;
            }

            targetColor = new Color(1, 1, 1, alpha);
            timeLeft = 3f;
        }
        else
        {
            storyText.color = Color.Lerp(storyText.color, targetColor, Time.deltaTime / timeLeft);
            timeLeft -= Time.deltaTime;
        }
    }

    private IEnumerator Pause(float seconds, bool showed)
    {
        pause = true;
        yield return new WaitForSeconds(seconds);

        if (showed)
        {
            if (IsHost)
            {
                start = true;

                foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
                {
                    StartCoroutine(player.PlayerObject.GetComponent<Player>().Respawn());
                }
            }

            panel.SetActive(false);
        }
        else
        {
            pause = false;
        }
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
        room.SetActive(false);
        story.SetActive(true);
    }
}
