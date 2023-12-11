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
        if (!IsHost)
        {
            return;
        }

        if (MainScene.start && startTime == 0)
        {
            startTime = Time.time;
        }

        if (MainScene.start && counter < Sigmoid(Time.time - startTime))
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-100f, 100f)), Quaternion.Euler(0, -90, 0));
            enemy.GetComponent<NetworkObject>().Spawn(true);
            counter++;
        }
    }

    private int Sigmoid(float t)
    {
        return (int)(enemyLimit * (1.0f / (1.0f + Mathf.Exp((-1.0f / (timeLimit / 10.0f)) * (t - (timeLimit / 2.0f))))));
    }
}
