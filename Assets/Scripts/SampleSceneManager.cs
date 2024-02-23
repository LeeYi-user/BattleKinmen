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
    [SerializeField] private TextMeshProUGUI lobbyMenuClassOptionText;

    private string[] classes = { "榴彈兵", "地雷兵", "醫療兵" };
    private int playerClass = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MainMenuStartButtonClick()
    {
        mainMenuStartButtonText.text = "開始遊戲";
        mainMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void MainMenuStartButtonEnter()
    {
        mainMenuStartButtonText.text = "<u>開始遊戲</u>";
    }

    public void MainMenuStartButtonExit()
    {
        mainMenuStartButtonText.text = "開始遊戲";
    }

    public void MainMenuSettingsButtonClick()
    {
        mainMenuSettingsButtonText.text = "設定";
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void MainMenuSettingsButtonEnter()
    {
        mainMenuSettingsButtonText.text = "<u>設定</u>";
    }

    public void MainMenuSettingsButtonExit()
    {
        mainMenuSettingsButtonText.text = "設定";
    }

    public void MainMenuQuitButtonClick()
    {
        Application.Quit();
    }

    public void MainMenuQuitButtonEnter()
    {
        mainMenuQuitButtonText.text = "<u>離開</u>";
    }

    public void MainMenuQuitButtonExit()
    {
        mainMenuQuitButtonText.text = "離開";
    }

    public void SettingsMenuBackButtonClick()
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void LobbyMenuJoinButtonClick()
    {
        lobbyMenu.SetActive(false);
        roomerMenu.SetActive(true);
    }

    public void LobbyMenuCreateButtonClick()
    {
        lobbyMenu.SetActive(false);
        createMenu.SetActive(true);
    }

    public void LobbyMenuBackButtonClick()
    {
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);
    }

    public void LobbyMenuRightArrowButtonClick()
    {
        playerClass++;

        if (playerClass > 2)
        {
            playerClass = 0;
        }

        lobbyMenuClassOptionText.text = classes[playerClass];
    }

    public void LobbyMenuLeftArrowButtonClick()
    {
        playerClass--;

        if (playerClass < 0)
        {
            playerClass = 2;
        }

        lobbyMenuClassOptionText.text = classes[playerClass];
    }

    public void CreateMenuCancelButtonClick()
    {
        createMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void CreateMenuConfirmButtonClick()
    {
        createMenu.SetActive(false);
        ownerMenu.SetActive(true);
    }

    public void OwnerMenuQuitButtonClick()
    {
        ownerMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void OwnerMenuKickButtonClick()
    {
        Debug.Log("Kick");
    }

    public void OwnerMenuStartButtonClick()
    {
        Debug.Log("Start");
    }

    public void RoomerMenuQuitButtonClick()
    {
        roomerMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void RoomerMenuInfoButtonClick()
    {
        roomerMenu.SetActive(false);
        infoMenu.SetActive(true);
    }

    public void RoomerMenuReadyButtonClick()
    {
        Debug.Log("Ready");
    }

    public void InfoMenuBackButtonClick()
    {
        infoMenu.SetActive(false);
        roomerMenu.SetActive(true);
    }
}
