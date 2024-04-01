using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public static Vector3 RandomPosition(Transform grid)
    {
        float x = grid.position.x + Random.Range(-grid.localScale.x, grid.localScale.x) / 2;
        float y = grid.position.y + Random.Range(-grid.localScale.y, grid.localScale.y) / 2;
        float z = grid.position.z + Random.Range(-grid.localScale.z, grid.localScale.z) / 2;

        return new Vector3(x, y, z);
    }

    public static bool CheckPosition(Vector3 position, Transform grid)
    {
        if (Mathf.Abs(position.x - grid.position.x) <= grid.localScale.x / 2 &&
            Mathf.Abs(position.y - grid.position.y) <= grid.localScale.y / 2 &&
            Mathf.Abs(position.z - grid.position.z) <= grid.localScale.z / 2)
        {
            return true;
        }

        return false;
    }
}
