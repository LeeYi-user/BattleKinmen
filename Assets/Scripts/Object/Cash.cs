using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Cash : NetworkBehaviour
{
    [SerializeField] private GameObject body;

    private void Start()
    {
        if (!IsHost)
        {
            return;
        }

        Destroy(gameObject, 30f);
    }

    private void Update()
    {
        if (!IsHost)
        {
            return;
        }

        if (PlayerManager.gameOver)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            body.SetActive(false);

            if (!IsHost)
            {
                return;
            }

            ShopManager.Instance.teamCash.Value += (int)(100f * ShopManager.Instance.cashBonus);
            Destroy(gameObject);
        }
    }
}
