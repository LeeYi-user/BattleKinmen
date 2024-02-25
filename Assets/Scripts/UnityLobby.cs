using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using TMPro;

public class UnityLobby : MonoBehaviour
{
    public static UnityLobby Instance;

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    [SerializeField] private GameObject lobbyContainer;
    [SerializeField] private GameObject lobbyUIPrefab;

    [SerializeField] private GameObject playerContainerForOwner;
    [SerializeField] private GameObject playerContainerForRoomer;
    [SerializeField] private GameObject playerUIPrefab;

    [SerializeField] private TextMeshProUGUI InfoMenuNameText;
    [SerializeField] private TextMeshProUGUI InfoMenuMaxPlayersText;
    [SerializeField] private TextMeshProUGUI InfoMenuModeText;
    [SerializeField] private TextMeshProUGUI InfoMenuFriendlyFireText;

    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private GameObject roomerMenu;

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SwitchProfile(Random.Range(int.MinValue, int.MaxValue).ToString());
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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

        try
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 15f;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
        catch
        {
            hostLobby = null;
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
                lobbyUpdateTimer = 1.1f * SampleSceneManager.clients;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                ListPlayers();

                if (hostLobby != null)
                {
                    UpdateLobbyCount();
                }
            }
        }
        catch
        {
            roomerMenu.SetActive(false);
            lobbyMenu.SetActive(true);
            joinedLobby = null;
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = CreatePlayer("´Nºü"),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "count", new DataObject(DataObject.VisibilityOptions.Public, "1")
                    },
                    {
                        "mode", new DataObject(DataObject.VisibilityOptions.Public, "·mÅy")
                    },
                    {
                        "friendly_fire", new DataObject(DataObject.VisibilityOptions.Public, "¶}")
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            foreach (Lobby lobby in queryResponse.Results)
            {
                bool flag = false;

                foreach (Transform child in lobbyContainer.transform)
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
                    GameObject lobbyUI = Instantiate(lobbyUIPrefab, lobbyContainer.transform.position, Quaternion.identity);
                    lobbyUI.transform.SetParent(lobbyContainer.transform, false);
                    lobbyUI.GetComponent<LobbyUI>().id = lobby.Id;
                    lobbyUI.GetComponent<LobbyUI>().nameText.text = lobby.Name;
                    lobbyUI.GetComponent<LobbyUI>().countText.text = lobby.Data["count"].Value + "/" + lobby.MaxPlayers.ToString();
                    lobbyUI.GetComponent<LobbyUI>().modeText.text = lobby.Data["mode"].Value;
                }
            }

            foreach (Transform child in lobbyContainer.transform)
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
        catch (LobbyServiceException e)
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
                Player = CreatePlayer("¶¢¸m")
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);

            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void LobbyInfo()
    {
        InfoMenuNameText.text = joinedLobby.Name;
        InfoMenuMaxPlayersText.text = joinedLobby.MaxPlayers.ToString();
        InfoMenuModeText.text = joinedLobby.Data["mode"].Value;
        InfoMenuFriendlyFireText.text = joinedLobby.Data["friendly_fire"].Value;
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
                    "class", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, SampleSceneManager.classes[SampleSceneManager.playerClass])
                }
            }
        };
    }

    public void ListPlayers()
    {
        GameObject playerContainer;

        if (hostLobby != null)
        {
            playerContainer = playerContainerForOwner;
        }
        else
        {
            playerContainer = playerContainerForRoomer;
        }

        foreach (Unity.Services.Lobbies.Models.Player player in joinedLobby.Players)
        {
            bool flag = false;

            foreach (Transform child in playerContainer.transform)
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
                GameObject playerUI = Instantiate(playerUIPrefab, playerContainer.transform.position, Quaternion.identity);
                playerUI.transform.SetParent(playerContainer.transform, false);
                playerUI.GetComponent<PlayerUI>().id = player.Id;
                playerUI.GetComponent<PlayerUI>().nameText.text = player.Data["name"].Value;
                playerUI.GetComponent<PlayerUI>().statusText.text = player.Data["status"].Value;
                playerUI.GetComponent<PlayerUI>().classText.text = player.Data["class"].Value;
            }
        }

        foreach (Transform child in playerContainer.transform)
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
        catch (LobbyServiceException e)
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
                        "status", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "´Nºü")
                    }
                }
            });
        }
        catch (LobbyServiceException e)
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
        catch (LobbyServiceException e)
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
        catch (LobbyServiceException e)
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
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
