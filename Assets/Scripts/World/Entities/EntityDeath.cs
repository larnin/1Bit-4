using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityDeath : MonoBehaviour
{
    [SerializeField] GameObject m_instantiateAtDeathPrefab;

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

        Destroy(gameObject, 0.05f);

        if(m_instantiateAtDeathPrefab != null)
        {
            var obj = Instantiate(m_instantiateAtDeathPrefab);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
        }
    }
}
