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

    [SerializeField] private SkinnedMeshRenderer skin;
    [SerializeField] private Material red;
    [SerializeField] private Material blue;

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

        //healthBar = GameObject.Find("Health").GetComponent<Image>();
        currentHealth = maxHealth;
        live = true;
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

            gameObject.GetComponent<PlayerMovement>().Despawn();
            gameObject.GetComponent<PlayerCamera>().Despawn();
            gameObject.GetComponent<PlayerModel>().Despawn();
            gameObject.GetComponent<PlayerGun>().Despawn();

            PlayerDespawn_ServerRpc(NetworkObjectId, NetworkManager.LocalClientId);
            StartCoroutine(Respawn());
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
        skin.material = red;
        yield return new WaitForSeconds(0.15f);
        skin.material = blue;
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);

        live = true;

        //GameObject.Find("Crosshair").GetComponent<Image>().enabled = true;
        //GameObject.Find("Death Screen").GetComponent<Image>().enabled = false;
        //GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = false;

        gameObject.GetComponent<PlayerMovement>().Respawn();
        gameObject.GetComponent<PlayerCamera>().Respawn();
        gameObject.GetComponent<PlayerModel>().Respawn();
        gameObject.GetComponent<PlayerGun>().Respawn();

        currentHealth = maxHealth;
        //healthBar.fillAmount = currentHealth / maxHealth;

        PlayerRespawn_ServerRpc(NetworkObjectId, NetworkManager.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerDespawn_ServerRpc(ulong objectId, ulong playerId)
    {
        PlayerDespawn_ClientRpc(objectId, playerId);
    }

    [ClientRpc]
    void PlayerDespawn_ClientRpc(ulong objectId, ulong playerId)
    {
        GameObject playerGO = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject;

        if (playerId != NetworkManager.LocalClientId)
        {
            playerGO.GetComponent<PlayerModel>().body.layer = LayerMask.NameToLayer("Default");
            playerGO.GetComponent<PlayerModel>().bodySkin.enabled = false;
            playerGO.GetComponent<PlayerModel>().fakeGunSkin.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlayerRespawn_ServerRpc(ulong objectId, ulong playerId)
    {
        PlayerRespawn_ClientRpc(objectId, playerId);
    }

    [ClientRpc]
    void PlayerRespawn_ClientRpc(ulong objectId, ulong playerId)
    {
        GameObject playerGO = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject;

        if (playerId != NetworkManager.LocalClientId)
        {
            playerGO.GetComponent<PlayerModel>().body.layer = LayerMask.NameToLayer("Hittable");
            playerGO.GetComponent<PlayerModel>().bodySkin.enabled = true;
            playerGO.GetComponent<PlayerModel>().fakeGunSkin.enabled = true;
        }
    }
}
