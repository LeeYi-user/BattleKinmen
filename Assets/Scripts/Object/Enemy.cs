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

    private float currentHealth;

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
        audioSource.volume = MenuManager.volume;

        if (!IsHost)
        {
            return;
        }

        currentHealth = EnemyManager.Instance.enemyHealth;
        target = EnemyManager.Instance.enemyTarget;
    }

    private void Update()
    {
        if (!IsHost || destroying)
        {
            return;
        }

        if (PlayerManager.gameOver)
        {
            destroying = true;
            Destroy(gameObject);
            return;
        }

        if (!running)
        {
            running = true;
            agent.SetDestination(target.position);
            animator.SetBool("isRunning", true);
        }

        if (currentHealth <= 0)
        {
            destroying = true;
            Die();
            return;
        }

        if (Grid.CheckPosition(transform.position, target))
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

        EnemyManager.Instance.enemies.Value--;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (!IsHost || invading || PlayerManager.gameOver || RelayManager.disconnecting || !ShopManager.Instance)
        {
            return;
        }

        GameObject cashGO = Instantiate(cash, hips.position, Quaternion.identity);
        cashGO.GetComponent<NetworkObject>().Spawn(true);
    }

    private void Invade()
    {
        invading = true;

        if (TeamModeManager.Instance)
        {
            TeamModeManager.Instance.currentDefense--;
        }

        EnemyManager.Instance.enemies.Value--;
        Destroy(gameObject);
    }

    private IEnumerator Shoot()
    {
        List<Transform> players = new List<Transform>();

        foreach (NetworkClient client in NetworkManager.ConnectedClients.Values)
        {
            players.Add(client.PlayerObject.transform);
        }

        Transform player = GetClosestPlayer(players);

        if (!Physics.Linecast(head.transform.position, player.GetComponent<Player>().head.position, whatIsGround))
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            transform.LookAt(player.transform);
            animator.SetTrigger("isFiring");
            PlayMuzzleFlash_ClientRpc();
            PlayAudioSource_ClientRpc();
            player.GetComponent<Player>().TakeDamage(EnemyManager.Instance.enemyDamage);

            yield return new WaitForSeconds(0.8f);

            if (!destroying)
            {
                agent.isStopped = false;
            }
        }
    }

    Transform GetClosestPlayer(List<Transform> players)
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (Transform t in players)
        {
            float dist = Vector3.Distance(t.position, currentPos);

            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }

        return tMin;
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
        currentHealth -= damage;
    }
}
