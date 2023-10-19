using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerModel : NetworkBehaviour  // 這個腳本跟網路有關, 所以要用 NetworkBehavior
{
    // 該檔案是用來調整玩家模型的, 請把它放在玩家物件之下
    [SerializeField] private Transform orientation;

    [SerializeField] private GameObject body;
    [SerializeField] private SkinnedMeshRenderer skin;
    [SerializeField] private GameObject realGun;
    [SerializeField] private SkinnedMeshRenderer fakeGun;

    // Start is called before the first frame update
    void Start()
    {
        // 如果當前的玩家物件不是自己, 就直接 return
        if (!IsOwner)
        {
            realGun.SetActive(false);
            return;
        }

        body.layer = LayerMask.NameToLayer("Default");
        skin.enabled = false;
        fakeGun.enabled = false;
        // 否則就呼叫 ServerRPC, 告知 Server/Host (房間主持人) 自己已經加入遊戲
        JoinTeam_ServerRpc(NetworkObjectId, InitScene.team); // NetworkObjectId 就是玩家"物件"的ID, 跟以後會用到的 ClientID 不同
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        body.transform.rotation = orientation.rotation;
    }

    [ServerRpc(RequireOwnership = false)]
    private void JoinTeam_ServerRpc(ulong objectId, int team)
    {
        // 當 Server/Host 接收到有玩家進來時, 房間主持人 (Host) 就要更新存在自己這邊的玩家陣營列表
        InitScene.playerTeam[objectId] = team;
        // 然後再根據列表, 將所有玩家最新的陣營資訊廣播出去
        foreach (KeyValuePair<ulong, int> player in InitScene.playerTeam)
        {
            // 從 Server/Host 廣播給玩家時, 要用 ClientRPC
            JoinTeam_ClientRpc(player.Key, player.Value);
        }
    }

    [ClientRpc]
    private void JoinTeam_ClientRpc(ulong objectId, int team)
    {
        // 當玩家 (Client) 接收到從 Server/Host 傳來的陣營資訊時, 就要再自己更新當前場景下的物件資訊
        PlayerModel playerModel = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<PlayerModel>();

        if (team == 1) // 如果是一隊
        {

        }
        else // 反之
        {

        }
    }
}
