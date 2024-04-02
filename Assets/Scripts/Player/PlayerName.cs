using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class PlayerName : NetworkBehaviour
{
    public new NetworkVariable<FixedString32Bytes> name = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public TextMeshProUGUI nameText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            return;
        }

        name.OnValueChanged += ChangeName;
    }

    private void ChangeName(FixedString32Bytes previous, FixedString32Bytes current)
    {
        nameText.text = name.Value.ToString();
    }

    public void Despawn()
    {
        name.Value = "";
    }

    public void Respawn()
    {
        name.Value = MenuManager.playerName;
    }
}
