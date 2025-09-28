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

    public abstract StatusType GetStatusType();

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

    public static StatusEffectBase Create(JsonObject obj, GameObject owner)
    {
        StatusType statusType;
        var jsonStatus = obj.GetElement("status");
        if (jsonStatus != null && jsonStatus.IsJsonString())
        {
            if(Enum.TryParse<StatusType>(jsonStatus.ToString(), out statusType))
            {
                var status = Create(statusType, owner);
                if(status != null)
                {
                    status.Load(obj);
                    return status;
                }
            }
        }

        return null;
    }

    public virtual JsonObject Save()
    {
        var obj = new JsonObject();
        obj.AddElement("status", GetStatusType().ToString());

        return obj;
    }

    protected virtual void Load(JsonObject obj)
    {
        //nothing to do here
    }
}
