using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerGun : NetworkBehaviour
{
    [SerializeField] private float damage; // 30
    [SerializeField] private float range; // 100
    [SerializeField] private float fireRate; // 0.5

    [SerializeField] private Camera fpsCam;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject[] impactEffect;

    [SerializeField] private int maxAmmo; // 5
    [SerializeField] private float reloadTime; // 1

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Animator animator;

    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private TrailRenderer BulletTrail;
    [SerializeField] private float BulletSpeed; // 100

    [SerializeField] private ParticleSystem fakeMuzzleFlash;
    [SerializeField] private Transform fakeBulletSpawnPoint;

    private bool isReloading;
    private ClientNetworkVariable<int> currentAmmo = new ClientNetworkVariable<int>(0);
    private float nextTimeToFire;

    private void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        currentAmmo.OnValueChanged += ShowAmmo;
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
        animator.SetTrigger("reset");

        isReloading = false;

        if (currentAmmo.Value <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !transform.parent.GetComponent<PlayerWeapon>().live)
        {
            return;
        }

        if (isReloading)
        {
            return;
        }

        if (currentAmmo.Value <= 0 || (currentAmmo.Value < maxAmmo && Input.GetKey(KeyCode.R)))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && Cursor.lockState == CursorLockMode.Locked)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        animator.SetBool("isReloading", true);

        yield return new WaitForSeconds(reloadTime);

        isReloading = false;
        animator.SetBool("isReloading", false);

        currentAmmo.Value = maxAmmo;
    }

    private void Shoot()
    {
        currentAmmo.Value--;

        muzzleFlash.Play();
        PlayFakeMuzzleFlash_ServerRpc();
        animator.SetTrigger("isFiring");
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
                ShootPlayer_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage);
            }
            else if (hit.transform.gameObject.CompareTag("Enemy"))
            {
                MadeImpact = 2;
                ShootEnemy_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage);
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
        currentAmmo.Value = maxAmmo;
        nextTimeToFire = 0f;
    }
}
