using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

public class HealEvent
{
    public float heal;

    public HealEvent(float _heal)
    {
        heal = _heal;
    }
}

public class IsFrozenEvent
{
    public bool frozen = false;
}
