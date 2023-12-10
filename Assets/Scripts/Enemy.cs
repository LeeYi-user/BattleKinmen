using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float maxHealth;

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(30, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Transform target;

    public Animator animator;
    private bool running;

    public LayerMask whatIsPlayer;
    public float attackRange;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsHost)
        {
            return;
        }

        target = GameObject.Find("Enemy Target").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsHost)
        {
            return;
        }

        if (MainScene.start && !running)
        {
            running = true;
            agent.SetDestination(target.position);
            animator.SetBool("isRunning", true);
        }

        if (currentHealth.Value <= 0)
        {
            agent.isStopped = true;
            animator.SetTrigger("isDying");
            Destroy(gameObject, 2f);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
    }
}
