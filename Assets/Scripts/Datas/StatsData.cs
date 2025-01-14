using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class StatsData
{
    [Serializable]
    class StatInfo
    {
        public StatType type;
        public float initialValue;
    }

    [SerializeField] List<StatInfo> m_initialValues = new List<StatInfo>();

    public float GetInitialValue(StatType type)
    {
        foreach(var s in m_initialValues)
        {
            if (s.type == type)
                return s.initialValue;
        }

        return 0;
    }
}
