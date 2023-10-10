using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerModel : NetworkBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material blueTeamColor;
    [SerializeField] private Material redTeamColor;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        JoinTeam_ServerRpc(NetworkObjectId, InitScene.team);
    }

    [ServerRpc(RequireOwnership = false)]
    private void JoinTeam_ServerRpc(ulong objectId, string team)
    {
        InitScene.playerTeam[objectId] = team;

        foreach (KeyValuePair<ulong, string> player in InitScene.playerTeam)
        {
            JoinTeam_ClientRpc(player.Key, player.Value);
        }
    }

    [ClientRpc]
    private void JoinTeam_ClientRpc(ulong objectId, string team)
    {
        PlayerModel playerModel = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<PlayerModel>();

        if (team == "Blue")
        {
            playerModel.meshRenderer.material = blueTeamColor;
        }
        else
        {
            playerModel.meshRenderer.material = redTeamColor;
        }
    }
}
