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

    class LocalStat
    {
        public StatType type;
        public float value;
        public string id;
    }

    [SerializeField] List<StatInfo> m_values = new List<StatInfo>();

    List<LocalStat> m_localStats = new List<LocalStat>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetStatEvent>.LocalSubscriber(GetStat, gameObject));
        m_subscriberList.Add(new Event<AddStatEvent>.LocalSubscriber(AddStat, gameObject));
        m_subscriberList.Add(new Event<RemoveStatEvent>.LocalSubscriber(RemoveStat, gameObject));
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

        foreach(var v in m_localStats)
        {
            if (v.type == e.type)
                e.value += v.value;
        }
    }

    void AddStat(AddStatEvent e)
    {
        foreach (var v in m_localStats)
        {
            if (v.type == e.type && v.id == e.id)
            {
                v.value += e.value;
                return;
            }
        }

        var stat = new LocalStat();
        stat.id = e.id;
        stat.type = e.type;
        stat.value = e.value;
        m_localStats.Add(stat);
    }

    void RemoveStat(RemoveStatEvent e)
    {
        m_localStats.RemoveAll(x => { return x.id == e.id && x.type == e.type; });
    }

    public static List<StatType> GetProjectileStats()
    {
        return new List<StatType>
        {
            StatType.DamagesMultiplier,
        };
    }
}
