using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuSceneManager : MonoBehaviour
{
    public static bool host;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject modeMenu;
    [SerializeField] private GameObject optionsMenu;

    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private TextMeshProUGUI optionsButtonText;
    [SerializeField] private TextMeshProUGUI quitButtonText;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI hostButtonText;

    private void Update()
    {
        if (inputField.text == "")
        {
            host = true;
            hostButtonText.text = "HOST";
        }
        else
        {
            host = false;
            hostButtonText.text = "JOIN";
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

    public void HostButtonClick()
    {
        SceneManager.LoadScene("MainScene");

        if (InitSceneManager.relay)
        {
            UnityRelay unityRelay = GameObject.Find("NetworkManager").GetComponent<UnityRelay>();

            if (host)
            {
                unityRelay.CreateRelay();
            }
            else
            {
                unityRelay.JoinRelay(inputField.text);
            }
        }
    }

    public void BackButtonClick()
    {
        mainMenu.SetActive(true);
        modeMenu.SetActive(false);
        optionsMenu.SetActive(false);
    }
}
