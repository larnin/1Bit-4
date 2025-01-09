using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected float m_damages = 1;
    [SerializeField] protected DamageType m_damageType = DamageType.Normal;
    [SerializeField] protected float m_damageEffect = 1;

    protected GameObject m_target;
    protected GameObject m_caster;
    protected float m_damagesMultiplier = 1;

    public void SetTarget(GameObject target)
    {
        m_target = target;
    }

    public void SetCaster(GameObject caster)
    {
        m_caster = caster;
    }

    public void SetDamagesMultiplier(float multiplier)
    {
        m_damagesMultiplier = multiplier;
    }
}
