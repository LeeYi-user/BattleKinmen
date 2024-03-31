using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class TeamModeManager : NetworkBehaviour
{
    public static TeamModeManager Instance;

    [Header("Parent UI")]
    public GameObject storyScreen;

    [Header("Child UI")]
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI enemyCounter;
    public TextMeshProUGUI waveCounter;
    public TextMeshProUGUI cashCounter;
    public TextMeshProUGUI gameoverMessage;

    [Header("Defense")]
    public int maxDefense;
    public int currentDefense;

    [HideInInspector]
    public NetworkVariable<float> breakTime = new NetworkVariable<float>(30.99f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int phase;
    private bool pause;
    private float timeLeft;
    private Color targetColor = new Color(1, 1, 1, 0);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Instance = null;
    }

    private void Start()
    {
        currentDefense = maxDefense;
    }

    private void Update()
    {
        GameOver();
        Counter2();
        Counter1();
        TextFade2();
        TextFade1();
    }

    private void GameOver()
    {
        if (!IsHost || PlayerManager.gameOver)
        {
            return;
        }

        if (currentDefense <= 0)
        {
            PlayerManager.Instance.GameOver_ClientRpc();

            foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU LOSE");
            }
        }
    }

    private void Counter2()
    {
        if (PlayerManager.Instance.gameStart < 2 || PlayerManager.gameOver || RelayManager.disconnecting)
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
            enemyCounter.text = EnemyManager.Instance.enemies.Value.ToString();
        }

        waveCounter.text = "第 " + EnemyManager.Instance.waves.Value.ToString() + " 波";
        cashCounter.text = "$ " + (ShopManager.Instance.teamCash.Value - ShopManager.Instance.cashSpent).ToString();
    }

    private void Counter1()
    {
        if (!IsHost || PlayerManager.Instance.gameStart < 2 || PlayerManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        breakTime.Value = Mathf.Max(breakTime.Value - Time.deltaTime, 0f);
    }

    private void TextFade2()
    {
        if (pause || !PlayerManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            if (phase == 0)
            {
                PlayerManager.Instance.gamingScreen.SetActive(false);
                PlayerManager.Instance.gameoverScreen.SetActive(true);
                PlayerManager.Instance.gameoverScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                gameoverMessage.color = new Color(1, 1, 1, 0);
                targetColor = new Color(0, 0, 0, 1);
                timeLeft = 2f;
                StartCoroutine(Pause(2f));
            }
            else if (phase == 1)
            {
                PlayerManager.Instance.gameoverScreen.GetComponent<Image>().color = targetColor;
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
                PlayerManager.Instance.Back();
            }

            phase++;
        }
        else
        {
            if (phase == 1)
            {
                PlayerManager.Instance.gameoverScreen.GetComponent<Image>().color = Color.Lerp(PlayerManager.Instance.gameoverScreen.GetComponent<Image>().color, targetColor, Time.deltaTime / timeLeft);
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
        if (pause || PlayerManager.Instance.gameStart != 1 || RelayManager.disconnecting)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            storyText.color = targetColor;
            
            if (phase == 0)
            {
                PlayerManager.Instance.loadingScreen.SetActive(false);
                storyScreen.SetActive(true);
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
                phase = -1;
                PlayerManager.Instance.gameStart = 2;

                if (IsHost)
                {
                    foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
                    {
                        StartCoroutine(player.PlayerObject.GetComponent<Player>().Respawn());
                    }
                }

                storyScreen.SetActive(false);
                PlayerManager.Instance.gamingScreen.SetActive(true);
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
}
