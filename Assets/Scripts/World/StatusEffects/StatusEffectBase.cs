using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class StatusEffectBase
{
    protected GameObject m_owner;

    public StatusEffectBase(GameObject owner)
    {
        m_owner = owner;
    }

    public abstract void Update();

    public abstract bool Ended();

    public abstract void Start(float power);

    public abstract void OnDestroy();

    public static StatusType DamageTypeToStatus(DamageType type)
    {
        switch(type)
        {
            case DamageType.Fire:
                return StatusType.Burning;
            case DamageType.Freeze:
                return StatusType.Frozen;
        }
        return StatusType.None;
    }

    public static StatusEffectBase Create(StatusType type, GameObject owner)
    {
        switch(type)
        {
            case StatusType.Burning:
                return new StatusEffectBurning(owner);
            case StatusType.Frozen:
                return new StatusEffectFrozen(owner);
        }
        return null;
    }
}
