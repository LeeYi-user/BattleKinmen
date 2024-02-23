using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UnityLobby : MonoBehaviour
{
    public static UnityLobby Instance;

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

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
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = 15;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;

            if (lobbyUpdateTimer <= 0f)
            {
                lobbyUpdateTimer = 1.1f;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = CreatePlayer("¥N∫¸"),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "count", new DataObject(DataObject.VisibilityOptions.Public, "1")
                    },
                    {
                        "mode", new DataObject(DataObject.VisibilityOptions.Public, "∑m≈y")
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
                Debug.Log(lobby.Name + " " + lobby.Data["count"].Value + "/" + lobby.MaxPlayers.ToString() + " " + lobby.Data["mode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = CreatePlayer("∂¢∏m")
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
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
        foreach (Unity.Services.Lobbies.Models.Player player in joinedLobby.Players)
        {
            Debug.Log(player.Data["name"].Value + " " + player.Data["status"].Value + " " + player.Data["class"].Value);
        }
    }

    public async void QuitLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
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
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
