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

    public bool live;

    private void Start()
    {
        audioSource.volume = SampleSceneManager.Instance.volume;

        if (!IsOwner)
        {
            return;
        }

        selectedWeapon.OnValueChanged += SelectWeapon;

        Despawn();
    }

    private void Update()
    {
        if (!IsOwner || MainSceneManager.Instance.start < 2 || MainSceneManager.Instance.gameover || MainSceneManager.disconnecting)
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
