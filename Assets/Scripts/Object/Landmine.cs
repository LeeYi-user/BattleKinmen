using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Landmine : NetworkBehaviour
{
    [SerializeField] private GameObject explosion;

    public ulong ownerId;
    public float explosionRange;
    public float explosionDamage;

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
                collider.GetComponent<Enemy>().TakeDamage(explosionDamage, ownerId);
            }

            GameObject explosionGO = Instantiate(explosion, transform.position, Quaternion.identity);
            explosionGO.GetComponent<NetworkObject>().Spawn(true);
            Destroy(explosionGO, 2f);
            Destroy(gameObject);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (!IsHost || RelayManager.disconnecting)
        {
            return;
        }

        NetworkManager.SpawnManager.SpawnedObjects[ownerId].GetComponent<Player>().playerWeapon.landmines.Remove(gameObject);
    }
}
