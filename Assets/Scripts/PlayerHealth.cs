using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerHealth : NetworkBehaviour
{
    // 元件用途: 操控玩家血量
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private float maxHealth; // 100

    [SerializeField] private Material[] color;
    [SerializeField] private CapsuleCollider bodyCollider;
    [SerializeField] private SkinnedMeshRenderer bodySkin;
    [SerializeField] private SkinnedMeshRenderer fakeGunSkin;

    [SerializeField] private PlayerGun playerGun;
    [SerializeField] private PlayerModel playerModel;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool spawning = true;

    // Update is called once per frame
    void Update()
    {
        if (!IsHost || spawning)
        {
            return;
        }

        if (transform.position.y < -10f)
        {
            TakeDamage(100f);
        }

        if (currentHealth.Value <= 0)
        {
            spawning = true;
            StartCoroutine(Spawn(2f));
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
        StartCoroutine(DamageFlash(0.15f));
    }

    IEnumerator DamageFlash(float seconds)
    {
        ChangeColor_ClientRpc(1);

        yield return new WaitForSeconds(seconds);

        ChangeColor_ClientRpc(0);
    }

    [ClientRpc]
    void ChangeColor_ClientRpc(int index)
    {
        bodySkin.material = color[index];
    }

    public IEnumerator Spawn(float seconds)
    {
        if (MainScene.start)
        {
            PlayerDespawn_ClientRpc();
        }

        yield return new WaitForSeconds(seconds);

        currentHealth.Value = maxHealth;

        PlayerRespawn_ClientRpc();
    }

    [ClientRpc]
    void PlayerDespawn_ClientRpc()
    {
        bodyCollider.enabled = false;

        if (IsOwner)
        {
            //GameObject.Find("Crosshair").GetComponent<Image>().enabled = false;
            //GameObject.Find("Death Screen").GetComponent<Image>().enabled = true;
            //GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = true;

            playerGun.Despawn();
            playerModel.Despawn();
            playerCamera.Despawn();
            playerMovement.Despawn();
        }
        else
        {
            bodySkin.enabled = false;
            fakeGunSkin.enabled = false;
        }
    }

    [ClientRpc]
    void PlayerRespawn_ClientRpc()
    {
        bodyCollider.enabled = true;

        if (IsOwner)
        {
            if (!MainScene.start)
            {
                MainScene.start = true;
                GameObject.Find("Panel").SetActive(false);
            }
            
            //GameObject.Find("Crosshair").GetComponent<Image>().enabled = true;
            //GameObject.Find("Death Screen").GetComponent<Image>().enabled = false;
            //GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = false;

            playerGun.Respawn();
            playerModel.Respawn();
            playerCamera.Respawn();
            playerMovement.Respawn();
            FinishSpawning_ServerRpc();
        }
        else
        {
            bodySkin.enabled = true;
            fakeGunSkin.enabled = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void FinishSpawning_ServerRpc()
    {
        StartCoroutine(FinishSpawning());
    }

    IEnumerator FinishSpawning()
    {
        yield return new WaitUntil(() => transform.position.y >= -10f);
        spawning = false;
    }
}
