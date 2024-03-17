using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Player : NetworkBehaviour
{
    [SerializeField] private float maxHealth; // 100
    [SerializeField] private float minAltitude; // -10

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private CapsuleCollider bodyCollider;
    [SerializeField] private SkinnedMeshRenderer[] bodySkins;

    [SerializeField] private PlayerWeapon playerWeapon;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private bool spawning = true;

    public Dictionary<string, int> playerItems = new Dictionary<string, int>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            return;
        }

        currentHealth.OnValueChanged += ShowHealth;
    }

    private void ShowHealth(float previous, float current)
    {
        MainSceneManager.Instance.healthBar.text = "";

        for (int i = 0; i < currentHealth.Value; i += 10)
        {
            MainSceneManager.Instance.healthBar.text += "|";
        }
    }

    private void Start()
    {
        foreach (ShopItem shopItem in Shop.Instance.shopItems)
        {
            playerItems[shopItem.name] = (int)shopItem.levelSlider.value;
        }
    }

    private void Update()
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
            PlayerDespawn_ClientRpc("YOU DIED");
            StartCoroutine(Respawn(2f));
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
    }

    [ClientRpc]
    public void PlayerDespawn_ClientRpc(string msg)
    {
        bodyCollider.enabled = false;

        if (IsOwner)
        {
            MainSceneManager.Instance.crosshair.SetActive(false);
            MainSceneManager.Instance.deathScreen.SetActive(true);
            MainSceneManager.Instance.deathMessage.text = msg;

            playerWeapon.Despawn();
            playerCamera.Despawn();
            playerMovement.Despawn();
        }
        else
        {
            foreach (SkinnedMeshRenderer bodySkin in bodySkins)
            {
                bodySkin.enabled = false;
            }
        }
    }

    public IEnumerator Respawn(float seconds = 0)
    {
        yield return new WaitForSeconds(seconds);

        if (MainSceneManager.Instance.gameover)
        {
            yield break;
        }

        currentHealth.Value = maxHealth;

        PlayerRespawn_ClientRpc();
    }

    [ClientRpc]
    private void PlayerRespawn_ClientRpc()
    {
        bodyCollider.enabled = true;

        if (IsOwner)
        {
            MainSceneManager.Instance.crosshair.SetActive(true);
            MainSceneManager.Instance.deathScreen.SetActive(false);

            playerWeapon.Respawn();
            playerCamera.Respawn();
            playerMovement.Respawn();
            FinishSpawning_ServerRpc();
        }
        else
        {
            foreach (SkinnedMeshRenderer bodySkin in bodySkins)
            {
                bodySkin.enabled = true;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void FinishSpawning_ServerRpc()
    {
        StartCoroutine(FinishSpawning());
    }

    private IEnumerator FinishSpawning()
    {
        yield return new WaitUntil(() => transform.position.y >= minAltitude);
        spawning = false;
    }
}
