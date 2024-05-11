using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    public Lobby hostLobby;
    public Lobby joinedLobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    [SerializeField] private GameObject lobbyUIPrefab;
    [SerializeField] private GameObject playerUIPrefab;

    public bool start = false;
    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
    public Dictionary<ulong, List<string>> playersItems = new Dictionary<ulong, List<string>>();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Update()
    {
        HandleHeartbeatTimer();
        HandleLobbyPollForUpdates();
    }

    private async void HandleHeartbeatTimer()
    {
        if (hostLobby == null)
        {
            heartbeatTimer = 0f;
            return;
        }

        if (heartbeatTimer > 0f)
        {
            heartbeatTimer -= Time.deltaTime;
            return;
        }

        heartbeatTimer = 15f;

        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby == null || RelayManager.disconnecting)
        {
            lobbyUpdateTimer = 0f;
            return;
        }

        if (lobbyUpdateTimer > 0f)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            return;
        }

        lobbyUpdateTimer = 1.1f;

        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;

            if (hostLobby != null)
            {
                hostLobby = joinedLobby;
                UpdateLobbyCount();
            }

            if (start)
            {
                return;
            }

            if (lobby.Data["state"].Value == "waiting")
            {
                ListPlayers();
            }
            else if (!RelayManager.disconnecting)
            {
                start = true;
                LoadGameMode(joinedLobby.Data["mode"].Value);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);

            if (e.ToString().Contains("Rate limit") || !MenuManager.Instance || RelayManager.disconnecting)
            {
                if (e.ToString().Contains("lobby not found") && !MenuManager.Instance)
                {
                    GameManager.Instance.Back();
                }

                return;
            }

            MenuManager.Instance.ownerMenu.SetActive(false);
            MenuManager.Instance.roomerMenu.SetActive(false);
            MenuManager.Instance.infoMenu.SetActive(false);
            MenuManager.Instance.modeMenu.SetActive(false);
            MenuManager.Instance.lobbyMenu.SetActive(true);

            hostLobby = null;
            joinedLobby = null;
        }
    }

    public void LoadGameMode(string mode)
    {
        switch (mode)
        {
            case "搶灘":
                StartCoroutine(LoadSceneAsync("BeachScene"));
                break;
            case "巷戰":
                StartCoroutine(LoadSceneAsync("StreetScene1"));
                break;
            case "演習":
                StartCoroutine(LoadSceneAsync("StreetScene2"));
                break;
            default:
                break;
        }
    }

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        MenuManager.Instance.loadMenu.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            MenuManager.Instance.progressSlider.value = progress;

            yield return null;
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, int gameMode, bool friendlyFire)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = CreatePlayer("就緒"),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "count", new DataObject(DataObject.VisibilityOptions.Public, "1")
                    },
                    {
                        "mode", new DataObject(DataObject.VisibilityOptions.Public, MenuManager.modes[gameMode])
                    },
                    {
                        "friendly_fire", new DataObject(DataObject.VisibilityOptions.Public, friendlyFire ? "開" : "關")
                    },
                    {
                        "time_limit", new DataObject(DataObject.VisibilityOptions.Public, MenuManager.timeLimit.ToString())
                    },
                    {
                        "state", new DataObject(DataObject.VisibilityOptions.Public, "waiting", DataObject.IndexOptions.S1)
                    },
                    {
                        "code", new DataObject(DataObject.VisibilityOptions.Public, "")
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;
            RelayManager.disconnecting = false;
            MenuManager.Instance.createMenu.SetActive(false);
            MenuManager.Instance.ownerMenu.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.Name, MenuManager.Instance.lobbyMenuSearchBar.text, QueryFilter.OpOptions.CONTAINS)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            foreach (Lobby lobby in queryResponse.Results)
            {
                bool flag = false;

                foreach (Transform child in MenuManager.Instance.lobbyMenuContainer.transform)
                {
                    if (lobby.Id == child.GetComponent<LobbyUI>().id)
                    {
                        flag = true;
                        child.GetComponent<LobbyUI>().nameText.text = lobby.Name;
                        child.GetComponent<LobbyUI>().countText.text = lobby.Data["count"].Value + "/" + lobby.MaxPlayers.ToString();
                        child.GetComponent<LobbyUI>().modeText.text = lobby.Data["mode"].Value;
                        break;
                    }
                }

                if (!flag)
                {
                    GameObject lobbyUI = Instantiate(lobbyUIPrefab, MenuManager.Instance.lobbyMenuContainer.transform.position, Quaternion.identity);
                    lobbyUI.transform.SetParent(MenuManager.Instance.lobbyMenuContainer.transform, false);
                    lobbyUI.GetComponent<LobbyUI>().id = lobby.Id;
                    lobbyUI.GetComponent<LobbyUI>().nameText.text = lobby.Name;
                    lobbyUI.GetComponent<LobbyUI>().countText.text = lobby.Data["count"].Value + "/" + lobby.MaxPlayers.ToString();
                    lobbyUI.GetComponent<LobbyUI>().modeText.text = lobby.Data["mode"].Value;
                }
            }

            foreach (Transform child in MenuManager.Instance.lobbyMenuContainer.transform)
            {
                bool flag = false;

                foreach (Lobby lobby in queryResponse.Results)
                {
                    if (child.GetComponent<LobbyUI>().id == lobby.Id)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    if (child.GetComponent<LobbyUI>().id == MenuManager.Instance.selectedLobbyId)
                    {
                        MenuManager.Instance.selectedLobbyId = "";
                    }

                    Destroy(child.gameObject);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = CreatePlayer("閒置")
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);

            joinedLobby = lobby;
            RelayManager.disconnecting = false;

            int i = 0;

            foreach (Transform transform in MenuManager.Instance.modeMenu.transform)
            {
                if (i == Array.IndexOf(MenuManager.modes, joinedLobby.Data["mode"].Value))
                {
                    transform.gameObject.SetActive(true);
                }
                else
                {
                    transform.gameObject.SetActive(false);
                }

                i++;
            }

            MenuManager.Instance.lobbyMenu.SetActive(false);
            MenuManager.Instance.roomerMenu.SetActive(true);
            MenuManager.Instance.modeMenu.SetActive(true);
            MenuManager.Instance.selectedLobbyId = "";
            MenuManager.Instance.lobbyMenuSearchBar.text = "";
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void LobbyInfo()
    {
        try
        {
            MenuManager.Instance.infoMenuLobbyNameText.text = joinedLobby.Name;
            MenuManager.Instance.infoMenuMaxPlayersText.text = joinedLobby.MaxPlayers.ToString();
            MenuManager.Instance.infoMenuGameModeText.text = joinedLobby.Data["mode"].Value;
            MenuManager.Instance.infoMenuFriendlyFireText.text = joinedLobby.Data["friendly_fire"].Value;
            MenuManager.Instance.infoMenuTimeLimitText.text = joinedLobby.Data["time_limit"].Value;
            MenuManager.Instance.infoMenuFriendlyFireInfo.SetActive(joinedLobby.Data["mode"].Value != "演習");
            MenuManager.Instance.infoMenuTimeLimitInfo.SetActive(joinedLobby.Data["mode"].Value == "演習");

            MenuManager.Instance.roomerMenu.SetActive(false);
            MenuManager.Instance.infoMenu.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private Unity.Services.Lobbies.Models.Player CreatePlayer(string status)
    {
        return new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, MenuManager.playerName)
                },
                {
                    "status", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, status)
                },
                {
                    "class", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, MenuManager.classes[MenuManager.playerClass])
                }
            }
        };
    }

    public void ListPlayers()
    {
        GameObject container;
        TMP_InputField searchBar;

        if (hostLobby != null)
        {
            container = MenuManager.Instance.ownerMenuContainer;
            searchBar = MenuManager.Instance.ownerMenuSearchBar;
        }
        else
        {
            container = MenuManager.Instance.roomerMenuContainer;
            searchBar = MenuManager.Instance.roomerMenuSearchBar;
        }

        foreach (Unity.Services.Lobbies.Models.Player player in joinedLobby.Players)
        {
            if (!player.Data["name"].Value.Contains(searchBar.text))
            {
                continue;
            }

            bool flag = false;

            foreach (Transform child in container.transform)
            {
                if (player.Id == child.GetComponent<PlayerUI>().id)
                {
                    flag = true;
                    child.GetComponent<PlayerUI>().nameText.text = player.Data["name"].Value;
                    child.GetComponent<PlayerUI>().statusText.text = player.Data["status"].Value;
                    child.GetComponent<PlayerUI>().classText.text = player.Data["class"].Value;
                    break;
                }
            }

            if (!flag)
            {
                GameObject playerUI = Instantiate(playerUIPrefab, container.transform.position, Quaternion.identity);
                playerUI.transform.SetParent(container.transform, false);
                playerUI.GetComponent<PlayerUI>().id = player.Id;
                playerUI.GetComponent<PlayerUI>().nameText.text = player.Data["name"].Value;
                playerUI.GetComponent<PlayerUI>().statusText.text = player.Data["status"].Value;
                playerUI.GetComponent<PlayerUI>().classText.text = player.Data["class"].Value;
            }
        }

        foreach (Transform child in container.transform)
        {
            bool flag = false;

            foreach (Unity.Services.Lobbies.Models.Player player in joinedLobby.Players)
            {
                if (child.GetComponent<PlayerUI>().id == player.Id)
                {
                    flag = true;
                    break;
                }
            }

            if (!flag || !child.GetComponent<PlayerUI>().nameText.text.Contains(searchBar.text))
            {
                if (child.GetComponent<PlayerUI>().id == MenuManager.Instance.selectedPlayerId)
                {
                    MenuManager.Instance.selectedPlayerId = "";
                }

                Destroy(child.gameObject);
            }
        }
    }

    public async void UpdateLobbyCount()
    {
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "count", new DataObject(DataObject.VisibilityOptions.Public, hostLobby.Players.Count.ToString())
                    }
                }
            });
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdateLobbyState(string state)
    {
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "state", new DataObject(DataObject.VisibilityOptions.Public, state)
                    }
                }
            });
        }
        catch (Exception e)
        {
            if (e.ToString().Contains("Rate limit"))
            {
                Debug.Log("UpdateLobbyState() failed, retry in 1 second");
                await Task.Delay(1000);
                UpdateLobbyState(state);
            }
            else
            {
                Debug.Log(e);
            }
        }
    }

    public async void UpdateLobbyCode(string joinCode)
    {
        try
        {
            await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "code", new DataObject(DataObject.VisibilityOptions.Public, joinCode)
                    }
                }
            });
        }
        catch (Exception e)
        {
            if (e.ToString().Contains("Rate limit"))
            {
                Debug.Log("UpdateLobbyCode() failed, retry in 1 second");
                await Task.Delay(1000);
                UpdateLobbyCode(joinCode);
            }
            else
            {
                Debug.Log(e);
            }
        }
    }

    public async void UpdatePlayerStatus()
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {
                        "status", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "就緒")
                    }
                }
            });
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void QuitLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        hostLobby = null;
        joinedLobby = null;
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        hostLobby = null;
        joinedLobby = null;
    }

    public async void KickPlayer(string playerId)
    {
        try
        {
            if (playerId == joinedLobby.Players[0].Id)
            {
                return;
            }

            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
