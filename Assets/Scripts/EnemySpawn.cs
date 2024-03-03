using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawn : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    public static int waves;

    public static NetworkVariable<int> enemies = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int enemyLeft;
    private float timeLeft;

    private void Start()
    {
        waves = 1;
    }

    private void Update()
    {
        if (!IsHost || MainSceneManager.start < 2 || MainSceneManager.breakTime > 0 || MainSceneManager.gameover)
        {
            enemies.Value = waves * 15 + 10 - 20;
            enemyLeft = enemies.Value;
            timeLeft = 10f / (waves + 9);
            return;
        }

        timeLeft -= Time.deltaTime;

        if (enemyLeft > 0 && timeLeft < 0)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-100f, 100f)), Quaternion.Euler(0, -90, 0));
            enemy.GetComponent<NetworkObject>().Spawn(true);
            enemyLeft--;
            timeLeft = 10f / (waves + 9);
        }

        if (enemies.Value <= 0)
        {
            MainSceneManager.breakTime = 60f;
            waves++;
        }
    }
}
