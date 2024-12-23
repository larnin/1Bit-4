using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StatusEffectBurning : StatusEffectBase
{
    float m_duration;
    float m_power;

    LifeComponent m_lifeComponent;
    GameObject m_effectVisual;

    public StatusEffectBurning(GameObject owner) : base(owner)
    {
        m_lifeComponent = owner.GetComponent<LifeComponent>();
    }

    public override bool Ended()
    {
        return m_duration > 0;
    }

    public override void OnDestroy()
    {
        RemoveEffect();
    }

    public override void Start(float power)
    {
        float duration = Global.instance.statusDatas.burning.powerToDuration * power;
        bool needToStart = false;
        if (duration > m_duration)
        {
            m_duration = duration;
            needToStart = true;
        }
        if (power > m_power)
        {
            m_power = power;
            needToStart = true;
        }

        if (needToStart)
            StartEffect();
    }

    public override void Update()
    {
        if(m_lifeComponent != null)
        {
            float dmg = Global.instance.statusDatas.burning.powerToDot * m_power * Time.deltaTime;
            m_lifeComponent.Hit(new Hit(dmg, m_owner, DamageType.Effect));
        }

        m_duration -= Time.deltaTime;
        if (m_duration <= 0)
            RemoveEffect();
    }

    void StartEffect()
    {
        RemoveEffect();

        var prefab = Global.instance.statusDatas.burning.effectPrefab;
        if(prefab != null)
        {
            var instance = GameObject.Instantiate(prefab);
            instance.transform.parent = m_owner.transform;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * GetScale();
        }

        var icon = Global.instance.statusDatas.burning.icon;
        if(icon != null && icon != "" && DisplayIcons.instance != null)
        {
            var pos = m_owner.transform.position;
            DisplayIcons.instance.Register(pos, 1, 1, icon);
        }
    }

    void RemoveEffect()
    {
        if (m_effectVisual != null)
            GameObject.Destroy(m_effectVisual);
    }

    float GetScale()
    {
        var type = GameSystem.GetEntityType(m_owner);
        if (type != EntityType.Building)
            return 1;

        var building = m_owner.GetComponent<BuildingBase>();
        if (building == null)
            return 1;

        var size = building.GetSize();

        return Mathf.Max(size.x, size.z);
    }
}
