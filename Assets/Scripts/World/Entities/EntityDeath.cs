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
        m_subscriberList.Add(new Event<SaveLevelEvent>.LocalSubscriber(Save, gameObject));
        m_subscriberList.Add(new Event<LoadLevelEvent>.LocalSubscriber(Load, gameObject));
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

    void Load(LoadLevelEvent e)
    {
        var jsonDead = e.obj.GetElement("isDead");
        if (jsonDead != null && jsonDead.IsJsonNumber())
            m_isDead = jsonDead.Bool();

        if (m_isDead)
            Destroy(gameObject);
    }

    void Save(SaveLevelEvent e)
    {
        e.obj.AddElement("isDead", m_isDead);
    }
}
