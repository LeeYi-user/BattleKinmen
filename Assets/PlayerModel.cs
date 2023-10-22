﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerModel : NetworkBehaviour
{
    // 元件用途: 操控玩家模型
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private Transform orientation;

    public GameObject body;
    public SkinnedMeshRenderer bodySkin;
    public GameObject realGun;
    public SkinnedMeshRenderer realGunSkin;
    public SkinnedMeshRenderer fakeGunSkin;

    private bool live;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            realGun.SetActive(false);
            return;
        }

        body.layer = LayerMask.NameToLayer("Default");
        bodySkin.enabled = false;
        fakeGunSkin.enabled = false;
        live = true;

        JoinTeam_ServerRpc(NetworkObjectId, InitScene.team);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        body.transform.rotation = orientation.rotation;
    }

    [ServerRpc(RequireOwnership = false)]
    private void JoinTeam_ServerRpc(ulong objectId, int team)
    {
        InitScene.playerTeam[objectId] = team;

        foreach (KeyValuePair<ulong, int> player in InitScene.playerTeam)
        {
            JoinTeam_ClientRpc(player.Key, player.Value);
        }
    }

    [ClientRpc]
    private void JoinTeam_ClientRpc(ulong objectId, int team)
    {
        try
        {
            PlayerModel playerModel = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<PlayerModel>();

            if (team == 1)
            {

            }
            else
            {

            }
        }
        catch
        {

        }
    }

    public void Despawn()
    {
        live = false;
        realGunSkin.enabled = false;
    }

    public void Respawn()
    {
        body.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        realGunSkin.enabled = true;
        live = true;
    }
}
