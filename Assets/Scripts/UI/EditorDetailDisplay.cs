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

class WorldData
{
    public int size = 4;
    public int height = 2;
}

public class EditorDetailDisplay : MonoBehaviour
{
    [SerializeField] Sprite m_testSprite;

    UIElementContainer m_container;

    SubscriberList m_subscriberList = new SubscriberList();

    EditorToolCategoryType m_currentCategory = EditorToolCategoryType.None;

    WorldData m_worldData = new WorldData();

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

        var foldBuildings = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Buildings");
        foreach(BuildingType type in Enum.GetValues(typeof(BuildingType)))
        {
            if (Global.instance.buildingDatas.GetBuilding(type) == null)
                continue;

            var temp = type;
            UIElementData.Create<UIElementButton>(foldBuildings.GetContainer()).SetText(type.ToString()).SetClickFunc(() => { OnBuildingClick(temp); });
        }

        var foldEntities = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Entities");
        //todo entities

        var foldResources = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Resources");
        foreach(BlockType type in Enum.GetValues(typeof(BlockType)))
        {
            if (!Global.instance.blockDatas.IsCustomBlock(type))
                continue;

            var temp = type;
            UIElementData.Create<UIElementButton>(foldResources.GetContainer()).SetText(type.ToString()).SetClickFunc(() => { OnResourceClick(temp); });
        }

        var foldQuest = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Quest");
        //todo quest elements
    }

    void OnBuildingClick(BuildingType type)
    {
        Event<EditorBuildingButtonClickedEvent>.Broadcast(new EditorBuildingButtonClickedEvent(type));
    }

    void OnResourceClick(BlockType type)
    {
        Event<EditorResourceButtonClickedEvent>.Broadcast(new EditorResourceButtonClickedEvent(type));
    }

    void DrawTerraformation()
    {
        DrawHeader();

        UIElementData.Create<UIElementSimpleText>(m_container).SetText("Grid size");
        UIElementData.Create<UIElementIntInput>(m_container).SetLabel("- Width").SetValue(m_worldData.size).SetValueChangeFunc(SetWorldSize);
        UIElementData.Create<UIElementIntInput>(m_container).SetLabel("- Height").SetValue(m_worldData.height).SetValueChangeFunc(SetWorldHeight);

        var foldTools = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Tools");
    }

    void SetWorldSize(int value)
    {
        m_worldData.size = value;
    }

    void SetWorldHeight(int value)
    {
        m_worldData.height = value;
    }

    void DrawGeneration()
    {
        DrawHeader();

        UIElementData.Create<UIElementLabelAndText>(m_container).SetLabel("Label").SetText("Text a little longer");
        UIElementData.Create<UIElementLine>(m_container);
        UIElementData.Create<UIElementFloatInput>(m_container).SetLabel("Float").SetValue(14.37f);
        UIElementData.Create<UIElementTextInput>(m_container).SetLabel("Text").SetText("Nothing !");
        UIElementData.Create<UIElementSprite>(m_container).SetSprite(m_testSprite).SetPreserveAspect(true).SetAlignment(UIElementAlignment.left).SetScale(2);
        UIElementData.Create<UIElementToggle>(m_container).SetLabel("Toggle");
        UIElementData.Create<UIElementFillValue>(m_container).SetLabel("Fill").SetMax(10).SetValue(7);
    }

    void Clean()
    {
        m_container.RemoveAndDestroyAll();

        gameObject.SetActive(false);
    }
}

