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

        if (MainScene.gameover)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Shoot()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        GameObject player = FindClosestPlayer();

        transform.LookAt(player.transform);
        animator.SetTrigger("isFiring");
        PlayMuzzleFlash_ClientRpc();
        PlayAudioSource_ClientRpc();
        player.GetComponent<PlayerHealth>().TakeDamage(30f);

        yield return new WaitForSeconds(0.8f);

        agent.isStopped = false;
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

    void Die()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        animator.SetTrigger("isDying");
        Destroy(gameObject, 2f);

        EnemySpawn.counter--;
    }

    void Invade()
    {
        MainScene.lives--;
        EnemySpawn.counter--;
        Destroy(gameObject);
    }

    public GameObject FindClosestPlayer()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Player");
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

    public void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
    }
}
