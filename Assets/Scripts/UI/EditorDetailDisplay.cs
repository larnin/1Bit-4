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

public enum EditorSimpleToolType
{
    SimpleBlock,
    Cuboid,
    Sphere,
    Smooth,
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

        var foldBuildings = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Buildings").SetFolded(true);
        foreach(BuildingType type in Enum.GetValues(typeof(BuildingType)))
        {
            if (Global.instance.buildingDatas.GetBuilding(type) == null)
                continue;

            var temp = type;
            UIElementData.Create<UIElementButton>(foldBuildings.GetContainer()).SetText(type.ToString()).SetClickFunc(() => { OnBuildingClick(temp); });
        }

        var foldEntities = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Entities").SetFolded(true);
        //todo entities

        var foldResources = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Resources").SetFolded(true);
        foreach(BlockType type in Enum.GetValues(typeof(BlockType)))
        {
            if (!Global.instance.blockDatas.IsCustomBlock(type))
                continue;

            var temp = type;
            UIElementData.Create<UIElementButton>(foldResources.GetContainer()).SetText(type.ToString()).SetClickFunc(() => { OnResourceClick(temp); });
        }

        var foldQuest = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Quest").SetFolded(true);
        foreach(QuestElementType type in Enum.GetValues(typeof(QuestElementType)))
        {
            if (!Global.instance.editorDatas.questElements.Exists(x => { return x.type == type; }))
                continue;

            var temp = type;
            UIElementData.Create<UIElementButton>(foldQuest.GetContainer()).SetText(type.ToString()).SetClickFunc(() => { OnQuestElementClick(temp); });
        }
    }

    void OnBuildingClick(BuildingType type)
    {
        if (EditorToolHolder.instance == null)
            return;

        var tool = EditorToolHolder.instance.MakePlaceBuildingTool(type);
        EditorToolHolder.instance.SetCurrentTool(tool);
    }

    void OnResourceClick(BlockType type)
    {
        //Event<EditorResourceButtonClickedEvent>.Broadcast(new EditorResourceButtonClickedEvent(type));
    }

    void OnQuestElementClick(QuestElementType type)
    {
        if (EditorToolHolder.instance == null)
            return;

        var tool = EditorToolHolder.instance.MakePlaceQuestElementTool(type);
        EditorToolHolder.instance.SetCurrentTool(tool);
    }

    void DrawTerraformation()
    {
        DrawHeader();

        UIElementData.Create<UIElementSimpleText>(m_container).SetText("Grid size");
        UIElementData.Create<UIElementIntInput>(m_container).SetLabel("- Width").SetValue(GetWorldSize()).SetValueChangeFunc(SetWorldSize).SetBounds(1, 100);
        UIElementData.Create<UIElementIntInput>(m_container).SetLabel("- Height").SetValue(GetWorldHeight()).SetValueChangeFunc(SetWorldHeight).SetBounds(1, 100);
        UIElementData.Create<UIElementToggle>(m_container).SetLabel("- Loop X").SetValue(GetWorldLoopX()).SetValueChangeFunc(SetWorldLoopX);
        UIElementData.Create<UIElementToggle>(m_container).SetLabel("- Loop Z").SetValue(GetWorldLoopZ()).SetValueChangeFunc(SetWorldLoopZ);

        var ToolsContainer = UIElementData.Create<UIElementFoldable>(m_container).SetHeaderText("Tools").GetContainer();
        UIElementData.Create<UIElementButton>(ToolsContainer).SetText("Simple Block").SetClickFunc(() => { EnableSimpleTool(EditorSimpleToolType.SimpleBlock); });
        UIElementData.Create<UIElementButton>(ToolsContainer).SetText("Cuboid").SetClickFunc(() => { EnableSimpleTool(EditorSimpleToolType.Cuboid); });
        UIElementData.Create<UIElementButton>(ToolsContainer).SetText("Sphere").SetClickFunc(() => { EnableSimpleTool(EditorSimpleToolType.Sphere); });
        UIElementData.Create<UIElementButton>(ToolsContainer).SetText("Smooth").SetClickFunc(() => { EnableSimpleTool(EditorSimpleToolType.Smooth); });
    }

    int GetWorldSize()
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid != null)
            return grid.grid.Size();
        return 0;
    }

    int GetWorldHeight()
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid != null)
            return grid.grid.Height();
        return 0;
    }

    void SetWorldSize(int value)
    {
        if (EditorGridBehaviour.instance != null)
            EditorGridBehaviour.instance.SetGridSize(value, GetWorldHeight());
    }

    void SetWorldHeight(int value)
    {

        if (EditorGridBehaviour.instance != null)
            EditorGridBehaviour.instance.SetGridSize(GetWorldSize(), value);
    }

    bool GetWorldLoopX()
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid != null)
            return grid.grid.LoopX();
        return false;
    }

    bool GetWorldLoopZ()
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid != null)
            return grid.grid.LoopZ();
        return false;
    }

    void SetWorldLoopX(bool loop)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid != null)
            grid.grid.SetLoopX(loop);
    }

    void SetWorldLoopZ(bool loop)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid != null)
            grid.grid.SetLoopZ(loop);
    }

    void EnableSimpleTool(EditorSimpleToolType type)
    {
        EditorToolBase tool = null;
        switch(type)
        {
            case EditorSimpleToolType.SimpleBlock:
                tool = new EditorToolSimpleBlock();
                break;
            case EditorSimpleToolType.Cuboid:
                tool = new EditorToolShapeBlock(EditorToolShape.Cuboid);
                break;
            case EditorSimpleToolType.Sphere:
                tool = new EditorToolShapeBlock(EditorToolShape.Sphere);
                break;
            case EditorSimpleToolType.Smooth:
                tool = new EditorToolShapeBlock(EditorToolShape.Smooth);
                break;
            default:
                throw new NotImplementedException("Implement theses fucking tools");
        }

        if (tool == null)
            return;

        var holder = EditorToolHolder.instance;
        if (holder == null)
            return;

        holder.SetCurrentTool(tool);
    }

    void DrawGeneration()
    {
        DrawHeader();

        if(EditorWorldGeneration.instance != null)
        {
            EditorWorldGeneration.instance.DrawSettings(m_container);
        }
    }

    void Clean()
    {
        m_container.RemoveAndDestroyAll();

        gameObject.SetActive(false);
    }
}

