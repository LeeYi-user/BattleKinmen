using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScene : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField menuInput;
    [SerializeField] private TMP_Text menuButtonText;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject teamPanel;

    float timeleft;
    Image background;
    Color targetColor;

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
            if (timeleft > Time.deltaTime)
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

        menuPanel.SetActive(false);
        storyPanel.SetActive(true);
    }

    IEnumerator ChooseTeam()
    {
        yield return new WaitForSeconds(2);
        storyPanel.SetActive(false);
        teamPanel.SetActive(true);
    }

    public void BlueTeam()
    {
        InitScene.team = "Blue";
        SceneManager.LoadScene("SampleScene");
    }

    public void RedTeam()
    {
        InitScene.team = "Red";
        SceneManager.LoadScene("SampleScene");
    }
}
