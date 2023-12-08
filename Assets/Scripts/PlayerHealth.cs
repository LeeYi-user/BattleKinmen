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
    [SerializeField] private float minAltitude; // -10

    [SerializeField] private CapsuleCollider bodyCollider;
    [SerializeField] private SkinnedMeshRenderer[] bodySkin;
    [SerializeField] private GameObject fakeGunSkin;

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

        if (transform.position.y < minAltitude)
        {
            TakeDamage(100f);
        }

        if (currentHealth.Value <= 0)
        {
            spawning = true;

            PlayerDespawn_ClientRpc();
            StartCoroutine(Respawn(2f));
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
    }

    public IEnumerator Respawn(float seconds = 0)
    {
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
            foreach (SkinnedMeshRenderer skin in bodySkin)
            {
                skin.enabled = false;
            }

            fakeGunSkin.SetActive(false);
        }
    }

    [ClientRpc]
    void PlayerRespawn_ClientRpc()
    {
        bodyCollider.enabled = true;

        if (IsOwner)
        {
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
            foreach (SkinnedMeshRenderer skin in bodySkin)
            {
                skin.enabled = true;
            }

            fakeGunSkin.SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void FinishSpawning_ServerRpc()
    {
        StartCoroutine(FinishSpawning());
    }

    IEnumerator FinishSpawning()
    {
        yield return new WaitUntil(() => transform.position.y >= minAltitude);
        spawning = false;
    }
}
