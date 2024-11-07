using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LifeComponent : MonoBehaviour
{
    [SerializeField] float m_maxLife = 1;

    float m_life;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<BuildSelectionDetailLifeEvent>.LocalSubscriber(BuildLife, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        m_life = m_maxLife;
    }

    public void Hit(float dmg, GameObject caster)
    {
        HitBeforeApplyEvent e = new HitBeforeApplyEvent(dmg, caster);
        Event<HitBeforeApplyEvent>.Broadcast(e, gameObject);

        m_life -= e.dmg;

        if (m_life <= 0)
        {
            m_life = 0;
            Event<DeathEvent>.Broadcast(new DeathEvent(), gameObject);
        }
        else Event<LifeLossEvent>.Broadcast(new LifeLossEvent(e.dmg), gameObject);
    }

    public float GetMaxLife()
    {
        return m_maxLife;
    }

    public float GetLife()
    {
        return m_life;
    }

    public float GetLifePercent()
    {
        return m_life / m_maxLife;
    }

    public void Heal(float value)
    {
        HealBeforeApplyEvent e = new HealBeforeApplyEvent(value);
        Event<HealBeforeApplyEvent>.Broadcast(e, gameObject);

        m_life += e.heal;
        if (m_life > m_maxLife)
            m_life = m_maxLife;

        Event<HealEvent>.Broadcast(new HealEvent(e.heal));
    }

    void BuildLife(BuildSelectionDetailLifeEvent e)
    {

    }
}
