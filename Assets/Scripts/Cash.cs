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

        if (MainSceneManager.Instance.gameover)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        body.SetActive(false);

        if (!IsHost)
        {
            return;
        }

        if (other.transform.CompareTag("Player"))
        {
            MainSceneManager.Instance.teamCash.Value += 100;
            Destroy(gameObject);
        }
    }
}
