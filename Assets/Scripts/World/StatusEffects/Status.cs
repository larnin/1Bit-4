using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum DamageType
{
    Normal,
    Effect,
    Freeze,
    Fire,
}

public enum StatusType
{
    None,
    Frozen,
    Burning,
}

public class Status : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    Dictionary<StatusType, StatusEffectBase> m_effects = new Dictionary<StatusType, StatusEffectBase>();

    private void Awake()
    {
        m_subscriberList.Add(new Event<LifeLossEvent>.LocalSubscriber(OnHit, gameObject));
        m_subscriberList.Add(new Event<IsFrozenEvent>.LocalSubscriber(IsFrozen, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnHit(LifeLossEvent e)
    {
        if (e.hit.damageType == DamageType.Effect)
            return;

        var effectType = StatusEffectBase.DamageTypeToStatus(e.hit.damageType);
        if (effectType == StatusType.None)
            return;

        StatusEffectBase current;
        m_effects.TryGetValue(effectType, out current);
        if(current == null)
        {
            current = StatusEffectBase.Create(effectType, gameObject);
            if (current == null)
                return;
            m_effects.Add(effectType, current);
        }

        current.Start(e.hit.damageEffectPower);
    }

    void IsFrozen(IsFrozenEvent e)
    {
        e.frozen = HaveEffectActive(StatusType.Frozen);
    }

    bool HaveEffectActive(StatusType type)
    {
        StatusEffectBase current;
        m_effects.TryGetValue(type, out current);

        if (current == null)
            return false;

        return !current.Ended();
    }

    private void Update()
    {
        List<StatusType> toRemove = new List<StatusType>();

        foreach(var effect in m_effects)
        {
            if (effect.Value.Ended())
            {
                effect.Value.OnDestroy();
                toRemove.Add(effect.Key);
            }
            else effect.Value.Update();
        }

        foreach (var s in toRemove)
            m_effects.Remove(s);
    }
}
