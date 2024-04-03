using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject lobbyMenu;
    public GameObject createMenu;
    public GameObject ownerMenu;
    public GameObject roomerMenu;
    public GameObject infoMenu;
    public GameObject modeMenu;
    public GameObject loadMenu;

    public TextMeshProUGUI mainMenuStartButtonText;
    public TextMeshProUGUI mainMenuSettingsButtonText;
    public TextMeshProUGUI mainMenuQuitButtonText;
    public TMP_InputField lobbyMenuPlayerNameInput;
    public TextMeshProUGUI lobbyMenuClassOptionText;
    public TMP_InputField lobbyMenuSearchBar;
    public TMP_InputField createMenuLobbyNameInput;
    public TextMeshProUGUI createMenuMaxPlayersText;
    public TextMeshProUGUI createMenuGameModeText;
    public GameObject createMenuFriendlyFireSetting;
    public TextMeshProUGUI createMenuFriendlyFireText;
    public TMP_InputField ownerMenuSearchBar;
    public TMP_InputField roomerMenuSearchBar;
    public TextMeshProUGUI infoMenuLobbyNameText;
    public TextMeshProUGUI infoMenuMaxPlayersText;
    public TextMeshProUGUI infoMenuGameModeText;
    public TextMeshProUGUI infoMenuFriendlyFireText;

    public GameObject lobbyMenuContainer;
    public GameObject ownerMenuContainer;
    public GameObject roomerMenuContainer;

    public Slider sensSlider;
    public Slider volumeSlider;
    public Slider progressSlider;

    public static string[] classes = { "榴彈兵", "地雷兵", "醫療兵" };
    public static string playerName = "玩家";
    public static int playerClass = 0;

    public string[] modes = { "搶灘", "巷戰", "演習" };
    public int maxPlayers = 6;
    public static int gameMode = 0;
    public static bool friendlyFire = false;

    public string selectedLobbyId;
    public string selectedPlayerId;

    public static float sens = 0.25f;
    public static float volume = 0.25f;

    private float lobbyQueryTimer;

    private void Awake()
    {
        Instance = this;
        gameMode = 0;
        friendlyFire = false;
        LobbyManager.Instance.start = 0;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Start()
    {
        lobbyMenuPlayerNameInput.text = playerName;
        lobbyMenuClassOptionText.text = classes[playerClass];
        sensSlider.value = sens;
        volumeSlider.value = volume;
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
        lobbyQueryTimer -= Time.deltaTime;

        if (lobbyQueryTimer < 0f)
        {
            lobbyQueryTimer = 1.1f;
            LobbyManager.Instance.ListLobbies();
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
        if (selectedLobbyId == "")
        {
            return;
        }

        ClearPlayerList();
        LobbyManager.Instance.JoinLobby(selectedLobbyId);
    }

    public void LobbyMenuCreateButtonClick()
    {
        maxPlayers = 6;
        gameMode = 0;
        friendlyFire = false;
        createMenuLobbyNameInput.text = "房間";
        createMenuMaxPlayersText.text = maxPlayers.ToString();
        createMenuGameModeText.text = modes[gameMode].ToString();
        createMenuFriendlyFireText.text = friendlyFire ? "開" : "關";

        lobbyMenu.SetActive(false);
        createMenu.SetActive(true);
        createMenuFriendlyFireSetting.SetActive(true);
        modeMenu.SetActive(true);

        int i = 0;

        foreach (Transform transform in modeMenu.transform)
        {
            if (i == gameMode)
            {
                transform.gameObject.SetActive(true);
            }
            else
            {
                transform.gameObject.SetActive(false);
            }

            i++;
        }

        selectedLobbyId = "";
        lobbyMenuSearchBar.text = "";
    }

    public void LobbyMenuBackButtonClick()
    {
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);
        modeMenu.SetActive(false);
        selectedLobbyId = "";
        lobbyMenuSearchBar.text = "";
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

    public void CreateMenuMaxPlayersOptionRightArrowButtonClick()
    {
        if (maxPlayers < 10)
        {
            maxPlayers++;
        }

        createMenuMaxPlayersText.text = maxPlayers.ToString();
    }
    public void CreateMenuMaxPlayersOptionLeftArrowButtonClick()
    {
        if (maxPlayers > 1)
        {
            maxPlayers--;
        }

        createMenuMaxPlayersText.text = maxPlayers.ToString();
    }

    public void CreateMenuGameModeOptionRightArrowButtonClick()
    {
        gameMode++;

        if (gameMode > 2)
        {
            gameMode = 0;
        }

        if (gameMode == 2)
        {
            createMenuFriendlyFireSetting.SetActive(false);
        }
        else
        {
            createMenuFriendlyFireSetting.SetActive(true);
        }

        createMenuGameModeText.text = modes[gameMode].ToString();

        int i = 0;

        foreach (Transform transform in modeMenu.transform)
        {
            if (i == gameMode)
            {
                transform.gameObject.SetActive(true);
            }
            else
            {
                transform.gameObject.SetActive(false);
            }

            i++;
        }
    }

    public void CreateMenuGameModeOptionLeftArrowButtonClick()
    {
        gameMode--;

        if (gameMode < 0)
        {
            gameMode = 2;
        }

        if (gameMode == 2)
        {
            createMenuFriendlyFireSetting.SetActive(false);
        }
        else
        {
            createMenuFriendlyFireSetting.SetActive(true);
        }

        createMenuGameModeText.text = modes[gameMode].ToString();

        int i = 0;

        foreach (Transform transform in modeMenu.transform)
        {
            if (i == gameMode)
            {
                transform.gameObject.SetActive(true);
            }
            else
            {
                transform.gameObject.SetActive(false);
            }

            i++;
        }
    }

    public void CreateMenuFriendlyFireOptionArrowButtonClick()
    {
        if (friendlyFire)
        {
            friendlyFire = false;
            createMenuFriendlyFireText.text = "關";
        }
        else
        {
            friendlyFire = true;
            createMenuFriendlyFireText.text = "開";
        }
    }

    public void CreateMenuCancelButtonClick()
    {
        createMenu.SetActive(false);
        modeMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void CreateMenuConfirmButtonClick()
    {
        if (gameMode == 2)
        {
            friendlyFire = true;
        }

        ClearPlayerList();
        LobbyManager.Instance.CreateLobby(createMenuLobbyNameInput.text, maxPlayers, gameMode, friendlyFire);
        createMenu.SetActive(false);
        ownerMenu.SetActive(true);
    }

    public void OwnerMenuQuitButtonClick()
    {
        LobbyManager.Instance.DeleteLobby();
        ownerMenu.SetActive(false);
        modeMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void OwnerMenuKickButtonClick()
    {
        if (selectedPlayerId == "")
        {
            return;
        }

        LobbyManager.Instance.KickPlayer(selectedPlayerId);
    }

    public void OwnerMenuStartButtonClick()
    {
        LobbyManager.Instance.start = 3;
        LobbyManager.Instance.UpdateLobbyState("started");
        StartCoroutine(LobbyManager.Instance.LoadSceneAsync("BeachScene"));
    }

    public void RoomerMenuQuitButtonClick()
    {
        LobbyManager.Instance.QuitLobby();
        roomerMenu.SetActive(false);
        modeMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void RoomerMenuInfoButtonClick()
    {
        LobbyManager.Instance.LobbyInfo();
        roomerMenu.SetActive(false);
        infoMenu.SetActive(true);
    }

    public void RoomerMenuReadyButtonClick()
    {
        LobbyManager.Instance.UpdatePlayerStatus();
    }

    public void InfoMenuBackButtonClick()
    {
        infoMenu.SetActive(false);
        roomerMenu.SetActive(true);
    }

    public void ClearPlayerList()
    {
        foreach (Transform transform in ownerMenuContainer.transform)
        {
            Destroy(transform.gameObject);
        }

        foreach (Transform transform in roomerMenuContainer.transform)
        {
            Destroy(transform.gameObject);
        }

        selectedPlayerId = "";
        ownerMenuSearchBar.text = "";
        roomerMenuSearchBar.text = "";
    }
}
