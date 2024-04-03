using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class TeamModeManager : NetworkBehaviour
{
    public static TeamModeManager Instance;

    [Header("Parent UI")]
    public GameObject storyScreen;

    [Header("Child UI")]
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI downCounter;
    public TextMeshProUGUI gameoverMessage;

    [Header("Variable")]
    public int waveLimit = 0;

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
        if (!IsHost || GameManager.gameOver)
        {
            return;
        }

        if (GameManager.Instance.currentDefense <= 0)
        {
            GameManager.Instance.GameOver_ClientRpc();

            foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU LOSE");
            }
        }
        else if (GameManager.Instance.waves.Value == waveLimit)
        {
            GameManager.Instance.GameOver_ClientRpc();

            foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU WIN");
            }
        }
    }

    private void Counter2()
    {
        if (GameManager.Instance.gameStart < 2 || GameManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        if (GameManager.Instance.breakTime.Value > 0)
        {
            int minutes = (int)GameManager.Instance.breakTime.Value / 60;
            int seconds = (int)GameManager.Instance.breakTime.Value % 60;

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

            downCounter.text = minstr + ":" + secstr;
        }
        else
        {
            downCounter.text = GameManager.Instance.enemies.Value.ToString();

            if (!IsHost)
            {
                return;
            }

            if (GameManager.Instance.enemies.Value > 0)
            {
                GameManager.Instance.enemyDisable = false;
            }
            else
            {
                GameManager.Instance.enemyDisable = true;
                GameManager.Instance.waves.Value++;
                GameManager.Instance.breakTime.Value = 30.99f;
            }
        }
    }

    private void Counter1()
    {
        if (!IsHost || GameManager.Instance.gameStart < 2 || GameManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        GameManager.Instance.breakTime.Value = Mathf.Max(GameManager.Instance.breakTime.Value - Time.deltaTime, 0f);
    }

    private void TextFade2()
    {
        if (pause || !GameManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            if (phase == 0)
            {
                GameManager.Instance.gamingScreen.SetActive(false);
                GameManager.Instance.gameoverScreen.SetActive(true);
                GameManager.Instance.gameoverScreen.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                gameoverMessage.color = new Color(1, 1, 1, 0);
                targetColor = new Color(0, 0, 0, 1);
                timeLeft = 2f;
                StartCoroutine(Pause(2f));
            }
            else if (phase == 1)
            {
                GameManager.Instance.gameoverScreen.GetComponent<Image>().color = targetColor;
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
                GameManager.Instance.Back();
            }

            phase++;
        }
        else
        {
            if (phase == 1)
            {
                GameManager.Instance.gameoverScreen.GetComponent<Image>().color = Color.Lerp(GameManager.Instance.gameoverScreen.GetComponent<Image>().color, targetColor, Time.deltaTime / timeLeft);
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
        if (pause || GameManager.Instance.gameStart != 1 || RelayManager.disconnecting)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            storyText.color = targetColor;
            
            if (phase == 0)
            {
                GameManager.Instance.loadingScreen.SetActive(false);
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
                GameManager.Instance.gameStart = 2;

                if (IsHost)
                {
                    foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
                    {
                        StartCoroutine(player.PlayerObject.GetComponent<Player>().Respawn());
                    }
                }

                storyScreen.SetActive(false);
                GameManager.Instance.gamingScreen.SetActive(true);
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
