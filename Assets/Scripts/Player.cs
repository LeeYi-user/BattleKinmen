using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Player : NetworkBehaviour
{
    public Transform head;
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> bulletproof = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> invTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private float minAltitude; // -10
    [SerializeField] private CapsuleCollider bodyCollider;
    [SerializeField] private SkinnedMeshRenderer[] bodySkins;

    public PlayerCamera playerCamera;
    public PlayerWeapon playerWeapon;
    public PlayerMovement playerMovement;

    private bool spawning = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            return;
        }

        currentHealth.OnValueChanged += ShowHealth;
        invTime.OnValueChanged += ChangeHealth;
    }

    private void ShowHealth(float previous, float current)
    {
        MainSceneManager.Instance.healthBar.text = "";

        for (int i = 0; i < currentHealth.Value; i += 10)
        {
            MainSceneManager.Instance.healthBar.text += "|";
        }
    }

    private void ChangeHealth(float previous, float current)
    {
        if (invTime.Value > 0f)
        {
            MainSceneManager.Instance.healthBar.color = Color.yellow;
        }
        else
        {
            MainSceneManager.Instance.healthBar.color = Color.green;
        }
    }

    private void Update()
    {
        if (!IsHost || spawning)
        {
            return;
        }

        if (invTime.Value > 0)
        {
            invTime.Value -= Time.deltaTime;
        }

        if (transform.position.y < minAltitude)
        {
            TakeDamage(100f);
        }

        if (currentHealth.Value <= 0)
        {
            spawning = true;
            PlayerDespawn_ClientRpc("YOU DIED");
            StartCoroutine(Respawn(MainSceneManager.Instance.respawnCooldown));
        }
    }

    public void TakeDamage(float damage)
    {
        if (invTime.Value > 0f)
        {
            return;
        }

        currentHealth.Value -= damage * (1f - bulletproof.Value);
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

            playerCamera.Spawn();
            playerWeapon.Despawn();
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

        if (MainSceneManager.gameover)
        {
            yield break;
        }

        currentHealth.Value = maxHealth.Value;
        invTime.Value = 3f;

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

            playerCamera.Spawn();
            playerWeapon.Respawn();
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
