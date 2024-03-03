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
    [SerializeField] private GameObject fakeGunSkin;

    [SerializeField] private PlayerGun playerGun;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private bool spawning = true;

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
            GameObject.Find("Crosshair").GetComponent<Image>().enabled = false;
            GameObject.Find("Death Screen").GetComponent<Image>().enabled = true;
            GameObject.Find("Death Message").GetComponent<TMP_Text>().text = msg;
            GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = true;

            playerGun.Despawn();
            playerCamera.Despawn();
            playerMovement.Despawn();
        }
        else
        {
            foreach (SkinnedMeshRenderer bodySkin in bodySkins)
            {
                bodySkin.enabled = false;
            }

            fakeGunSkin.SetActive(false);
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
            GameObject.Find("Crosshair").GetComponent<Image>().enabled = true;
            GameObject.Find("Death Screen").GetComponent<Image>().enabled = false;
            GameObject.Find("Death Message").GetComponent<TMP_Text>().enabled = false;

            playerGun.Respawn();
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

            fakeGunSkin.SetActive(true);
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
