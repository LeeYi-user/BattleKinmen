using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] private TextMeshProUGUI waveCounter;

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
        if (!GameManager.Instance.gameStart || GameManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        waveCounter.text = "第 " + GameManager.Instance.waves.Value.ToString() + " 波";

        if (!IsHost)
        {
            return;
        }

        if (GameManager.Instance.enemyDisable.Value)
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

            Vector3 enemyPosition = Grid.RandomPosition(GameManager.Instance.enemySpawnArea[Random.Range(0, GameManager.Instance.enemySpawnArea.Count)]);
            GameObject enemy = Instantiate(GameManager.Instance.enemy, enemyPosition, Quaternion.LookRotation(GameManager.Instance.enemyTarget.position - enemyPosition, Vector3.up));
            enemy.GetComponent<Enemy>().target = GameManager.Instance.enemyTarget;
            enemy.GetComponent<Enemy>().health = enemyHealth;
            enemy.GetComponent<Enemy>().damage = enemyDamage;
            enemy.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}
