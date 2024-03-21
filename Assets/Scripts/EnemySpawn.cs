﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawn : NetworkBehaviour
{
    public static EnemySpawn Instance;

    [SerializeField] private GameObject enemyPrefab;

    public NetworkVariable<int> waves = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> enemies = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float enemyHealth;
    public float enemyDelay;
    public float enemyDamage;
    private int leftToSpawn;
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

        if (MainSceneManager.Instance.start < 2 || MainSceneManager.Instance.breakTime.Value > 0 || MainSceneManager.Instance.gameover || MainSceneManager.disconnecting)
        {
            enemyDamage = 26 + waves.Value * 4;
            enemies.Value = waves.Value * 10;
            leftToSpawn = enemies.Value;
            timeLeft = 0;
            return;
        }

        timeLeft -= Time.deltaTime;

        if (leftToSpawn > 0 && timeLeft < 0)
        {
            leftToSpawn--;
            timeLeft = 15f / (waves.Value + 9f) * enemyDelay;
            enemyHealth = Random.Range(1f, 30f + waves.Value * 5f);

            GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-100f, 100f)), Quaternion.Euler(0, -90, 0));
            enemy.GetComponent<NetworkObject>().Spawn(true);
        }

        if (enemies.Value <= 0)
        {
            MainSceneManager.Instance.breakTime.Value = 21f;
            waves.Value++;
        }
    }
}
