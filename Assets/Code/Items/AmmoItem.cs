using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoItem : Item
{
    public float m_Ammo;

    public override void Pick(FPPlayerController Player)
    {
        Player.AddAmmo(m_Ammo);
        gameObject.SetActive(false);

    }
}