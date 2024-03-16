using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject cash;
    [SerializeField] private Transform head;
    [SerializeField] private Transform hips;
    [SerializeField] private float maxHealth;

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackRate;

    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private Animator animator;

    private Transform target;
    private bool running;
    private bool invading;
    private bool destroying;
    private float nextTimeToAttack;

    private void Start()
    {
        audioSource.volume = SampleSceneManager.Instance.volume;

        if (!IsHost)
        {
            return;
        }

        target = GameObject.Find("Enemy Target").transform;
    }

    private void Update()
    {
        if (!IsHost || destroying)
        {
            return;
        }

        if (MainSceneManager.Instance.gameover)
        {
            destroying = true;
            Destroy(gameObject);
            return;
        }

        if (!running)
        {
            running = true;
            currentHealth.Value = maxHealth;
            agent.SetDestination(target.position);
            animator.SetBool("isRunning", true);
        }

        if (currentHealth.Value <= 0)
        {
            destroying = true;
            Die();
            return;
        }

        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            destroying = true;
            Invade();
            return;
        }

        if (Physics.CheckSphere(transform.position, attackRange, whatIsPlayer) && Time.time >= nextTimeToAttack)
        {
            nextTimeToAttack = Time.time + 1f / attackRate;
            StartCoroutine(Shoot());
        }
    }

    private void Die()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        animator.SetTrigger("isDying");
        Destroy(gameObject, 2f);

        EnemySpawn.Instance.enemies.Value--;
        EnemySpawn.Instance.leftForSpawn++;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (!IsHost || invading || MainSceneManager.Instance.gameover || MainSceneManager.disconnecting)
        {
            return;
        }

        GameObject cashGO = Instantiate(cash, hips.position, Quaternion.identity);
        cashGO.GetComponent<NetworkObject>().Spawn(true);
    }

    private void Invade()
    {
        invading = true;
        MainSceneManager.Instance.mapDefense--;
        EnemySpawn.Instance.enemies.Value--;
        EnemySpawn.Instance.leftForSpawn++;
        Destroy(gameObject);
    }

    private IEnumerator Shoot()
    {
        GameObject player = FindClosestPlayer();

        if (!Physics.Linecast(head.transform.position, player.transform.Find("Main Camera").transform.position, whatIsGround))
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            transform.LookAt(player.transform);
            animator.SetTrigger("isFiring");
            PlayMuzzleFlash_ClientRpc();
            PlayAudioSource_ClientRpc();
            player.GetComponent<Player>().TakeDamage(30f);

            yield return new WaitForSeconds(0.8f);

            if (!destroying)
            {
                agent.isStopped = false;
            }
        }
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

    public void TakeDamage(float damage)
    {
        currentHealth.Value -= damage;
    }
}
