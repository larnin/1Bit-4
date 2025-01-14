using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class GetStatEvent
{
    public StatType type;
    public float set = 0;
    public float add = 0;
    public float mul = 1;

    public GetStatEvent(StatType _type)
    {
        type = _type;
    }

    public float GetValue()
    {
        return (set + add) * mul;
    }
}