using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Unity.Netcode;

public class PlayerWeapon : NetworkBehaviour
{
    [HideInInspector] public ClientNetworkVariable<int> selectedWeapon = new ClientNetworkVariable<int>(-1);

    [SerializeField] private Transform thirdPersonWeapons;
    [SerializeField] private Transform thirdPersonRigs;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;

    [SerializeField] private GameObject grenade;
    [SerializeField] private GameObject landmine;

    public bool live;

    private void Start()
    {
        audioSource.volume = SampleSceneManager.volume;

        if (!IsOwner)
        {
            return;
        }

        selectedWeapon.OnValueChanged += SelectWeapon;

        Despawn();
    }

    private void Update()
    {
        if (!IsOwner || !live)
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Transform playerCamera = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>().playerCamera.transform;
            LaunchGrenade_ServerRpc(playerCamera.position, playerCamera.forward, playerCamera.right);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Transform playerCamera = NetworkManager.LocalClient.PlayerObject.GetComponent<Player>().playerCamera.transform;
            PlaceLandmine_ServerRpc(playerCamera.position);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Vector3 playerPosition = NetworkManager.LocalClient.PlayerObject.transform.position;
            HealPlayers_ServerRpc(playerPosition);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void LaunchGrenade_ServerRpc(Vector3 pos, Vector3 zAxis, Vector3 xAxis)
    {
        GameObject grenadeGO = Instantiate(grenade, pos, Random.rotation);
        grenadeGO.GetComponent<Grenade>().forceAxis = zAxis;
        grenadeGO.GetComponent<Grenade>().rotateAxis = xAxis;
        grenadeGO.GetComponent<NetworkObject>().Spawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaceLandmine_ServerRpc(Vector3 pos)
    {
        RaycastHit hit;

        if (Physics.Raycast(pos, Vector3.down, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
        {
            GameObject landmineGO = Instantiate(landmine, hit.point, Quaternion.Euler(-90f, 0f, Random.Range(0f, 360f)));
            landmineGO.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealPlayers_ServerRpc(Vector3 pos)
    {
        Collider[] colliders = Physics.OverlapSphere(pos, 6f, 1 << LayerMask.NameToLayer("Player"));

        foreach (Collider collider in colliders)
        {
            Player player = collider.transform.parent.GetComponent<Player>();
            player.currentHealth.Value = Mathf.Min(player.currentHealth.Value + 50, player.maxHealth.Value);
            player.invTime.Value += 1f;
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
        live = true;
        selectedWeapon.Value = 0;

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
    }
}
