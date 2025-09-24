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
        m_subscriberList.Add(new Event<EditorSystemButtonClickedEvent>.Subscriber(OnEditorSystemButtonClick));
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
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.System));
    }

    public void OnEntitiesClick()
    {
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.Entities));
    }

    public void OnTerraformationClick()
    {
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.Terraformation));
    }

    public void OnGenerationClick()
    {
        Event<ToggleEditorToolCategoryEvent>.Broadcast(new ToggleEditorToolCategoryEvent(EditorToolCategoryType.Generation));
    }

    void OnEditorSystemButtonClick(EditorSystemButtonClickedEvent e)
    {
        switch(e.button)
        {
            case EditorSystemButtonType.New:

                break;
            case EditorSystemButtonType.Load:

                break;
            case EditorSystemButtonType.Save:

                break;
            case EditorSystemButtonType.SaveAs:

                break;
            case EditorSystemButtonType.Undo:

                break;
            case EditorSystemButtonType.Redo:

                break;
            case EditorSystemButtonType.Exit:

                break;
            default:
                break;
        }
    }
}
