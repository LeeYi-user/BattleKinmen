using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawn : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    [SerializeField] private float slope; // 1
    [SerializeField] private float timeLimit; // 300
    [SerializeField] private float enemyLimit; // 100

    private float startTime;
    public static int enemyCounter;

    private void Start()
    {
        startTime = 0;
        enemyCounter = 0;
    }

    private void Update()
    {
        if (!IsHost || !MainSceneManager.start || MainSceneManager.gameover)
        {
            return;
        }

        if (startTime == 0)
        {
            startTime = Time.time;
        }

        if (enemyCounter < Sigmoid(Time.time - startTime))
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-100f, 100f)), Quaternion.Euler(0, -90, 0));
            enemy.GetComponent<NetworkObject>().Spawn(true);
            enemyCounter++;
        }
    }

    private float Sigmoid(float t)
    {
        return enemyLimit / (1f + Mathf.Exp(-slope / (timeLimit / 10f) * (t - (timeLimit / 2f))));
    }
}
