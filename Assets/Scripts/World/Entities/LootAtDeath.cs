using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LootAtDeath : MonoBehaviour
{
    [SerializeField] List<OneResourceCost> m_loots;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnDeath(DeathEvent e)
    {
        if (ResourceSystem.instance == null)
            return;

        foreach(var l in m_loots)
        {
            ResourceSystem.instance.AddResource(l.type, l.count);
        }
    }
}
