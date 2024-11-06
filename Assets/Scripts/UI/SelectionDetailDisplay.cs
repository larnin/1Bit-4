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

        var builder = m_target.GetComponent<I_UIElementBuilder>();
        if(builder == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        builder.Build(m_container);
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

