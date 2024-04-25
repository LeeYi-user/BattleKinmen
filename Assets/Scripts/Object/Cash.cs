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

        if (GameManager.gameOver)
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

            int cashAmount = (int)(100f * GameManager.Instance.cashBonus);
            GameManager.Instance.teamCash.Value += cashAmount;
            GameManager.Instance.Popup_ClientRpc("拾取資金! (資金 +" + cashAmount.ToString() + ")", Color.white, true, other.transform.parent.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            Destroy(gameObject);
        }
    }
}
