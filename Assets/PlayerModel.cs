using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerModel : NetworkBehaviour  // 這個腳本跟網路有關, 所以要用 NetworkBehavior
{
    // 該檔案是用來調整玩家模型的, 請把它放在玩家物件之下
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material blueTeamColor;
    [SerializeField] private Material redTeamColor;

    // Start is called before the first frame update
    void Start()
    {
        // 如果當前的玩家物件不是自己, 就直接 return
        if (!IsOwner)
        {
            return;
        }
        // 否則就呼叫 ServerRPC, 告知 Server/Host (房間主持人) 自己已經加入遊戲
        JoinTeam_ServerRpc(NetworkObjectId, InitScene.team); // NetworkObjectId 就是玩家"物件"的ID, 跟以後會用到的 ClientID 不同
    }

    [ServerRpc(RequireOwnership = false)]
    private void JoinTeam_ServerRpc(ulong objectId, string team)
    {
        // 當 Server/Host 接收到有玩家進來時, 房間主持人 (Host) 就要更新存在自己這邊的玩家陣營列表
        InitScene.playerTeam[objectId] = team;
        // 然後再根據列表, 將所有玩家最新的陣營資訊廣播出去
        foreach (KeyValuePair<ulong, string> player in InitScene.playerTeam)
        {
            // 從 Server/Host 廣播給玩家時, 要用 ClientRPC
            JoinTeam_ClientRpc(player.Key, player.Value);
        }
    }

    [ClientRpc]
    private void JoinTeam_ClientRpc(ulong objectId, string team)
    {
        // 當玩家 (Client) 接收到從 Server/Host 傳來的陣營資訊時, 就要再自己更新當前場景下的物件資訊
        PlayerModel playerModel = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<PlayerModel>();

        if (team == "Blue") // 如果是藍隊
        {
            playerModel.meshRenderer.material = blueTeamColor; // 就將物件弄成藍色
        }
        else // 反之
        {
            playerModel.meshRenderer.material = redTeamColor; // 就將物件弄成紅色
        }
    }
}
