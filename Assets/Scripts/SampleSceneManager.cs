using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SampleSceneManager : MonoBehaviour
{
    public static SampleSceneManager Instance;

    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject lobbyMenu;
    public GameObject createMenu;
    public GameObject ownerMenu;
    public GameObject roomerMenu;
    public GameObject infoMenu;

    public TextMeshProUGUI mainMenuStartButtonText;
    public TextMeshProUGUI mainMenuSettingsButtonText;
    public TextMeshProUGUI mainMenuQuitButtonText;
    public TMP_InputField lobbyMenuPlayerNameInput;
    public TextMeshProUGUI lobbyMenuClassOptionText;
    public TMP_InputField createMenuLobbyNameInput;
    public TMP_InputField createMenuMaxPlayersInput;
    public TextMeshProUGUI infoMenuLobbyNameText;
    public TextMeshProUGUI infoMenuMaxPlayersText;
    public TextMeshProUGUI infoMenuGameModeText;
    public TextMeshProUGUI infoMenuFriendlyFireText;

    public GameObject lobbyMenuContainer;
    public GameObject ownerMenuContainer;
    public GameObject roomerMenuContainer;

    public Slider sensSlider;
    public Slider volumeSlider;

    public string[] classes = { "榴彈兵", "地雷兵", "醫療兵" };
    public static string playerName = "玩家";
    public static int playerClass = 0;

    public string selectedLobbyId;
    public string selectedPlayerId;

    public float sens;
    public float volume;

    private float lobbyQueryTimer;

    public static int start;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        lobbyMenuPlayerNameInput.text = playerName;
        sensSlider.value = sens;
        volumeSlider.value = volume;
        start = 0;
    }

    private void Update()
    {
        playerName = lobbyMenuPlayerNameInput.text;
        sens = sensSlider.value;
        volume = volumeSlider.value;
        HandleLobbyListPollForUpdates();
    }

    private void HandleLobbyListPollForUpdates()
    {
        if (!lobbyMenu.activeSelf)
        {
            lobbyQueryTimer = 1.1f;
            return;
        }

        lobbyQueryTimer -= Time.deltaTime;

        if (lobbyQueryTimer < 0f)
        {
            lobbyQueryTimer = 1.1f;
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
        createMenuMaxPlayersInput.text = "6";
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
        UnityLobby.Instance.CreateLobby(createMenuLobbyNameInput.text, int.Parse(createMenuMaxPlayersInput.text));
        createMenu.SetActive(false);
        ownerMenu.SetActive(true);
    }

    public void OwnerMenuQuitButtonClick()
    {
        UnityLobby.Instance.QuitLobby();
        ownerMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void OwnerMenuKickButtonClick()
    {
        UnityLobby.Instance.KickPlayer(selectedPlayerId);
    }

    public void OwnerMenuStartButtonClick()
    {
        start = 3;
        SceneManager.LoadScene("MainScene");
        UnityLobby.Instance.UpdateLobbyState("starting");
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
