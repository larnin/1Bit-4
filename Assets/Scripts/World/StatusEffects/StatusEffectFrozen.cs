using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StatusEffectFrozen : StatusEffectBase
{
    float m_duration;

    public StatusEffectFrozen(GameObject owner) : base(owner)
    {

    }

    public override bool Ended()
    {
        return m_duration < 0;
    }

    public override StatusType GetStatusType()
    {
        return StatusType.Frozen;
    }

    public override void OnDestroy()
    {

    }

    public override void Start(float power)
    {
        if (m_duration > 0)
            return;

        m_duration = Global.instance.statusDatas.frozen.powerToDuration * power;

        var icon = Global.instance.statusDatas.frozen.icon;
        if (icon != null && icon != "" && DisplayIconsV2.instance != null)
        {
            var pos = m_owner.transform.position;
            DisplayIconsV2.instance.Register(pos, 1, 1, icon);
        }
    }

    public override void Update()
    {
        m_duration -= Time.deltaTime;
    }

    protected override void Load(JsonObject obj)
    {
        base.Load(obj);

        var jsonDuration = obj.GetElement("duration");
        if (jsonDuration != null && jsonDuration.IsJsonNumber())
            m_duration = jsonDuration.Float();
    }

    public override JsonObject Save()
    {
        var obj = base.Save();

        obj.AddElement("duration", m_duration);

        return obj;
    }
}

