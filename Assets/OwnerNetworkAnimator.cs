using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class OwnerNetworkAnimator : NetworkAnimator
{
    // 元件用途: 同步玩家動畫
    // 元件位置: 包含第三人稱動畫的玩家子物件(player prefab children)之下

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
