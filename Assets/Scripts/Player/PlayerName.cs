using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerName : NetworkBehaviour
{
    public new NetworkVariable<FixedString32Bytes> name = new NetworkVariable<FixedString32Bytes>("連接中", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> nameDisplay = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public TextMeshProUGUI nameText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            return;
        }

        name.Value = MenuManager.playerName;
    }

    private void Update()
    {
        if (IsOwner)
        {
            return;
        }

        nameText.text = name.Value.ToString();
        nameText.enabled = nameDisplay.Value;
    }

    public void Despawn()
    {
        nameDisplay.Value = false;
    }

    public void Respawn()
    {
        nameDisplay.Value = true;
    }
}
