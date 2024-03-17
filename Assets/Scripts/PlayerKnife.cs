using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerKnife : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float fireRate;

    private float nextTimeToAttack;

    private void OnEnable()
    {
        animator.SetTrigger("reset");
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !transform.parent.GetComponent<PlayerWeapon>().live)
        {
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToAttack && Cursor.lockState == CursorLockMode.Locked)
        {
            nextTimeToAttack = Time.time + 1f / fireRate;
            Attack();
        }
    }

    public void Attack()
    {
        animator.SetTrigger("isAttacking");
    }

    public void Respawn()
    {
        nextTimeToAttack = 0f;
    }
}
