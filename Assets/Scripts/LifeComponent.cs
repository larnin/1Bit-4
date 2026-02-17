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
    float m_maxLifeMultiplier = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<BuildSelectionDetailLifeEvent>.LocalSubscriber(BuildLife, gameObject));
        m_subscriberList.Add(new Event<GetLifeEvent>.LocalSubscriber(GetLife, gameObject));
        m_subscriberList.Add(new Event<IsDeadEvent>.LocalSubscriber(IsDead, gameObject));
        m_subscriberList.Add(new Event<HitEvent>.LocalSubscriber(Hit, gameObject));
        m_subscriberList.Add(new Event<HealEvent>.LocalSubscriber(Heal, gameObject));
        m_subscriberList.Add(new Event<LoadEvent>.LocalSubscriber(Load, gameObject));
        m_subscriberList.Add(new Event<SaveEvent>.LocalSubscriber(Save, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        UpdateMultiplier();

        m_life = m_maxLife * m_maxLifeMultiplier;
    }

    void Hit(Hit hit)
    {
        if (m_life <= 0)
            return;

        Event<HitBeforeApplyEvent>.Broadcast(new HitBeforeApplyEvent(hit), gameObject);

        var casterTeam = new GetTeamEvent();
        if(hit.caster != null)
            Event<GetTeamEvent>.Broadcast(casterTeam, hit.caster);

        var targetTeam = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), gameObject);

        if (targetTeam.team == Team.Neutral)
            return;

        if (casterTeam.team == targetTeam.team)
            return;

        UpdateMultiplier();

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
        UpdateMultiplier();

        return m_maxLife * m_maxLifeMultiplier;
    }

    public float GetLife()
    {
        UpdateMultiplier();
        return m_life;
    }

    public float GetLifePercent()
    {
        UpdateMultiplier();

        return m_life / (m_maxLife * m_maxLifeMultiplier);
    }

    void Heal(float value, bool percent = false)
    {
        if (m_life <= 0)
            return;

        if (percent)
            value *= m_maxLife * m_maxLifeMultiplier;

        var heal = Event<HealBeforeApplyEvent>.Broadcast(new HealBeforeApplyEvent(value), gameObject);

        UpdateMultiplier();

        m_life += heal.heal;
        if (m_life > m_maxLife * m_maxLifeMultiplier)
            m_life = m_maxLife * m_maxLifeMultiplier;
    }

    void BuildLife(BuildSelectionDetailLifeEvent e)
    {
        UIElementData.Create<UIElementLine>(e.container);
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Health").SetMaxFunc(GetMaxLife).SetValueFunc(GetLife).SetValueDisplayType(UIElementFillValueDisplayType.classic).SetNbDigits(0);
    }

    void GetLife(GetLifeEvent e)
    {
        UpdateMultiplier();

        e.haveLife = true;
        e.life = m_life;
        e.maxLife = m_maxLife * m_maxLifeMultiplier;
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

    void UpdateMultiplier()
    {
        var stat = new GetStatEvent(StatType.MaxLifeMultiplier);
        stat.set = 1;
        Event<GetStatEvent>.Broadcast(stat, gameObject);

        float multiplier = stat.GetValue();
        if(multiplier != m_maxLifeMultiplier)
        {
            m_life = m_life * multiplier / m_maxLifeMultiplier;
            m_maxLifeMultiplier = multiplier;
        }
    }

    void Load(LoadEvent e)
    {
        var jsonObj = e.obj.GetElement("life");
        if(jsonObj != null && jsonObj.IsJsonObject())
        {
            var obj = jsonObj.JsonObject();

            var jsonValue = obj.GetElement("value");
            if (jsonValue != null && jsonValue.IsJsonNumber())
                m_life = jsonValue.Float();

            var jsonMul = obj.GetElement("mul");
            if (jsonMul != null && jsonMul.IsJsonNumber())
                m_maxLifeMultiplier = jsonMul.Float();
        }
    }

    void Save(SaveEvent e)
    {
        var obj = new JsonObject();
        e.obj.AddElement("life", obj);

        obj.AddElement("value", m_life);
        obj.AddElement("mul", m_maxLifeMultiplier);
    }
}
