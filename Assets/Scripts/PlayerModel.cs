using System.Collections;
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
    public SkinnedMeshRenderer fakeGunSkin;
    public GameObject realGun;

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

        Despawn();
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

    public void Despawn()
    {
        realGun.SetActive(false);

        live = false;
    }

    public void Respawn()
    {
        realGun.SetActive(true);

        body.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        live = true;
    }
}
