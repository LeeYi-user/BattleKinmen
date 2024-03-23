using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Landmine : NetworkBehaviour
{
    [SerializeField] private GameObject explosion;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost)
        {
            return;
        }

        if (other.transform.CompareTag("Enemy"))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 6f, 1 << LayerMask.NameToLayer("Enemy"));

            foreach (Collider collider in colliders)
            {
                collider.GetComponent<Enemy>().TakeDamage(100f);
            }

            GameObject explosionGO = Instantiate(explosion, transform.position, Quaternion.identity);
            explosionGO.GetComponent<NetworkObject>().Spawn(true);
            Destroy(explosionGO, 2f);
            Destroy(gameObject);
        }
    }
}
