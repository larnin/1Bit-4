using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class StatInfo
{
    public StatType type;
    public float initialValue;

    [HideLabel]
    [HorizontalGroup("Min", Width = 30)]
    public bool haveMin;

    [HorizontalGroup("Min")]
    [EnableIf("haveMin")]
    public float minValue;

    [HideLabel]
    [HorizontalGroup("Max", Width = 30)]
    public bool haveMax;

    [HorizontalGroup("Max")]
    [EnableIf("haveMax")]
    public float maxValue;
}

[Serializable]
public class StatsData
{
    [SerializeField] List<StatInfo> m_statInfos = new List<StatInfo>();

    public StatInfo GetStatInfos(StatType type)
    {
        foreach(var s in m_statInfos)
        {
            if (s.type == type)
                return s;
        }
        return null;
    }
}
