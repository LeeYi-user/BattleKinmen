using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.Netcode;

public class PlayerWeapon : NetworkBehaviour
{
    [HideInInspector] public CustomVariable<int> selectedWeapon = new CustomVariable<int>(-1);

    [SerializeField] private Transform thirdPersonWeapons;
    [SerializeField] private Transform thirdPersonRigs;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;

    [SerializeField] private GameObject grenade;
    [SerializeField] private GameObject landmine;

    public NetworkVariable<float> grenadeCooldown = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // -2
    public NetworkVariable<float> grenadeRange = new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +0.25
    public NetworkVariable<float> grenadeDamage = new NetworkVariable<float>(30f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +15
    public NetworkVariable<float> grenadeDistance = new NetworkVariable<float>(900f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +100

    public NetworkVariable<float> landmineCooldown = new NetworkVariable<float>(10f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // -2
    public NetworkVariable<float> landmineRange = new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +0.25
    public NetworkVariable<float> landmineDamage = new NetworkVariable<float>(30f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +15
    public NetworkVariable<float> landmineLimit = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +1

    public NetworkVariable<float> healCooldown = new NetworkVariable<float>(30f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // -5
    public NetworkVariable<float> healRange = new NetworkVariable<float>(6f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +1.5
    public NetworkVariable<float> healAmount = new NetworkVariable<float>(50f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +25
    public NetworkVariable<float> healInv = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // +0.5

    public bool live;

    private float cooldown;

    public List<GameObject> landmines;

    private void Start()
    {
        audioSource.volume = MenuManager.volume;

        if (!IsOwner)
        {
            return;
        }

        selectedWeapon.OnValueChanged += SelectWeapon;

        Despawn();
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }

        if (!live || Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (selectedWeapon.Value >= transform.childCount - 1)
            {
                selectedWeapon.Value = 0;
            }
            else
            {
                selectedWeapon.Value++;
            }
        }

        if (cooldown > 0 || GameManager.Instance.skillDisable)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Transform player = NetworkManager.LocalClient.PlayerObject.transform;
            Transform playerCamera = player.GetComponent<Player>().playerCamera.transform;

            switch (MenuManager.playerClass)
            {
                case 0:
                    cooldown = grenadeCooldown.Value;
                    LaunchGrenade_ServerRpc(playerCamera.position, playerCamera.forward, playerCamera.right);
                    break;
                case 1:
                    cooldown = landmineCooldown.Value;
                    PlaceLandmine_ServerRpc(playerCamera.position);
                    break;
                case 2:
                    cooldown = healCooldown.Value;
                    HealPlayers_ServerRpc(player.position);
                    break;
                default:
                    break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void LaunchGrenade_ServerRpc(Vector3 pos, Vector3 zAxis, Vector3 xAxis)
    {
        GameObject grenadeGO = Instantiate(grenade, pos, Random.rotation);
        grenadeGO.GetComponent<Grenade>().forceAxis = zAxis;
        grenadeGO.GetComponent<Grenade>().rotateAxis = xAxis;
        grenadeGO.GetComponent<Grenade>().ownerId = NetworkObjectId;
        grenadeGO.GetComponent<Grenade>().force = grenadeDistance.Value;
        grenadeGO.GetComponent<Grenade>().explosionRange = grenadeRange.Value;
        grenadeGO.GetComponent<Grenade>().explosionDamage = grenadeDamage.Value;
        grenadeGO.GetComponent<NetworkObject>().Spawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceLandmine_ServerRpc(Vector3 pos)
    {
        RaycastHit hit;

        if (Physics.Raycast(pos, Vector3.down, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
        {
            GameObject landmineGO = Instantiate(landmine, hit.point, Quaternion.Euler(-90f, 0f, Random.Range(0f, 360f)));
            landmineGO.GetComponent<Landmine>().ownerId = NetworkObjectId;
            landmineGO.GetComponent<Landmine>().explosionRange = landmineRange.Value;
            landmineGO.GetComponent<Landmine>().explosionDamage = landmineDamage.Value;
            landmineGO.GetComponent<NetworkObject>().Spawn(true);
            landmines.Add(landmineGO);

            if (landmines.Count > landmineLimit.Value)
            {
                Destroy(landmines[0]);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealPlayers_ServerRpc(Vector3 pos)
    {
        Collider[] colliders = Physics.OverlapSphere(pos, healRange.Value, 1 << LayerMask.NameToLayer("Player"));

        foreach (Collider collider in colliders)
        {
            Player player = collider.transform.parent.GetComponent<Player>();
            player.currentHealth.Value = Mathf.Min(player.currentHealth.Value + healAmount.Value, player.maxHealth.Value);
            player.invTime.Value += healInv.Value;
        }
    }

    public void SelectWeapon()
    {
        animator.SetInteger("selectedWeapon", selectedWeapon.Value);

        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon.Value)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }

        SelectWeapon_ServerRpc(selectedWeapon.Value);
    }

    [ServerRpc]
    public void SelectWeapon_ServerRpc(int index)
    {
        SelectWeapon_ClientRpc(index);
    }

    [ClientRpc]
    public void SelectWeapon_ClientRpc(int index)
    {
        if (IsOwner)
        {
            return;
        }

        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(false);
        }

        int i = 0;

        foreach (Transform weapon in thirdPersonWeapons)
        {
            if (i == index)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }

        int j = 0;

        foreach (Transform rig in thirdPersonRigs)
        {
            if (j == index)
            {
                rig.GetComponent<Rig>().weight = 1f;
            }
            else
            {
                rig.GetComponent<Rig>().weight = 0f;
            }

            j++;
        }
    }

    public void Despawn()
    {
        live = false;
        selectedWeapon.Value = -1;
    }

    public void Respawn()
    {
        foreach (Transform weapon in transform)
        {
            if (weapon.GetComponent<PlayerGun>() != null)
            {
                weapon.GetComponent<PlayerGun>().Respawn();
            }
            else if (weapon.GetComponent<PlayerKnife>() != null)
            {
                weapon.GetComponent<PlayerKnife>().Respawn();
            }
        }

        live = true;
        selectedWeapon.Value = 0;
    }
}
