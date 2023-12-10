using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawn : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private int counter;

    // Update is called once per frame
    void Update()
    {
        if (!IsHost)
        {
            return;
        }

        if (MainScene.start && counter < 10)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f)), Quaternion.Euler(0, -90, 0));
            enemy.GetComponent<NetworkObject>().Spawn(true);
            counter++;
        }
    }
}
