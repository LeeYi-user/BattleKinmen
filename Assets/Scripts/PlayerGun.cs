using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; // 這個腳本有網路相關的 code, 所以要導入這個 package
using UnityEngine;

public class PlayerGun : NetworkBehaviour // 因為跟網路有關, 所以除了源物件要放 NetworkObject 之外, 這裡也要用 NetworkBehaviour
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
    [SerializeField] private float BulletSpeed; // 100

    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private TrailRenderer BulletTrail;

    [SerializeField] private ParticleSystem fakeMuzzleFlash;
    [SerializeField] private Animator fakeAnimator;
    [SerializeField] private AudioSource fakeAudioSource;
    [SerializeField] private Transform fakeBulletSpawnPoint;

    private Animator animator;
    private AudioSource audioSource;
    private int currentAmmo;
    private bool isReloading;
    private float nextTimeToFire;
    private bool live;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner) // 如果該玩家物件不是自己操控的
        {
            return; // 就直接 return
        }

        fakeMuzzleFlash.gameObject.SetActive(false);
        fakeAudioSource.enabled = false;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
        isReloading = false;
        nextTimeToFire = 0f;
        Despawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !live) // 如果該玩家物件不是自己操控的
        {
            return; // 就直接 return
        }

        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)) && Cursor.lockState == CursorLockMode.None)
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

        isReloading = false;
        animator.SetBool("isReloading", false);
        fakeAnimator.SetBool("isReloading", false);

        currentAmmo = maxAmmo;
    }

    void Shoot() // 射擊
    {
        currentAmmo--;

        muzzleFlash.Play(); // 這裡會在第一人稱視角的 client 端顯示火花
        PlayFakeMuzzleFlash_ServerRpc(NetworkManager.LocalClientId); // 這裡會告知 server 去生成火花, 以便其他玩家能夠看到 (註: 其他人看的是自己看不見的第三人稱火花)
        animator.SetTrigger("isFiring"); // 這裡會在第一人稱視角的 client 端顯示開火
        fakeAnimator.SetTrigger("isFiring"); // 因為已經有 OwnerNetworkAnimator 來同步動畫了, 所以不需要使用 RPC 便能讓其他玩家看到 (註: 其他人看的是自己看不見的第三人稱開火)
        audioSource.PlayOneShot(audioClip); // 這裡會在 client 端播放槍聲
        PlayFakeAudioSource_ServerRpc(NetworkManager.LocalClientId); // 這裡會告知 server 去播放槍聲, 以便其他玩家能夠聽到 (註: 其他人聽的是自己聽不見的第三人稱槍聲)

        RaycastHit hit;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range, LayerMask.GetMask("Hittable")))
        {
            int MadeImpact = 0;

            if (hit.transform.gameObject.CompareTag("Player"))
            {
                MadeImpact = 1;
                ShootPlayer_ServerRpc(hit.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId, damage);
            }

            // 這裡會創造兩道子彈軌跡, 一道是給自己看的第一人稱子彈軌跡, 一道是給其他人看的第三人稱子彈軌跡, 兩道都需要用 RPC 來告知 Server 去生成 (註: 因為 client 端無法用 Instantiate 函式)
            CreateBulletTrail_ServerRpc(NetworkManager.LocalClientId, BulletSpawnPoint.position, true, hit.point, hit.normal, MadeImpact, fpsCam.transform.forward);
            CreateBulletTrail_ServerRpc(NetworkManager.LocalClientId, fakeBulletSpawnPoint.position, false, hit.point, hit.normal, MadeImpact, fpsCam.transform.forward);
        }
        else
        {
            // 同上, 只是負責沒打中時的子彈軌跡
            CreateBulletTrail_ServerRpc(NetworkManager.LocalClientId, BulletSpawnPoint.position, true, hit.point, hit.normal, -1, fpsCam.transform.forward);
            CreateBulletTrail_ServerRpc(NetworkManager.LocalClientId, fakeBulletSpawnPoint.position, false, hit.point, hit.normal, -1, fpsCam.transform.forward);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFakeMuzzleFlash_ServerRpc(ulong playerId)
    {
        PlayFakeMuzzleFlash_ClientRpc(playerId);
    }

    [ClientRpc]
    private void PlayFakeMuzzleFlash_ClientRpc(ulong playerId)
    {
        if (playerId != NetworkManager.LocalClientId)
        {
            fakeMuzzleFlash.Play();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFakeAudioSource_ServerRpc(ulong playerId)
    {
        PlayFakeAudioSource_ClientRpc(playerId);
    }

    [ClientRpc]
    private void PlayFakeAudioSource_ClientRpc(ulong playerId)
    {
        if (playerId != NetworkManager.LocalClientId)
        {
            fakeAudioSource.PlayOneShot(audioClip);
        }
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

    [ServerRpc(RequireOwnership = false)] // 這裡的 playerId 是紀錄 client 的 LocalClientId, 而不是紀錄物件的 NetworkObjectId. 之所以用這個, 是因為 NetworkObjectId 無法在 RPC 中代表當前實例
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

        // 如果子彈軌跡是自己的 且 是給其他人看的第三人稱軌跡 那就對自己隱藏
        // 如果子彈軌跡是別人的 且 是給那個人看的第一人稱軌跡 那也對自己隱藏
        if ((playerId == NetworkManager.LocalClientId && !real) || (playerId != NetworkManager.LocalClientId && real))
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
        live = true;
        currentAmmo = maxAmmo;
        isReloading = false;
        nextTimeToFire = 0f;
    }
}
