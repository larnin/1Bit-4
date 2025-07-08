using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditorInterface : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<IsMouseOverUIEvent>.Subscriber(IsMouseOverUI));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void IsMouseOverUI(IsMouseOverUIEvent e)
    {
        e.overUI = false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);

        for (int index = 0; index < raysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = raysastResults[index];

            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                e.overUI = true;
                break;
            }
        }
    }

    public void OnSystemClick()
    {
        Event<ToggleToolCategoryEvent>.Broadcast(new ToggleToolCategoryEvent(ToolCategoryType.System));
    }

    public void OnEntitiesClick()
    {
        Event<ToggleToolCategoryEvent>.Broadcast(new ToggleToolCategoryEvent(ToolCategoryType.Entities));
    }

    public void OnTerraformationClick()
    {
        Event<ToggleToolCategoryEvent>.Broadcast(new ToggleToolCategoryEvent(ToolCategoryType.Terraformation));
    }

    public void OnGenerationClick()
    {
        Event<ToggleToolCategoryEvent>.Broadcast(new ToggleToolCategoryEvent(ToolCategoryType.Generation));
    }
}
