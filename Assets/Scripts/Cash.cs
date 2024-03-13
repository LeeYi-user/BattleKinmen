using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Cash : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost)
        {
            return;
        }

        if (other.transform.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
