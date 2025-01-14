using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum StatType
{
    MaxLifeMultiplier,
}


public class Stats : MonoBehaviour
{
    [Serializable]
    class StatInfo
    {
        public StatType type;
        public float initialValue;
    }

    [SerializeField] List<StatInfo> m_initialValues = new List<StatInfo>();

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
        foreach(var s in m_initialValues)
        {
            if(s.type == e.type)
            {
                e.set = s.initialValue;
                return;
            }
        }

        e.set = Global.instance.statsDatas.GetInitialValue(e.type);
    }
}
