using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ToolCategoryType
{
    None,
    System,
    Entities,
    Terraformation,
    Generation,
}

public class EditorDetailDisplay : MonoBehaviour
{
    UIElementContainer m_container;

    SubscriberList m_subscriberList = new SubscriberList();

    ToolCategoryType m_currentCategory = ToolCategoryType.None;

    private void Awake()
    {
        m_subscriberList.Add(new Event<ToggleToolCategoryEvent>.Subscriber(ToogleToolCategory));
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

    void ToogleToolCategory(ToggleToolCategoryEvent e)
    {
        if (m_currentCategory == e.category)
            m_currentCategory = ToolCategoryType.None;
        else m_currentCategory = e.category;

        switch(m_currentCategory)
        {
            case ToolCategoryType.System:
                DrawSystem();
                break;
            case ToolCategoryType.Entities:
                DrawEntities();
                break;
            case ToolCategoryType.Terraformation:
                DrawTerraformation();
                break;
            case ToolCategoryType.Generation:
                DrawGeneration();
                break;
            default:
                Clean();
                break;
        }
    }

    void DrawSystem()
    {

    }

    void DrawEntities()
    {

    }

    void DrawTerraformation()
    {

    }

    void DrawGeneration()
    {

    }

    void Clean()
    {

    }
}

