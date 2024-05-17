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

    public CapsuleCollider bodyCollider;
    public SkinnedMeshRenderer[] bodySkins;

    public PlayerName playerName;
    public PlayerCamera playerCamera;
    public PlayerWeapon playerWeapon;
    public PlayerMovement playerMovement;

    public NetworkVariable<int> playerClass = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool spawning = true;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        LobbyManager.Instance.players[NetworkObjectId] = this;

        if (!IsOwner)
        {
            return;
        }

        currentHealth.OnValueChanged += ShowHealth;
        invTime.OnValueChanged += ChangeHealth;
        playerClass.Value = MenuManager.playerClass;
        RelayManager.connected = true;
    }

    private void ShowHealth(float previous, float current)
    {
        GameManager.Instance.healthBar.text = "";

        for (int i = 0; i < currentHealth.Value; i += 10)
        {
            GameManager.Instance.healthBar.text += "|";
        }
    }

    private void ChangeHealth(float previous, float current)
    {
        if (invTime.Value > 0f)
        {
            GameManager.Instance.healthBar.color = Color.yellow;
        }
        else
        {
            GameManager.Instance.healthBar.color = Color.green;
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
            TakeDamage(100f, NetworkObjectId);
        }

        if (currentHealth.Value <= 0)
        {
            spawning = true;
            PlayerDespawn_ClientRpc("YOU DIED");
            StartCoroutine(Respawn(GameManager.Instance.respawnCooldown));
        }
    }

    public void TakeDamage(float damage, ulong attackerId)
    {
        if (invTime.Value > 0f || currentHealth.Value <= 0f)
        {
            return;
        }

        currentHealth.Value -= damage * (1f - bulletproof.Value);

        PlayerEffect_ClientRpc("damage");

        if (NetworkManager.SpawnManager.SpawnedObjects[attackerId].CompareTag("Player") && attackerId != NetworkObjectId)
        {
            Player attacker = NetworkManager.SpawnManager.SpawnedObjects[attackerId].GetComponent<Player>();

            if (MenuManager.gameMode == 2)
            {
                if (currentHealth.Value <= 0f)
                {
                    attacker.playerScore.Value += 10;
                    GameManager.Instance.Popup_ClientRpc("目標倒地! (分數 +10)", Color.white, true, attackerId);
                }
            }
            else
            {
                if (currentHealth.Value <= 0f)
                {
                    attacker.playerScore.Value -= 10;
                    GameManager.Instance.Popup_ClientRpc("誤殺友軍! (分數 -10)", Color.red, true, attackerId);
                }
                else
                {
                    GameManager.Instance.Popup_ClientRpc("誤傷友軍!", Color.yellow, true, attackerId);
                }
            }
        }
    }

    [ClientRpc]
    public void PlayerEffect_ClientRpc(string type)
    {
        if (!IsOwner)
        {
            return;
        }

        switch (type)
        {
            case "damage":
                GameManager.Instance.effect.color = new Color(1f, 0f, 0f, 100f / 255f);
                GameManager.Instance.timeLeft = 0.5f;
                break;
            case "heal":
                GameManager.Instance.effect.color = new Color(0f, 1f, 0f, 100f / 255f);
                GameManager.Instance.timeLeft = 0.5f;
                break;
            default:
                break;
        }
    }

    [ServerRpc]
    public void PlayerDespawn_ServerRpc()
    {
        currentHealth.Value = 0f;
    }

    [ClientRpc]
    public void PlayerDespawn_ClientRpc(string msg)
    {
        bodyCollider.enabled = false;

        if (IsOwner)
        {
            GameManager.Instance.gamingScreen.SetActive(false);
            GameManager.Instance.deathScreen.SetActive(true);
            GameManager.Instance.deathMessage.text = msg;

            playerName.Despawn();
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

    [ServerRpc]
    public void PlayerRespawn_ServerRpc()
    {
        StartCoroutine(Respawn());
    }

    public IEnumerator Respawn(float seconds = 0)
    {
        yield return new WaitForSeconds(seconds);

        if (GameManager.gameOver)
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
            GameManager.Instance.gamingScreen.SetActive(!GameManager.Instance.pauseScreen.activeSelf);
            GameManager.Instance.deathScreen.SetActive(false);

            playerName.Respawn();
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
