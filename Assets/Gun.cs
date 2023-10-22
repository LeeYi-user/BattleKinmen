﻿using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    // 元件用途: 操控玩家射擊
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private float damage;  // 15
    [SerializeField] private float range; // 100
    [SerializeField] private float fireRate; // 2

    [SerializeField] private Camera fpsCam;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject[] impactEffect;

    [SerializeField] private int maxAmmo; // 7
    [SerializeField] private float reloadTime; // 1.35

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private TrailRenderer BulletTrail;
    [SerializeField] private float BulletSpeed; // 100

    [SerializeField] private Animator fakeAnimator;
    [SerializeField] private AudioSource fakeAudioSource;
    [SerializeField] private ParticleSystem fakeMuzzleFlash;
    [SerializeField] private Transform fakeBulletSpawnPoint;

    private int currentAmmo;
    private bool isReloading;
    private float nextTimeToFire;
    private bool live;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        fakeMuzzleFlash.gameObject.SetActive(false);
        currentAmmo = maxAmmo;
        isReloading = false;
        nextTimeToFire = 0f;
        live = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            return;
        }

        if (isReloading || !live)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") && Cursor.lockState == CursorLockMode.Locked && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        animator.SetBool("isReloading", true);
        fakeAnimator.SetBool("isReloading", true);

        yield return new WaitForSeconds(reloadTime);

        animator.SetBool("isReloading", false);
        fakeAnimator.SetBool("isReloading", false);

        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void Shoot()
    {
        currentAmmo--;

        muzzleFlash.Play();
        PlayFakeMuzzleFlash_ServerRpc(NetworkObjectId);
        animator.SetTrigger("isFiring");
        fakeAnimator.SetTrigger("isFiring");
        audioSource.PlayOneShot(audioClip);
        PlayFakeAudioSource_ServerRpc(NetworkObjectId);

        RaycastHit hit;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range, LayerMask.GetMask("Hittable")))
        {
            int MadeImpact = 0;

            if (hit.transform.gameObject.CompareTag("Player"))
            {
                MadeImpact = 1;
                ShootPlayer_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage);
            }

            CreateBulletTrail_ServerRpc(NetworkManager.Singleton.LocalClientId, BulletSpawnPoint.position, true, hit.point, hit.normal, MadeImpact, fpsCam.transform.forward);
            CreateBulletTrail_ServerRpc(NetworkManager.Singleton.LocalClientId, fakeBulletSpawnPoint.position, false, hit.point, hit.normal, MadeImpact, fpsCam.transform.forward);
        }
        else
        {
            CreateBulletTrail_ServerRpc(NetworkManager.Singleton.LocalClientId, BulletSpawnPoint.position, true, hit.point, hit.normal, -1, fpsCam.transform.forward);
            CreateBulletTrail_ServerRpc(NetworkManager.Singleton.LocalClientId, fakeBulletSpawnPoint.position, false, hit.point, hit.normal, -1, fpsCam.transform.forward);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFakeMuzzleFlash_ServerRpc(ulong objectId)
    {
        PlayFakeMuzzleFlash_ClientRpc(objectId);
    }

    [ClientRpc]
    private void PlayFakeMuzzleFlash_ClientRpc(ulong objectId)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<Gun>().fakeMuzzleFlash.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFakeAudioSource_ServerRpc(ulong objectId)
    {
        PlayFakeAudioSource_ClientRpc(objectId);
    }

    [ClientRpc]
    private void PlayFakeAudioSource_ClientRpc(ulong objectId)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<Gun>().fakeAudioSource.PlayOneShot(audioClip);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootPlayer_ServerRpc(ulong objectId, float damage)
    {
        ShootPlayer_ClientRpc(objectId, damage);
    }

    [ClientRpc]
    private void ShootPlayer_ClientRpc(ulong objectId, float damage)
    {
        NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject.GetComponent<PlayerHealth>().TakeDamage(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateBulletTrail_ServerRpc(ulong playerId, Vector3 position, bool real, Vector3 HitPoint, Vector3 HitNormal, int MadeImpact, Vector3 forward)
    {
        TrailRenderer trail = Instantiate(BulletTrail, position, Quaternion.identity);

        trail.GetComponent<NetworkObject>().Spawn(true);

        if (MadeImpact > -1)
        {
            StartCoroutine(SpawnTrail(trail, HitPoint, HitNormal, MadeImpact, real));
        }
        else
        {
            StartCoroutine(SpawnTrail(trail, position + forward * range, Vector3.zero, MadeImpact, real));
        }

        CreateBulletTrail_ClientRpc(trail.GetComponent<NetworkObject>().NetworkObjectId, playerId, real);
    }

    [ClientRpc]
    private void CreateBulletTrail_ClientRpc(ulong objectId, ulong playerId, bool real)
    {
        GameObject trailGO = NetworkManager.SpawnManager.SpawnedObjects[objectId].gameObject;

        if ((playerId == NetworkManager.Singleton.LocalClientId && !real) || (playerId != NetworkManager.Singleton.LocalClientId && real))
        {
            trailGO.SetActive(false);
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, int MadeImpact, bool real)
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

        if ((MadeImpact > -1) && real)
        {
            GameObject impactGO = Instantiate(impactEffect[MadeImpact], HitPoint, Quaternion.LookRotation(HitNormal));
            impactGO.GetComponent<NetworkObject>().Spawn(true);
            Destroy(impactGO, 2f);
        }

        Destroy(Trail.gameObject, Trail.time);
    }

    public void Despawn()
    {
        live = false;
    }

    public void Respawn()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        nextTimeToFire = 0f;
        live = true;
    }
}
