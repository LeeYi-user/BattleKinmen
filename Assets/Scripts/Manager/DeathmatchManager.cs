using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class DeathmatchManager : NetworkBehaviour
{
    [Header("Parent UI")]
    [SerializeField] private GameObject blackScreen;

    [Header("Child UI")]
    [SerializeField] private TextMeshProUGUI downCounter;

    private int phase;
    private bool pause;
    private float timeLeft;
    private Color targetColor = new Color(0, 0, 0, 1);

    private bool gameStart;

    private void Update()
    {
        KeyInput();
        Counter2();
        Counter1();
        TextFade2();
        TextFade1();
    }

    private void KeyInput()
    {
        if (!IsHost || GameManager.gameOver)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            GameManager.Instance.breakTime.Value = 0f;
        }
    }

    private void GameOver()
    {
        GameManager.Instance.GameOver_ClientRpc();

        foreach (NetworkClient player in NetworkManager.ConnectedClients.Values)
        {
            bool flag = false;
            int score = player.PlayerObject.GetComponent<Player>().playerScore.Value;

            foreach (NetworkClient _player in NetworkManager.ConnectedClients.Values)
            {
                int _score = _player.PlayerObject.GetComponent<Player>().playerScore.Value;

                if (_score > score)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU LOSE");
            }
            else
            {
                player.PlayerObject.GetComponent<Player>().PlayerDespawn_ClientRpc("YOU WIN");
            }
        }
    }

    private void Counter2()
    {
        if (!GameManager.Instance.gameStart || GameManager.gameOver || RelayManager.disconnecting)
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
            if (!IsHost)
            {
                return;
            }

            GameManager.Instance.waves.Value++;

            if (GameManager.Instance.waves.Value == 2)
            {
                MenuManager.friendlyFire = true;
                GameManager.Instance.breakTime.Value = GameManager.Instance.timeLimit;
                GameManager.Instance.Popup_ClientRpc("戰鬥開始!", Color.red);
            }
            else
            {
                GameOver();
            }
        }
    }

    private void Counter1()
    {
        if (!IsHost || !GameManager.Instance.gameStart || GameManager.gameOver || RelayManager.disconnecting)
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
            GameManager.Instance.gameoverScreen.GetComponent<Image>().color = targetColor;

            if (phase == 0)
            {
                targetColor = new Color(0, 0, 0, 1);
                timeLeft = 2f;
                StartCoroutine(Pause(2f));
            }
            else if (phase == 1)
            {
                StartCoroutine(Pause(1f));
            }
            else if (phase == 2)
            {
                GameManager.Instance.Back();
            }

            phase++;
        }
        else
        {
            GameManager.Instance.gameoverScreen.GetComponent<Image>().color = Color.Lerp(GameManager.Instance.gameoverScreen.GetComponent<Image>().color, targetColor, Time.deltaTime / timeLeft);
            timeLeft -= Time.deltaTime;
        }
    }

    private void TextFade1()
    {
        if (pause || gameStart || !RelayManager.connected || RelayManager.disconnecting)
        {
            return;
        }

        if (timeLeft <= Time.deltaTime)
        {
            blackScreen.GetComponent<Image>().color = targetColor;

            if (phase == 0)
            {
                GameManager.Instance.loadingScreen.SetActive(false);
                blackScreen.SetActive(true);
                StartCoroutine(Pause(1f));
            }
            else if (phase == 1)
            {
                targetColor = new Color(0, 0, 0, 0);
                timeLeft = 2f;
                GameManager.Instance.GameStart();
            }
            else if (phase == 2)
            {
                phase = -1;
                gameStart = true;
                blackScreen.SetActive(false);
            }

            phase++;
        }
        else
        {
            blackScreen.GetComponent<Image>().color = Color.Lerp(blackScreen.GetComponent<Image>().color, targetColor, Time.deltaTime / timeLeft);
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
