using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Landmine : NetworkBehaviour
{
    [SerializeField] private GameObject explosion;

    public float lifetime;
    public float explosionRange;
    public float explosionDamage;

    private void Start()
    {
        if (!IsHost)
        {
            return;
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost)
        {
            return;
        }

        if (other.transform.CompareTag("Enemy"))
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange, 1 << LayerMask.NameToLayer("Enemy"));

            foreach (Collider collider in colliders)
            {
                collider.GetComponent<Enemy>().TakeDamage(explosionDamage);
            }

            GameObject explosionGO = Instantiate(explosion, transform.position, Quaternion.identity);
            explosionGO.GetComponent<NetworkObject>().Spawn(true);
            Destroy(explosionGO, 2f);
            Destroy(gameObject);
        }
    }
}
