using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnityLobby : MonoBehaviour
{
    public static UnityLobby Instance;

    public Lobby hostLobby;
    public Lobby joinedLobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    [SerializeField] private GameObject lobbyUIPrefab;
    [SerializeField] private GameObject playerUIPrefab;

    private void Awake()
    {
        Instance = this;
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
            heartbeatTimer = 15f;
            return;
        }

        heartbeatTimer -= Time.deltaTime;

        if (heartbeatTimer < 0f)
        {
            heartbeatTimer = 15f;
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby == null)
        {
            lobbyUpdateTimer = 0f;
            return;
        }

        try
        {
            lobbyUpdateTimer -= Time.deltaTime;

            if (lobbyUpdateTimer < 0f)
            {
                lobbyUpdateTimer = 1.1f;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                if (SampleSceneManager.start == 0)
                {
                    if (lobby.Data["state"].Value == "waiting")
                    {
                        ListPlayers();

                        if (hostLobby != null)
                        {
                            UpdateLobbyCount();
                        }
                    }
                    else if (lobby.Data["state"].Value == "starting")
                    {
                        SampleSceneManager.start = 1;
                        StartCoroutine(LoadSceneAsync("MainScene"));
                    }
                }
            }

            if (SampleSceneManager.start == 2)
            {
                if (joinedLobby.Data["state"].Value == "started")
                {
                    SampleSceneManager.start = 3;
                    StartCoroutine(JoinRelayAsync());
                }
            }
        }
        catch (Exception e)
        {
            if (e.ToString().Contains("Rate limit") || !SampleSceneManager.Instance)
            {
                Debug.Log(e);
                return;
            }

            if (!SampleSceneManager.Instance.mainMenu.activeSelf)
            {
                SampleSceneManager.Instance.roomerMenu.SetActive(false);
                SampleSceneManager.Instance.infoMenu.SetActive(false);
                SampleSceneManager.Instance.lobbyMenu.SetActive(true);
            }

            hostLobby = null;
            joinedLobby = null;
            Debug.Log(e);
        }
    }

    IEnumerator JoinRelayAsync()
    {
        yield return new WaitUntil(() => joinedLobby.Data["code"].Value != "");
        UnityRelay.Instance.JoinRelay(joinedLobby.Data["code"].Value);
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        SampleSceneManager.Instance.loadMenu.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            SampleSceneManager.Instance.progressSlider.value = progress;

            yield return null;
        }

        SampleSceneManager.start = 2;
    }

    public async void CreateLobby(string lobbyName, int maxPlayers)
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
                        "mode", new DataObject(DataObject.VisibilityOptions.Public, "搶灘")
                    },
                    {
                        "friendly_fire", new DataObject(DataObject.VisibilityOptions.Public, "開")
                    },
                    {
                        "state", new DataObject(DataObject.VisibilityOptions.Public, "waiting")
                    },
                    {
                        "code", new DataObject(DataObject.VisibilityOptions.Public, "", DataObject.IndexOptions.S1)
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;
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
                    new QueryFilter(QueryFilter.FieldOptions.S1, "", QueryFilter.OpOptions.EQ)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            foreach (Lobby lobby in queryResponse.Results)
            {
                bool flag = false;

                foreach (Transform child in SampleSceneManager.Instance.lobbyMenuContainer.transform)
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
                    GameObject lobbyUI = Instantiate(lobbyUIPrefab, SampleSceneManager.Instance.lobbyMenuContainer.transform.position, Quaternion.identity);
                    lobbyUI.transform.SetParent(SampleSceneManager.Instance.lobbyMenuContainer.transform, false);
                    lobbyUI.GetComponent<LobbyUI>().id = lobby.Id;
                    lobbyUI.GetComponent<LobbyUI>().nameText.text = lobby.Name;
                    lobbyUI.GetComponent<LobbyUI>().countText.text = lobby.Data["count"].Value + "/" + lobby.MaxPlayers.ToString();
                    lobbyUI.GetComponent<LobbyUI>().modeText.text = lobby.Data["mode"].Value;
                }
            }

            foreach (Transform child in SampleSceneManager.Instance.lobbyMenuContainer.transform)
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
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void LobbyInfo()
    {
        SampleSceneManager.Instance.infoMenuLobbyNameText.text = joinedLobby.Name;
        SampleSceneManager.Instance.infoMenuMaxPlayersText.text = joinedLobby.MaxPlayers.ToString();
        SampleSceneManager.Instance.infoMenuGameModeText.text = joinedLobby.Data["mode"].Value;
        SampleSceneManager.Instance.infoMenuFriendlyFireText.text = joinedLobby.Data["friendly_fire"].Value;
    }

    private Unity.Services.Lobbies.Models.Player CreatePlayer(string status)
    {
        return new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, SampleSceneManager.playerName)
                },
                {
                    "status", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, status)
                },
                {
                    "class", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, SampleSceneManager.Instance.classes[SampleSceneManager.playerClass])
                }
            }
        };
    }

    public void ListPlayers()
    {
        GameObject container;

        if (hostLobby != null)
        {
            container = SampleSceneManager.Instance.ownerMenuContainer;
        }
        else
        {
            container = SampleSceneManager.Instance.roomerMenuContainer;
        }

        foreach (Unity.Services.Lobbies.Models.Player player in joinedLobby.Players)
        {
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

            if (!flag)
            {
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
            Debug.Log(e);
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
            Debug.Log(e);
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

            hostLobby = null;
            joinedLobby = null;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

            hostLobby = null;
            joinedLobby = null;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void KickPlayer(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
