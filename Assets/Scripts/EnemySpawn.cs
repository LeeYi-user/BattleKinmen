using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawn : NetworkBehaviour
{
    public static EnemySpawn Instance;

    [SerializeField] private GameObject enemyPrefab;

    public NetworkVariable<int> waves = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> enemies = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private int enemyLeft;
    private float timeLeft;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!IsHost)
        {
            return;
        }

        if (MainSceneManager.Instance.start < 2 || MainSceneManager.Instance.breakTime.Value > 0 || MainSceneManager.Instance.gameover)
        {
            enemies.Value = waves.Value * 15 + 10 - 20;
            enemyLeft = enemies.Value;
            timeLeft = 10f / (waves.Value + 9);
            return;
        }

        timeLeft -= Time.deltaTime;

        if (enemyLeft > 0 && timeLeft < 0)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-100f, 100f)), Quaternion.Euler(0, -90, 0));
            enemy.GetComponent<NetworkObject>().Spawn(true);
            enemyLeft--;
            timeLeft = 10f / (waves.Value + 9);
        }

        if (enemies.Value <= 0)
        {
            MainSceneManager.Instance.breakTime.Value = 60f;
            waves.Value++;
        }
    }
}
