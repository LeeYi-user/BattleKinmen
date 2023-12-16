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

    [SerializeField] private GameObject realGunSkin;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Animator animator;

    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private TrailRenderer BulletTrail;
    [SerializeField] private float BulletSpeed; // 100

    [SerializeField] private ParticleSystem fakeMuzzleFlash;
    [SerializeField] private AudioSource fakeAudioSource;
    [SerializeField] private Transform fakeBulletSpawnPoint;

    private bool isReloading;
    private int currentAmmo;
    private float nextTimeToFire;
    private bool live;

    private void Start()
    {
        audioSource.volume = MenuSceneManager.volume;
        fakeAudioSource.volume = MenuSceneManager.volume;

        if (!IsOwner)
        {
            realGunSkin.SetActive(false);
            return;
        }

        Despawn();
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        if ((Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1)) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }

        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0)
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

        currentAmmo = maxAmmo;
    }

    private void Shoot()
    {
        currentAmmo--;

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

        CreateBulletTrail_ServerRpc(BulletSpawnPoint.position, true, hit.point, hit.normal, MadeImpact, fpsCam.transform.forward);
        CreateBulletTrail_ServerRpc(fakeBulletSpawnPoint.position, false, hit.point, hit.normal, MadeImpact, fpsCam.transform.forward);
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
            fakeAudioSource.PlayOneShot(audioClip);
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
    private void CreateBulletTrail_ServerRpc(Vector3 position, bool IsReal, Vector3 HitPoint, Vector3 HitNormal, int MadeImpact, Vector3 forward)
    {
        TrailRenderer trail = Instantiate(BulletTrail, position, Quaternion.identity);

        trail.GetComponent<NetworkObject>().Spawn(true);

        if (MadeImpact > 0)
        {
            StartCoroutine(SpawnTrail(trail, HitPoint, HitNormal, MadeImpact, IsReal));
        }
        else
        {
            StartCoroutine(SpawnTrail(trail, position + forward * range, Vector3.zero, MadeImpact, IsReal));
        }

        CreateBulletTrail_ClientRpc(trail.GetComponent<NetworkObject>().NetworkObjectId, IsReal);
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, int MadeImpact, bool IsReal)
    {
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= BulletSpeed * Time.deltaTime;
            yield return null;
        }

        Trail.transform.position = HitPoint;

        if ((MadeImpact > 0) && IsReal)
        {
            GameObject impactGO = Instantiate(impactEffect[MadeImpact - 1], HitPoint, Quaternion.LookRotation(HitNormal));
            impactGO.GetComponent<NetworkObject>().Spawn(true);
            Destroy(impactGO, 2f);
        }

        Destroy(Trail.gameObject, Trail.time);
    }

    [ClientRpc]
    private void CreateBulletTrail_ClientRpc(ulong objectId, bool IsReal)
    {
        GameObject trailGO = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject;

        trailGO.SetActive(false);

        //if (IsOwner != IsReal)
        //{
        //    trailGO.SetActive(false);
        //}
    }

    public void Despawn()
    {
        live = false;
        realGunSkin.SetActive(false);
    }

    public void Respawn()
    {
        live = true;
        isReloading = false;
        currentAmmo = maxAmmo;
        nextTimeToFire = 0f;
        realGunSkin.SetActive(true);
    }
}
