using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerHealth : NetworkBehaviour // 這個腳本跟網路有關, 所以要用 NetworkBehavior
{
    // 該檔案是用來控制玩家生命值的, 請把它放在玩家物件之下
    public Image healthBar;
    public float maxHealth = 100f;
    public float currentHealth;
    public float respawnTime = 2f;

    [SerializeField] private SkinnedMeshRenderer skin;
    [SerializeField] private Material blue;
    [SerializeField] private Material red;

    bool live;

    // Start is called before the first frame update
    void Start()
    {
        // 如果當前的玩家物件不是自己, 就直接 return
        if (!IsOwner)
        {
            return;
        }
        // 否則就抓取 HealthBar 物件, 以便正確顯示生命值
        healthBar = GameObject.Find("Health").GetComponent<Image>();
        currentHealth = maxHealth;
        live = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (live && currentHealth <= 0)
        {
            live = false;

            GameObject.Find("Crosshair").GetComponent<Image>().enabled = false;
            GameObject.Find("Death Screen").GetComponent<Image>().enabled = true;
            GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = true;
            gameObject.GetComponent<PlayerMovement>().Despawn();
            gameObject.GetComponent<PlayerCamera>().Despawn();
            gameObject.GetComponent<PlayerModel>().Despawn();
            gameObject.GetComponent<WeaponSway>().Despawn();
            gameObject.GetComponent<Gun>().Despawn();

            PlayerDespawn_ServerRpc(NetworkObjectId, NetworkManager.Singleton.LocalClientId);
            StartCoroutine(Respawn());
        }
    }

    // 這個 function 會在之後寫攻擊腳本時用到, 所以要先弄成 public
    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // 當前生命值 - 受到的傷害

        if (healthBar) // 因為前面有弄一個 if (!IsOwner) return, 所以非你控制的玩家物件都不會抓取 HealthBar 物件
                       // 所以要加這行才能避免出 Bug (這樣寫好像有點怪, 之後有空再修)
        {
            healthBar.fillAmount = currentHealth / maxHealth; // 控制 UI slide
        }

        StartCoroutine(DamageFlash());
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

        GameObject.Find("Crosshair").GetComponent<Image>().enabled = true;
        GameObject.Find("Death Screen").GetComponent<Image>().enabled = false;
        GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = false;
        gameObject.GetComponent<PlayerMovement>().Respawn();
        gameObject.GetComponent<PlayerCamera>().Respawn();
        gameObject.GetComponent<PlayerModel>().Respawn();
        gameObject.GetComponent<WeaponSway>().Respawn();
        gameObject.GetComponent<Gun>().Respawn();

        currentHealth = maxHealth;

        if (healthBar)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }

        PlayerRespawn_ServerRpc(NetworkObjectId, NetworkManager.Singleton.LocalClientId);
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

        if (playerId != NetworkManager.Singleton.LocalClientId)
        {
            playerGO.GetComponent<PlayerModel>().skin.enabled = false;
            playerGO.GetComponent<PlayerModel>().fakeGun.enabled = false;
            playerGO.GetComponent<PlayerModel>().body.layer = LayerMask.NameToLayer("Default");
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

        if (playerId != NetworkManager.Singleton.LocalClientId)
        {
            playerGO.GetComponent<PlayerModel>().skin.enabled = true;
            playerGO.GetComponent<PlayerModel>().fakeGun.enabled = true;
            playerGO.GetComponent<PlayerModel>().body.layer = LayerMask.NameToLayer("Hittable");
        }
    }
}
