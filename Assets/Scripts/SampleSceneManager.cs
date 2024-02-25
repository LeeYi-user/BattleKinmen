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

    [SerializeField] private TMP_InputField lobbyMenuPlayerNameInput;
    [SerializeField] private TMP_InputField createMenuLobbyNameInput;
    [SerializeField] private TMP_InputField createMenuLobbyMaxPlayersInput;

    [SerializeField] private GameObject lobbyContainer;

    public static string playerName;
    public static string[] classes = { "榴彈兵", "地雷兵", "醫療兵" };
    public static int playerClass = 0;
    public static string selectedLobbyId;
    public static string selectedPlayerId;

    private float lobbyQueryTimer;

    public static int clients = 1;

    private void Start()
    {
        lobbyMenuPlayerNameInput.text = "玩家";
    }

    private void Update()
    {
        playerName = lobbyMenuPlayerNameInput.text;
        HandleLobbyListPollForUpdates();
    }

    private void HandleLobbyListPollForUpdates()
    {
        if (!lobbyMenu.activeSelf)
        {
            lobbyQueryTimer = 0f;
            return;
        }

        lobbyQueryTimer -= Time.deltaTime;

        if (lobbyQueryTimer < 0f)
        {
            lobbyQueryTimer = 1.1f * clients;
            UnityLobby.Instance.ListLobbies();
        }
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
        UnityLobby.Instance.JoinLobby(selectedLobbyId);
        lobbyMenu.SetActive(false);
        roomerMenu.SetActive(true);
    }

    public void LobbyMenuCreateButtonClick()
    {
        createMenuLobbyNameInput.text = "房間";
        createMenuLobbyMaxPlayersInput.text = "6";
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
        UnityLobby.Instance.CreateLobby(createMenuLobbyNameInput.text, int.Parse(createMenuLobbyMaxPlayersInput.text));
        createMenu.SetActive(false);
        ownerMenu.SetActive(true);
    }

    public void OwnerMenuQuitButtonClick()
    {
        UnityLobby.Instance.DeleteLobby();
        ownerMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void OwnerMenuKickButtonClick()
    {
        UnityLobby.Instance.KickPlayer(selectedPlayerId);
    }

    public void OwnerMenuStartButtonClick()
    {
        Debug.Log("Start");
    }

    public void RoomerMenuQuitButtonClick()
    {
        UnityLobby.Instance.QuitLobby();
        roomerMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void RoomerMenuInfoButtonClick()
    {
        UnityLobby.Instance.LobbyInfo();
        roomerMenu.SetActive(false);
        infoMenu.SetActive(true);
    }

    public void RoomerMenuReadyButtonClick()
    {
        UnityLobby.Instance.UpdatePlayerStatus();
    }

    public void InfoMenuBackButtonClick()
    {
        infoMenu.SetActive(false);
        roomerMenu.SetActive(true);
    }
}
