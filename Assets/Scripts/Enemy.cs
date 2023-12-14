using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float maxHealth;

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Transform target;
    private bool running;
    private bool dying;

    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackRate;

    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Animator animator;

    private float nextTimeToAttack;

    private void Start()
    {
        if (!IsHost)
        {
            return;
        }

        target = GameObject.Find("Enemy Target").transform;
    }

    private void Update()
    {
        if (!IsHost)
        {
            return;
        }

        if (MainSceneManager.start && !running)
        {
            running = true;
            currentHealth.Value = maxHealth;
            agent.SetDestination(target.position);
            animator.SetBool("isRunning", true);
        }

        if (Physics.CheckSphere(transform.position, attackRange, whatIsPlayer) && Time.time >= nextTimeToAttack && !dying)
        {
            nextTimeToAttack = Time.time + 1f / attackRate;
            StartCoroutine(Shoot());
        }

        if (currentHealth.Value <= 0 && !dying)
        {
            dying = true;
            Die();
        }

        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            Invade();
        }

        if (MainSceneManager.gameover)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Shoot()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        GameObject player = FindClosestPlayer();

        transform.LookAt(player.transform);
        animator.SetTrigger("isFiring");
        PlayMuzzleFlash_ClientRpc();
        PlayAudioSource_ClientRpc();
        player.GetComponent<Player>().TakeDamage(30f);

        yield return new WaitForSeconds(0.8f);

        agent.isStopped = false;
    }

    public GameObject FindClosestPlayer()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;

            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        return closest;
    }

    [ClientRpc]
    private void PlayMuzzleFlash_ClientRpc()
    {
        muzzleFlash.Play();
    }

    [ClientRpc]
    private void PlayAudioSource_ClientRpc()
    {
        audioSource.PlayOneShot(audioClip);
    }

    private void Die()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        animator.SetTrigger("isDying");
        Destroy(gameObject, 2f);

        EnemySpawn.enemyCounter--;
    }

    private void Invade()
    {
        MainSceneManager.playerLives--;
        EnemySpawn.enemyCounter--;
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
    }
}
