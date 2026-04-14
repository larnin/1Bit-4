using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum StatType
{
    MaxLifeMultiplier,
    DamagesMultiplier,
}


public class Stats : MonoBehaviour
{
    [Serializable]
    class StatInfo
    {
        public StatType type;
        public float value;
    }

    [SerializeField] List<StatInfo> m_values = new List<StatInfo>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetStatEvent>.LocalSubscriber(GetStat, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void GetStat(GetStatEvent e)
    {
        foreach(var v in m_values)
        {
            if (v.type == e.type)
                e.value += v.value;
        }
    }

    public static List<StatType> GetProjectileStats()
    {
        return new List<StatType>
        {
            StatType.DamagesMultiplier,
        };
    }
}
