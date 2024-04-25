using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Grenade : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float gravity;
    [SerializeField] private GameObject explosion;

    public Vector3 forceAxis;
    public Vector3 rotateAxis;

    public ulong ownerId;
    public float force;
    public float explosionRange;
    public float explosionDamage;

    void Start()
    {
        if (!IsHost)
        {
            return;
        }

        rb.AddForce(forceAxis * force, ForceMode.Force);
    }

    void FixedUpdate()
    {
        if (!IsHost)
        {
            return;
        }

        rb.velocity += new Vector3(0, gravity * Time.fixedDeltaTime, 0);
        Quaternion q = Quaternion.AngleAxis(rb.velocity.magnitude, rotateAxis);
        rb.MoveRotation(rb.transform.rotation * q);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsHost)
        {
            return;
        }

        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
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
}
