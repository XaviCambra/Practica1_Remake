using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyHP : HitCollider
{
    [Header("Animations")]
    public Animator m_Animator;

    public override void Hit(float damage)
    {
        if (m_Life < 0)
            return;

        m_Life -= damage;

        m_Animator.SetTrigger("Hit");
        m_Animator.SetFloat("Vida", m_Life);
    }

    public void SetHP(float hp)
    {
        m_Life = hp;
        m_Animator.SetFloat("Vida", m_Life);
    }

    public bool HasDied()
    {
        return m_Life < 0;
    }
}
