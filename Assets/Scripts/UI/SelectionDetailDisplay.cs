using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SelectionDetailDisplay : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();
    GameObject m_target;
    UIElementContainer m_container;

    private void Awake()
    {
        m_subscriberList.Add(new Event<SetHoveredObjectEvent>.Subscriber(OnHover));
        m_subscriberList.Subscribe();
        m_container = GetComponent<UIElementContainer>();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    void OnHover(SetHoveredObjectEvent e)
    {
        if (m_target == e.hoveredObject)
            return;

        if (e.hoveredObject == null)
            return;
        
        m_target = e.hoveredObject;
        OnTargetChange();
    }

    void OnTargetChange()
    {
        if (m_container == null)
            return;

        m_container.RemoveAndDestroyAll();
        
        if (m_target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Event<BuildSelectionDetailCommonEvent>.Broadcast(new BuildSelectionDetailCommonEvent(m_container), m_target);
        Event<BuildSelectionDetailLifeEvent>.Broadcast(new BuildSelectionDetailLifeEvent(m_container), m_target);
        Event<BuildSelectionDetailStatusEvent>.Broadcast(new BuildSelectionDetailStatusEvent(m_container), m_target);

        gameObject.SetActive(m_container.GetElementNb() > 0);
    }

    private void Update()
    {
        if(m_target == null)
        {
            OnTargetChange();
            return;
        }
    }
}

