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
    [SerializeField] private float respawnTime; // 2

    [SerializeField] private Material[] color;
    [SerializeField] private CapsuleCollider bodyCollider;
    [SerializeField] private SkinnedMeshRenderer bodySkin;
    [SerializeField] private SkinnedMeshRenderer fakeGunSkin;

    [SerializeField] private PlayerGun playerGun;
    [SerializeField] private PlayerModel playerModel;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private Image healthBar;
    private float currentHealth;
    private bool live;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        currentHealth = maxHealth;
        //healthBar = GameObject.Find("Health").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        if (transform.position.y < -10f)
        {
            TakeDamage(100f);
        }

        if (currentHealth <= 0)
        {
            live = false;

            //GameObject.Find("Crosshair").GetComponent<Image>().enabled = false;
            //GameObject.Find("Death Screen").GetComponent<Image>().enabled = true;
            //GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = true;

            playerGun.Despawn();
            playerModel.Despawn();
            playerCamera.Despawn();
            playerMovement.Despawn();

            PlayerDespawn_ServerRpc(NetworkManager.LocalClientId);
            StartCoroutine(Respawn(respawnTime));
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsOwner)
        {
            StartCoroutine(DamageFlash());
            return;
        }

        currentHealth -= damage;
        //healthBar.fillAmount = currentHealth / maxHealth;
    }

    IEnumerator DamageFlash()
    {
        bodySkin.material = color[1];
        yield return new WaitForSeconds(0.15f);
        bodySkin.material = color[0];
    }

    public IEnumerator Respawn(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        live = true;

        //GameObject.Find("Crosshair").GetComponent<Image>().enabled = true;
        //GameObject.Find("Death Screen").GetComponent<Image>().enabled = false;
        //GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = false;

        playerGun.Respawn();
        playerModel.Respawn();
        playerCamera.Respawn();
        playerMovement.Respawn();

        currentHealth = maxHealth;
        //healthBar.fillAmount = currentHealth / maxHealth;

        PlayerRespawn_ServerRpc(NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerDespawn_ServerRpc(ulong playerId)
    {
        PlayerDespawn_ClientRpc(playerId);
    }

    [ClientRpc]
    void PlayerDespawn_ClientRpc(ulong playerId)
    {
        bodyCollider.enabled = false;

        if (playerId != NetworkManager.LocalClientId)
        {
            bodySkin.enabled = false;
            fakeGunSkin.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerRespawn_ServerRpc(ulong playerId)
    {
        PlayerRespawn_ClientRpc(playerId);
    }

    [ClientRpc]
    void PlayerRespawn_ClientRpc(ulong playerId)
    {
        bodyCollider.enabled = true;

        if (playerId != NetworkManager.LocalClientId)
        {
            bodySkin.enabled = true;
            fakeGunSkin.enabled = true;
        }
    }
}
