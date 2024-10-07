using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HitBeforeApplyEvent
{
    public float dmg;
    public float initialDmg;
    public GameObject caster;

    public HitBeforeApplyEvent(float _dmg, GameObject _caster)
    {
        dmg = _dmg;
        initialDmg = _dmg;
        caster = _caster;
    }
}

public class LifeLossEvent
{
    public float dmg;

    public LifeLossEvent(float _dmg)
    {
        dmg = _dmg;
    }
}

public class DeathEvent
{

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
