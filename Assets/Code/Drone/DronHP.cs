using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronHP : HitCollider
{
    public float m_Damage;
    public override void Hit(float damage)
    {
        m_DroneEnemy.GetComponent<DroneEnemy>().Hit(m_Damage);
    }
}
