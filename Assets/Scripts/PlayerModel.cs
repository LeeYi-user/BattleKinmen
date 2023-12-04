using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerModel : NetworkBehaviour
{
    // 元件用途: 操控玩家模型
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private Transform orientation;
    [SerializeField] private SkinnedMeshRenderer realGunSkin;

    private bool live;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            realGunSkin.enabled = false;
            return;
        }

        gameObject.layer = LayerMask.NameToLayer("Default");
        Despawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        transform.rotation = orientation.rotation;
    }

    public void Despawn()
    {
        live = false;
        realGunSkin.enabled = false;
    }

    public void Respawn()
    {
        live = true;
        realGunSkin.enabled = true;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
