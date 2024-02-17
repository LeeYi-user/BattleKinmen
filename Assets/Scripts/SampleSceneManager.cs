using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SampleSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject createMenu;
    [SerializeField] private GameObject ownerMenu;
    [SerializeField] private GameObject roomerMenu;
    [SerializeField] private GameObject infoMenu;

    [SerializeField] private TextMeshProUGUI mainMenuStartButtonText;
    [SerializeField] private TextMeshProUGUI mainMenuSettingsButtonText;
    [SerializeField] private TextMeshProUGUI mainMenuQuitButtonText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MainMenuStartButtonEnter()
    {
        mainMenuStartButtonText.text = "<u>開始遊戲</u>";
    }

    public void MainMenuStartButtonExit()
    {
        mainMenuStartButtonText.text = "開始遊戲";
    }

    public void MainMenuSettingsButtonEnter()
    {
        mainMenuSettingsButtonText.text = "<u>設定</u>";
    }

    public void MainMenuSettingsButtonExit()
    {
        mainMenuSettingsButtonText.text = "設定";
    }

    public void MainMenuQuitButtonEnter()
    {
        mainMenuQuitButtonText.text = "<u>離開</u>";
    }

    public void MainMenuQuitButtonExit()
    {
        mainMenuQuitButtonText.text = "離開";
    }

    public void MainMenuStartButtonClick()
    {
        mainMenuStartButtonText.text = "開始遊戲";
        mainMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void LobbyMenuBackButtonClick()
    {
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);
    }

    public void MainMenuSettingsButtonClick()
    {
        mainMenuSettingsButtonText.text = "設定";
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void SettingsMenuBackButtonClick()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void MainMenuQuitButtonClick()
    {
        Application.Quit();
    }
}
