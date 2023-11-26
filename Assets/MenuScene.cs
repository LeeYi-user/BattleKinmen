using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuScene : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject modeMenu;
    public GameObject optionsMenu;

    public Button playButton;
    public Button optionsButton;
    public Button quitButton;

    public TextMeshProUGUI playButtonText;
    public TextMeshProUGUI optionsButtonText;
    public TextMeshProUGUI quitButtonText;

    public TMP_InputField inputField;
    public TextMeshProUGUI inputButtonText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inputField.text == "")
        {
            inputButtonText.text = "HOST";
        }
        else
        {
            inputButtonText.text = "JOIN";
        }
    }

    public void PlayButtonHover()
    {
        playButtonText.text = "<u>PLAY</u>";
        optionsButtonText.text = "OPTIONS";
        quitButtonText.text = "QUIT";
    }

    public void OptionsButtonHover()
    {
        playButtonText.text = "PLAY";
        optionsButtonText.text = "<u>OPTIONS</u>";
        quitButtonText.text = "QUIT";
    }

    public void QuitButtonHover()
    {
        playButtonText.text = "PLAY";
        optionsButtonText.text = "OPTIONS";
        quitButtonText.text = "<u>QUIT</u>";
    }

    public void PlayButtonClick()
    {
        mainMenu.SetActive(false);
        modeMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void InputButtonClick()
    {
        if (inputButtonText.text == "HOST")
        {
            InitScene.host = true;
        }
        else
        {
            InitScene.host = false;
        }

        SceneManager.LoadScene("MainScene");
    }

    public void BackButtonClick()
    {
        mainMenu.SetActive(true);
        modeMenu.SetActive(false);
        optionsMenu.SetActive(false);
    }

    public void OptionsButtonClick()
    {
        mainMenu.SetActive(false);
        modeMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void QuitButtonClick()
    {
        Application.Quit();
    }
}
