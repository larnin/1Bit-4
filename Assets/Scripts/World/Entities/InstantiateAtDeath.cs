using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InstantiateAtDeath : MonoBehaviour
{
    [SerializeField] GameObject m_instantiateAtDeathPrefab;
    [SerializeField] ProjectileChoice m_instantiateAtDeathProjectile;

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

        if (m_instantiateAtDeathPrefab != null)
        {
            var obj = Instantiate(m_instantiateAtDeathPrefab);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
        }

        if(m_instantiateAtDeathProjectile.IsValid())
        {
            ProjectileStartInfos startInfos = new ProjectileStartInfos();
            startInfos.name = m_instantiateAtDeathProjectile.GetValue();
            startInfos.caster = gameObject;
            startInfos.position = transform.position;
            startInfos.rotation = transform.rotation;

            ProjectileBase.ThrowProjectile(startInfos);
        }
    }
}
