using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Hit
{
    public float initialDamages;
    public float damages;
    public GameObject caster;
    public DamageType damageType;
    public float damageEffectPower;

    public Hit(float _damages, GameObject _caster = null, DamageType _type = DamageType.Normal, float _effect = 1)
    {
        initialDamages = _damages;
        damages = _damages;
        caster = _caster;
        damageType = _type;
        damageEffectPower = _effect;
    }
}

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

    public void Hit(Hit hit)
    {
        HitBeforeApplyEvent e = new HitBeforeApplyEvent(hit);
        Event<HitBeforeApplyEvent>.Broadcast(e, gameObject);

        GetTeamEvent casterTeam = new GetTeamEvent();
        if(hit.caster != null)
            Event<GetTeamEvent>.Broadcast(casterTeam, hit.caster);

        GetTeamEvent targetTeam = new GetTeamEvent();
        Event<GetTeamEvent>.Broadcast(targetTeam, gameObject);

        if (targetTeam.team == Team.Neutral)
            return;

        if (casterTeam.team == targetTeam.team)
            return;

        m_life -= hit.damages;

        if (m_life <= 0)
        {
            m_life = 0;
            Event<DeathEvent>.Broadcast(new DeathEvent(hit), gameObject);
        }
        else Event<LifeLossEvent>.Broadcast(new LifeLossEvent(hit), gameObject);
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
        UIElementData.Create<UIElementLine>(e.container);
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Health").SetMaxFunc(GetMaxLife).SetValueFunc(GetLife).SetValueDisplayType(UIElementFillValueDisplayType.classic).SetNbDigits(0);
    }
}
