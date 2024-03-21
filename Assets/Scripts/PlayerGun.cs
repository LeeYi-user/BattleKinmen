using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerGun : NetworkBehaviour
{
    [SerializeField] private float range; // 100
    public NetworkVariable<float> damage = new NetworkVariable<float>(30f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> fireRate = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> maxAmmo = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public ClientNetworkVariable<int> currentAmmo = new ClientNetworkVariable<int>(0);
    public NetworkVariable<float> reloadTime = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private Camera fpsCam;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem fakeMuzzleFlash;
    [SerializeField] private GameObject[] impactEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Animator animator;

    private bool isFiring;
    private bool isReloading;
    private float nextTimeToFire;

    private void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        maxAmmo.OnValueChanged += UpdateAmmo;
        currentAmmo.OnValueChanged += ShowAmmo;
        fireRate.OnValueChanged += UpdateFireAnimSpeed;
        reloadTime.OnValueChanged += UpdateReloadAnimSpeed;
    }

    private void UpdateAmmo(int previous, int current)
    {
        if (!IsOwner || isReloading)
        {
            return;
        }

        currentAmmo.Value += current - previous;
    }

    private void ShowAmmo()
    {
        MainSceneManager.Instance.ammoBar.text = "";

        for (int i = 0; i < currentAmmo.Value; i += 1)
        {
            MainSceneManager.Instance.ammoBar.text += "|";
        }
    }

    private void OnEnable()
    {
        if (!IsOwner)
        {
            return;
        }

        UpdateFireAnimSpeed(0, 0);
        UpdateReloadAnimSpeed(0, 0);
        animator.SetTrigger("reset");

        isFiring = false;
        isReloading = false;

        if (currentAmmo.Value <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
    }

    private void UpdateFireAnimSpeed(float previous, float current)
    {
        animator.SetFloat("fireAnimSpeed", fireRate.Value / 0.5f);
    }

    private void UpdateReloadAnimSpeed(float previous, float current)
    {
        animator.SetFloat("reloadAnimSpeed", 1f / reloadTime.Value);
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !transform.parent.GetComponent<PlayerWeapon>().live)
        {
            return;
        }

        if (isFiring || isReloading)
        {
            return;
        }

        if (currentAmmo.Value <= 0 || (currentAmmo.Value < maxAmmo.Value && Input.GetKey(KeyCode.R)))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && Cursor.lockState == CursorLockMode.Locked)
        {
            nextTimeToFire = Time.time + 1f / fireRate.Value;
            Shoot();
        }
    }

    public void FinishFiring()
    {
        isFiring = false;
        animator.SetBool("isFiring", false);
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        animator.SetBool("isReloading", true);

        yield return new WaitForSeconds(reloadTime.Value);

        isReloading = false;
        animator.SetBool("isReloading", false);

        currentAmmo.Value = maxAmmo.Value;
    }

    private void Shoot()
    {
        isFiring = true;
        currentAmmo.Value--;

        muzzleFlash.Play();
        PlayFakeMuzzleFlash_ServerRpc();
        animator.SetBool("isFiring", true);
        audioSource.PlayOneShot(audioClip);
        PlayFakeAudioSource_ServerRpc();

        RaycastHit hit;
        int MadeImpact = 0;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            MadeImpact = 1;

            if (hit.transform.gameObject.CompareTag("Player"))
            {
                MadeImpact = 2;
                ShootPlayer_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage.Value);
            }
            else if (hit.transform.gameObject.CompareTag("Enemy"))
            {
                MadeImpact = 2;
                ShootEnemy_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage.Value);
            }
        }

        if (MadeImpact > 0)
        {
            CreateBulletImpact_ServerRpc(MadeImpact, hit.point, hit.normal);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFakeMuzzleFlash_ServerRpc()
    {
        PlayFakeMuzzleFlash_ClientRpc();
    }

    [ClientRpc]
    private void PlayFakeMuzzleFlash_ClientRpc()
    {
        if (!IsOwner)
        {
            fakeMuzzleFlash.Play();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFakeAudioSource_ServerRpc()
    {
        PlayFakeAudioSource_ClientRpc();
    }

    [ClientRpc]
    private void PlayFakeAudioSource_ClientRpc()
    {
        if (!IsOwner)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootPlayer_ServerRpc(ulong objectId, float damage)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<Player>().TakeDamage(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootEnemy_ServerRpc(ulong objectId, float damage)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<Enemy>().TakeDamage(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateBulletImpact_ServerRpc(int MadeImpact, Vector3 HitPoint, Vector3 HitNormal)
    {
        GameObject impactGO = Instantiate(impactEffect[MadeImpact - 1], HitPoint, Quaternion.LookRotation(HitNormal));
        impactGO.GetComponent<NetworkObject>().Spawn(true);
        Destroy(impactGO, 2f);
    }

    public void Respawn()
    {
        isReloading = false;
        currentAmmo.Value = maxAmmo.Value;
        nextTimeToFire = 0f;
    }
}
