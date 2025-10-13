using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityDeath : MonoBehaviour
{
    [SerializeField] GameObject m_instantiateAtDeathPrefab;

    bool m_isDead = false;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Add(new Event<SaveEvent>.LocalSubscriber(Save, gameObject));
        m_subscriberList.Add(new Event<LoadEvent>.LocalSubscriber(Load, gameObject));
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

        m_isDead = true;

        Destroy(gameObject, 0.05f);

        if(m_instantiateAtDeathPrefab != null)
        {
            var obj = Instantiate(m_instantiateAtDeathPrefab);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
        }
    }

    void Load(LoadEvent e)
    {
        var jsonDead = e.obj.GetElement("isDead");
        if (jsonDead != null && jsonDead.IsJsonNumber())
            m_isDead = jsonDead.Int() != 0 ? true : false;

        if (m_isDead)
            Destroy(gameObject);
    }

    void Save(SaveEvent e)
    {
        e.obj.AddElement("isDead", m_isDead ? 1 : 0);
    }
}
