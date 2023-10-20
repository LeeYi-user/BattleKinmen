using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScene : MonoBehaviour
{
    // 該檔案是用來做場景控制的腳本
    // 請把它放在同名場景(StartScene)底下的 For Script 物件

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_InputField menuInput;
    [SerializeField] private TMP_Text menuButtonText;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject teamPanel;

    float timeleft;
    bool skipped;
    Image background;
    Color targetColor;

    // Start is called before the first frame update
    void Start()
    {
        timeleft = 3; // 將轉場時間設置為 3 秒
        background = storyPanel.GetComponent<Image>(); // 把當前的背景設定為故事畫面的背景
        targetColor = new Color(0, 0, 0, 100); // 把轉場的最終顏色設定為黑色
    }

    // Update is called once per frame
    void Update()
    {
        if (menuPanel.activeSelf) // 如果現在是菜單畫面
        {
            if (menuInput.text == "") // 如果玩家沒有輸入 code, 那就是要主持遊戲
            {
                menuButtonText.text = "HOST";
            }
            else // 否則
            {
                menuButtonText.text = "JOIN"; // 就是要加入遊戲
            }

            return;
        }

        if (storyPanel.activeSelf) // 如果現在是故事畫面
        {
            if (Input.GetKeyDown(KeyCode.Space)) // 如果玩家按下空白鍵 (就可以直接跳過)
            {
                timeleft = 0; // 把剩餘轉場時間設為 0
                skipped = true; // 把"跳過"設為 true
            }

            if (timeleft > Time.deltaTime) // 轉場中
            {
                background.color = Color.Lerp(background.color, targetColor, Time.deltaTime / timeleft); // 繼續變換顏色
                timeleft -= Time.deltaTime;
            }
            else // 轉場結束
            {
                background.color = targetColor; // 把背景顏色設定為最終顏色 (黑色)
                StartCoroutine(ChooseTeam()); // 準備切換到下一個畫面 (選擇陣營)
            }

            return;
        }
    }

    // 給按鈕用的 function, 負責開始遊戲
    public void GameStart()
    {
        if (menuButtonText.text == "HOST") // 如果玩家主持遊戲的話
        {
            InitScene.host = true; // 就把該變數設為 true (因為有 static 和 DontDestroyOnLoad, 所以在這邊也能引用)
        }
        else
        {
            InitScene.host = false;
        }

        // 把畫面切換到故事畫面
        menuPanel.SetActive(false);
        storyPanel.SetActive(true);
    }

    // 選擇陣營
    IEnumerator ChooseTeam()
    {
        if (skipped) // 如果已經決定跳過
        {
            yield return new WaitForSeconds(0); // 就不等了
        }
        else // 否則
        {
            yield return new WaitForSeconds(2); // 再等個 2 秒
        }
        // 把畫面切換到陣營畫面
        storyPanel.SetActive(false);
        teamPanel.SetActive(true);
    }

    // 給按鈕用的 function, 負責加入一隊
    public void Team1()
    {
        InitScene.team = 1;
        SceneManager.LoadScene("SampleScene");
    }

    // 給按鈕用的 function, 負責加入二隊
    public void Team2()
    {
        InitScene.team = 2;
        SceneManager.LoadScene("SampleScene");
    }
}
