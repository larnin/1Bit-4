using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class GetLifeEvent
{
    public float life;
    public float maxLife = 1;
    public float lifePercent { get { return life / maxLife; } }
}

public class HaveLifeEvent
{
    public bool haveLife = false;
}

public class IsDeadEvent
{
    public bool isDead = false;
}

public class HitEvent
{
    public Hit hit;
    
    public HitEvent(Hit _hit)
    {
        hit = _hit;
    }
}

public class HealEvent
{
    public float value;
    public bool percent = false;

    public HealEvent(float _value, bool _percent = false)
    {
        value = _value;
        percent = _percent;
    }
}

public class HitBeforeApplyEvent
{
    public Hit hit;

    public HitBeforeApplyEvent(Hit _hit)
    {
        hit = _hit;
    }
}

public class LifeLossEvent
{
    public Hit hit;

    public LifeLossEvent(Hit _hit)
    {
        hit = _hit;
    }
}

public class DeathEvent
{
    public Hit hit;

    public DeathEvent(Hit _hit)
    {
        hit = _hit;
    }
}

public class HealBeforeApplyEvent
{
    public float heal;
    public float initialHeal;

    public HealBeforeApplyEvent(float _heal)
    {
        heal = _heal;
    }
}

public class IsFrozenEvent
{
    public bool frozen = false;
}

