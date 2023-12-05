using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemySpawn : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private int counter;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (MainScene.start && IsHost && counter < 10)
        {
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(5f, 0f, 5f), Quaternion.identity);
            enemy.GetComponent<NetworkObject>().Spawn(true);
            counter++;
        }
    }
}
