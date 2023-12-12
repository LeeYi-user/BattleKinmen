using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawn : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float enemyLimit; // 100
    [SerializeField] private float timeLimit; // 300

    public static int counter;
    private float startTime;

    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
        startTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsHost || !MainScene.start || MainScene.gameover)
        {
            return;
        }

        if (startTime == 0)
        {
            startTime = Time.time;
        }

        if (counter < Sigmoid(Time.time - startTime))
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-100f, 100f)), Quaternion.Euler(0, -90, 0));
            enemy.GetComponent<NetworkObject>().Spawn(true);
            counter++;
        }
    }

    private float Sigmoid(float t)
    {
        return enemyLimit / (1f + Mathf.Exp((5f * timeLimit - 10f * t) / timeLimit));
    }
}
