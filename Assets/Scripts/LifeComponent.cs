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
        m_subscriberList.Add(new Event<GetLifeEvent>.LocalSubscriber(GetLife, gameObject));
        m_subscriberList.Add(new Event<HaveLifeEvent>.LocalSubscriber(HaveLife, gameObject));
        m_subscriberList.Add(new Event<IsDeadEvent>.LocalSubscriber(IsDead, gameObject));
        m_subscriberList.Add(new Event<HitEvent>.LocalSubscriber(Hit, gameObject));
        m_subscriberList.Add(new Event<HealEvent>.LocalSubscriber(Heal, gameObject));
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

    void Hit(Hit hit)
    {
        if (m_life <= 0)
            return;

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

    float GetMaxLife()
    {
        return m_maxLife;
    }

    float GetLife()
    {
        return m_life;
    }

    void Heal(float value, bool percent = false)
    {
        if (m_life <= 0)
            return;

        if (percent)
            value *= m_maxLife;

        HealBeforeApplyEvent e = new HealBeforeApplyEvent(value);
        Event<HealBeforeApplyEvent>.Broadcast(e, gameObject);

        m_life += e.heal;
        if (m_life > m_maxLife)
            m_life = m_maxLife;
    }

    void BuildLife(BuildSelectionDetailLifeEvent e)
    {
        UIElementData.Create<UIElementLine>(e.container);
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Health").SetMaxFunc(GetMaxLife).SetValueFunc(GetLife).SetValueDisplayType(UIElementFillValueDisplayType.classic).SetNbDigits(0);
    }

    void GetLife(GetLifeEvent e)
    {
        e.life = m_life;
        e.maxLife = m_maxLife;
    }

    void HaveLife(HaveLifeEvent e)
    {
        e.haveLife = true;
    }

    void IsDead(IsDeadEvent e)
    {
        e.isDead = m_life <= 0;
    }

    void Hit(HitEvent e)
    {
        Hit(e.hit);
    }

    void Heal(HealEvent e)
    {
        Heal(e.value, e.percent);
    }
}
