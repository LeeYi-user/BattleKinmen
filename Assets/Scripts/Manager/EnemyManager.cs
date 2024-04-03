using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] private Transform enemyTarget;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private TextMeshProUGUI waveCounter;

    public List<Transform> spawnArea;

    private float enemyHealth;
    private float enemyDamage;
    private int leftToSpawn;
    private float timeLeft;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Instance = null;
    }

    private void Update()
    {
        if (GameManager.Instance.gameStart < 2 || GameManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        waveCounter.text = "第 " + GameManager.Instance.waves.Value.ToString() + " 波";

        if (!IsHost)
        {
            return;
        }

        if (GameManager.Instance.enemyDisable)
        {
            enemyDamage = 30 + GameManager.Instance.waves.Value * 5;
            GameManager.Instance.enemies.Value = GameManager.Instance.waves.Value * 10;
            leftToSpawn = GameManager.Instance.enemies.Value;
            timeLeft = 0;
            return;
        }

        timeLeft -= Time.deltaTime;

        if (leftToSpawn > 0 && timeLeft < 0)
        {
            leftToSpawn--;
            timeLeft = 15f / (GameManager.Instance.waves.Value + 9f) * GameManager.Instance.enemyDelay;
            enemyHealth = Random.Range(1f, 30f + GameManager.Instance.waves.Value * 10);

            Vector3 enemyPosition = Grid.RandomPosition(spawnArea[Random.Range(0, spawnArea.Count)]);
            GameObject enemy = Instantiate(enemyPrefab, enemyPosition, Quaternion.LookRotation(enemyTarget.position - enemyPosition, Vector3.up));
            enemy.GetComponent<Enemy>().target = enemyTarget;
            enemy.GetComponent<Enemy>().health = enemyHealth;
            enemy.GetComponent<Enemy>().damage = enemyDamage;
            enemy.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}
