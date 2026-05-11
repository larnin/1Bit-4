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

public class AddStatEvent
{
    public StatType type;
    public float value;
    public string id;

    public AddStatEvent(StatType _type, float _value, string _id)
    {
        type = _type;
        value = _value;
        id = _id;
    }
}

public class RemoveStatEvent
{
    public StatType type;
    public string id;

    public RemoveStatEvent(StatType _type, string _id)
    {
        type = _type;
        id = _id;
    }
}