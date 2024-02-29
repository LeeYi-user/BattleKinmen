﻿using System.Collections;
using System.Collections.Generic;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UnityRelay : MonoBehaviour
{
    public static UnityRelay Instance;

    private void Awake()
    {
        Instance = this;
    }

    public async void CreateRelay(int maxPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            UnityLobby.Instance.UpdateLobbyCode(joinCode);
            UnityLobby.Instance.UpdateLobbyState("started");
        }
        catch (RelayServiceException e)
        {
            SceneManager.LoadScene("SampleScene");
            Debug.Log(e);
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            SceneManager.LoadScene("SampleScene");
            Debug.Log(e);
        }
    }
}
