using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class GetStatEvent
{
    public StatType type;
    public float value;

    public GetStatEvent(StatType _type)
    {
        type = _type;
    }

    public float GetValue()
    {
        var infos = Global.instance.statsDatas.GetStatInfos(type);
        if(infos == null)
            return value;

        float realValue = value + infos.initialValue;
        if (infos.haveMin && realValue < infos.minValue)
            realValue = infos.minValue;

        if (infos.haveMax && realValue > infos.maxValue)
            realValue = infos.maxValue;

        return realValue;
    }
}