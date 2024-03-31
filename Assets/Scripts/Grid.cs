using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z));
    }
}
