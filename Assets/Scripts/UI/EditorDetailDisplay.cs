using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum EditorToolCategoryType
{
    None,
    System,
    Entities,
    Terraformation,
    Generation,
}

public enum EditorSystemButtonType
{
    New,
    Load,
    Save,
    SaveAs,
    Undo,
    Redo,
    Exit,
}

public class EditorDetailDisplay : MonoBehaviour
{
    UIElementContainer m_container;

    SubscriberList m_subscriberList = new SubscriberList();

    EditorToolCategoryType m_currentCategory = EditorToolCategoryType.None;

    private void Awake()
    {
        m_subscriberList.Add(new Event<ToggleEditorToolCategoryEvent>.Subscriber(ToogleToolCategory));
        m_subscriberList.Subscribe();

        m_container = GetComponent<UIElementContainer>();
    }

    private void Start()
    {
        Clean();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void ToogleToolCategory(ToggleEditorToolCategoryEvent e)
    {
        if (m_currentCategory == e.category)
            m_currentCategory = EditorToolCategoryType.None;
        else m_currentCategory = e.category;

        switch(m_currentCategory)
        {
            case EditorToolCategoryType.System:
                DrawSystem();
                break;
            case EditorToolCategoryType.Entities:
                DrawEntities();
                break;
            case EditorToolCategoryType.Terraformation:
                DrawTerraformation();
                break;
            case EditorToolCategoryType.Generation:
                DrawGeneration();
                break;
            default:
                Clean();
                break;
        }
    }

    void DrawHeader()
    {
        m_container.RemoveAndDestroyAll();
        gameObject.SetActive(true);

        UIElementData.Create<UIElementSimpleText>(m_container).SetText(m_currentCategory.ToString()).SetAlignment(UIElementAlignment.center);
        UIElementData.Create<UIElementSpace>(m_container).SetSpace(5);
    }

    void DrawSystem()
    {
        DrawHeader();

        foreach (EditorSystemButtonType button in Enum.GetValues(typeof(EditorSystemButtonType)))
        {
            var temp = button;
            UIElementData.Create<UIElementButton>(m_container).SetText(button.ToString()).SetClickFunc(() => { OnSystemButtonClick(temp); });
        }
    }

    void OnSystemButtonClick(EditorSystemButtonType button)
    {
        Event<EditorSystemButtonClickedEvent>.Broadcast(new EditorSystemButtonClickedEvent(button));
    }

    void DrawEntities()
    {
        DrawHeader();
    }

    void DrawTerraformation()
    {
        DrawHeader();
    }

    void DrawGeneration()
    {
        DrawHeader();
    }

    void Clean()
    {
        m_container.RemoveAndDestroyAll();

        gameObject.SetActive(false);
    }
}

