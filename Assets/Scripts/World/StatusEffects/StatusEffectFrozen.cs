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

    public override void OnDestroy()
    {

    }

    public override void Start(float power)
    {
        if (m_duration > 0)
            return;

        m_duration = Global.instance.statusDatas.frozen.powerToDuration * power;

        var icon = Global.instance.statusDatas.frozen.icon;
        if (icon != null && icon != "" && DisplayIcons.instance != null)
        {
            var pos = m_owner.transform.position;
            DisplayIcons.instance.Register(pos, 1, 1, icon);
        }
    }

    public override void Update()
    {
        m_duration -= Time.deltaTime;
    }
}

