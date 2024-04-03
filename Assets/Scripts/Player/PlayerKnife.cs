using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerKnife : NetworkBehaviour
{
    [SerializeField] private float range; // 2
    public NetworkVariable<float> damage = new NetworkVariable<float>(50f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> attackRate = new NetworkVariable<float>(1.25f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Camera fpsCam;
    [SerializeField] private GameObject[] impactEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Animator animator;

    private float nextTimeToAttack;

    private void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        attackRate.OnValueChanged += UpdateAttackAnimSpeed;
    }

    private void OnEnable()
    {
        if (!IsOwner)
        {
            return;
        }

        UpdateAttackAnimSpeed(0, 0);
        animator.SetTrigger("reset");
        GameManager.Instance.ammoBar.text = "";
    }

    private void UpdateAttackAnimSpeed(float previous, float current)
    {
        animator.SetFloat("attackAnimSpeed", attackRate.Value / 1.25f);
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !transform.parent.GetComponent<PlayerWeapon>().live)
        {
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToAttack && Cursor.lockState == CursorLockMode.Locked)
        {
            nextTimeToAttack = Time.time + 1f / attackRate.Value;
            Attack();
        }
    }

    public void Attack()
    {
        animator.SetTrigger("isAttacking");

        RaycastHit hit;
        int MadeImpact = 0;
        int layerMasks = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Enemy");

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range, layerMasks))
        {
            MadeImpact = 1;

            if (hit.transform.gameObject.CompareTag("Player"))
            {
                MadeImpact = 2;
                AttackPlayer_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage.Value);
            }
            else if (hit.transform.gameObject.CompareTag("Enemy"))
            {
                MadeImpact = 2;
                AttackEnemy_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage.Value);
            }
        }

        if (MadeImpact > 0)
        {
            CreateKnifeImpact_ServerRpc(MadeImpact, hit.point, hit.normal);
        }

        if (MadeImpact > 1)
        {
            audioSource.PlayOneShot(audioClip);
            PlayFakeAudioSource_ServerRpc();
        }
    }

    [ServerRpc]
    public void AttackPlayer_ServerRpc(ulong objectId, float damage)
    {
        if (!MenuManager.friendlyFire)
        {
            return;
        }

        NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<Player>().TakeDamage(damage);
    }

    [ServerRpc]
    public void AttackEnemy_ServerRpc(ulong objectId, float damage)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<Enemy>().TakeDamage(damage);
    }

    [ServerRpc]
    public void CreateKnifeImpact_ServerRpc(int MadeImpact, Vector3 HitPoint, Vector3 HitNormal)
    {
        GameObject impactGO = Instantiate(impactEffect[MadeImpact - 1], HitPoint, Quaternion.LookRotation(HitNormal));
        impactGO.GetComponent<NetworkObject>().Spawn(true);
        Destroy(impactGO, 2f);
    }

    [ServerRpc]
    public void PlayFakeAudioSource_ServerRpc()
    {
        PlayFakeAudioSource_ClientRpc();
    }

    [ClientRpc]
    public void PlayFakeAudioSource_ClientRpc()
    {
        if (IsOwner)
        {
            return;
        }

        audioSource.PlayOneShot(audioClip);
    }

    public void Respawn()
    {
        nextTimeToAttack = 0f;
    }
}
