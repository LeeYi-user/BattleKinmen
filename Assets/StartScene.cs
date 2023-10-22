using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScene : MonoBehaviour
{
    // 元件用途: 開始遊戲/選擇陣營
    // 元件位置: StartScene 的 For Script 之下

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField menuInput;
    [SerializeField] private TMP_Text menuButtonText;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject teamPanel;

    private float timeleft;
    private bool skipped;
    private Image background;
    private Color targetColor;

    // Start is called before the first frame update
    void Start()
    {
        timeleft = 3;
        background = storyPanel.GetComponent<Image>();
        targetColor = new Color(0, 0, 0, 100);
    }

    // Update is called once per frame
    void Update()
    {
        if (menuPanel.activeSelf)
        {
            if (menuInput.text == "")
            {
                menuButtonText.text = "HOST";
            }
            else
            {
                menuButtonText.text = "JOIN";
            }

            return;
        }

        if (storyPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                timeleft = 0;
                skipped = true;
            }
            else if (timeleft > Time.deltaTime)
            {
                background.color = Color.Lerp(background.color, targetColor, Time.deltaTime / timeleft);
                timeleft -= Time.deltaTime;
            }
            else
            {
                background.color = targetColor;
                StartCoroutine(ChooseTeam());
            }

            return;
        }
    }

    public void GameStart()
    {
        if (menuButtonText.text == "HOST")
        {
            InitScene.host = true;
        }
        else
        {
            InitScene.host = false;
        }

        menuPanel.SetActive(false);
        storyPanel.SetActive(true);
    }

    IEnumerator ChooseTeam()
    {
        if (skipped)
        {
            yield return new WaitForSeconds(0);
        }
        else
        {
            yield return new WaitForSeconds(2);
        }

        storyPanel.SetActive(false);
        teamPanel.SetActive(true);
    }

    public void Team1()
    {
        InitScene.team = 1;
        SceneManager.LoadScene("SampleScene");
    }

    public void Team2()
    {
        InitScene.team = 2;
        SceneManager.LoadScene("SampleScene");
    }
}
