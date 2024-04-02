using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private TextMeshProUGUI waveCounter;

    public NetworkVariable<int> waves = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> enemies = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public List<Transform> spawnArea;
    public Transform enemyTarget;
    public float enemyHealth;
    public float enemyDelay;
    public float enemyDamage;

    private int leftToSpawn;
    private float timeLeft;

    public bool disable = true;

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
        if (PlayerManager.Instance.gameStart < 2 || PlayerManager.gameOver || RelayManager.disconnecting)
        {
            return;
        }

        waveCounter.text = "第 " + waves.Value.ToString() + " 波";

        if (!IsHost)
        {
            return;
        }

        if (disable)
        {
            enemyDamage = 30 + waves.Value * 5;
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
            enemyHealth = Random.Range(1f, 30f + waves.Value * 10);

            Vector3 enemyPosition = Grid.RandomPosition(spawnArea[Random.Range(0, spawnArea.Count)]);
            GameObject enemy = Instantiate(enemyPrefab, enemyPosition, Quaternion.LookRotation(enemyTarget.position - enemyPosition, Vector3.up));
            enemy.GetComponent<NetworkObject>().Spawn(true);
        }
    }
}
