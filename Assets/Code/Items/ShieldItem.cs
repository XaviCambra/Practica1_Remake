using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldItem : Item
{
    public float m_Shield;

    public override void Pick(FPPlayerController Player)
    {
        if (Player.GetShield() < 1.0f)
        {
            Player.AddShield(m_Shield);
            gameObject.SetActive(false);
        }
    }
}
