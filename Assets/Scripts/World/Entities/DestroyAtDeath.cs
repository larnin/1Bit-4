using UnityEngine;
using System.Collections;

public class DestroyAtDeath : MonoBehaviour
{
    [SerializeField] GameObject m_effectPrefab;
    [SerializeField] float m_effectDelay = 0.0f;
    [SerializeField] float m_destroyDelay = 0.5f;

    SubscriberList m_subscriberList = new SubscriberList();

    bool m_dead = false;
    float m_deadTimer = 0;

    private void Awake()
    {
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        if(m_dead)
        {
            float nextTimer = m_deadTimer + Time.deltaTime;
            if(nextTimer >= m_effectDelay && m_deadTimer < m_effectDelay && m_effectPrefab != null)
            {
                var obj = Instantiate(m_effectPrefab);
                obj.transform.position = transform.position;
                obj.transform.rotation = transform.rotation;
            }

            if (nextTimer >= m_destroyDelay && m_destroyDelay < m_effectDelay)
                Destroy(gameObject);

            m_deadTimer = nextTimer;
        }
    }

    void OnDeath(DeathEvent e)
    {
        m_dead = true;
        m_deadTimer = 0;
    }
}
