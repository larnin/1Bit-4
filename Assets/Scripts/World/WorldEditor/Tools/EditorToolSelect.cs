using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolSelect : EditorToolBase
{
    GameObject m_cursor;

    GameObject m_selectedObject;

    bool m_updateCursorNextFrame = false;

    public override void Begin()
    {
        CreateCursor();
        m_selectedObject = null;
    }

    public override void Update()
    {
        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (camera.camera == null)
            return;

        if(Input.GetMouseButtonDown(0))
        {
            var overUI = Event<IsMouseOverUIEvent>.Broadcast(new IsMouseOverUIEvent());
            if (!overUI.overUI)
            {
                var ray = camera.camera.ScreenPointToRay(Input.mousePosition);

                GameObject newTarget = SelectCursor.LoopHoverRatcast(ray, Global.instance.editorDatas.toolHoverLayer);

                if (newTarget != null)
                {
                    var type = GameSystem.GetEntityType(newTarget);
                    if (type == EntityType.Building || type == EntityType.Building || type == EntityType.Quest || type == EntityType.Resource)
                        SelectObject(newTarget);
                    else SelectObject(null);
                }
                else SelectObject(null);
            }
        }

        if(Input.GetKeyDown(KeyCode.Delete))
            DestroySelectedObject();

        if (m_updateCursorNextFrame)
            UpdateCursor();
    }

    public override void End()
    {
        if (m_cursor != null)
            GameObject.Destroy(m_cursor);

        SelectObject(null);
    }

    void CreateCursor()
    {
        if (m_cursor != null)
            return;

        var obj = new GameObject("Cursor");
        obj.layer = LayerMask.NameToLayer(Global.instance.editorDatas.editorLayer);
        obj.transform.parent = m_holder.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        var filter = obj.AddComponent<MeshFilter>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = Global.instance.editorDatas.cursorMaterial;

        if (filter.mesh != null)
            GameObject.Destroy(filter.mesh);

        filter.mesh = WireframeMesh.SimpleCube(Vector3.one, Color.white);

        m_cursor = obj;
        m_cursor.SetActive(false);
    }

    void SelectObject(GameObject obj)
    {
        m_selectedObject = obj;

        UpdateCursor();

        UpdateSelectedDetails();
    }

    void UpdateCursor()
    {
        m_cursor.SetActive(m_selectedObject != null);

        if (m_selectedObject != null)
        {
            var collider = m_selectedObject.GetComponent<Collider>();
            if (collider != null)
            {
                var bound = collider.bounds;
                m_cursor.transform.position = bound.center;
                m_cursor.transform.localScale = bound.size + Vector3.one * 0.2f;
            }
            else m_cursor.transform.localScale = Vector3.one;
        }
    }

    void DestroySelectedObject()
    {
        if (m_selectedObject == null)
            return;

        var type = GameSystem.GetEntityType(m_selectedObject);

        if (type == EntityType.Resource)
        {
            if (EditorGridBehaviour.instance == null)
                return;

            var pos = m_selectedObject.transform.position;
            var posI = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

            if (posI.y == 0)
                EditorGridBehaviour.instance.SetBlock(posI, new Block(BlockType.water));
            else EditorGridBehaviour.instance.SetBlock(posI, new Block(BlockType.air));
        }
        else
        {
            GameObject.Destroy(m_selectedObject);
            m_selectedObject = null;
        }

        SelectObject(null);
    }

    void UpdateSelectedDetails()
    {
        if(m_selectedObject == null)
        {
            Event<EnableEditorCustomToolEvent>.Broadcast(new EnableEditorCustomToolEvent(false));
            return;
        }

        var type = GameSystem.GetEntityType(m_selectedObject);

        if(type == EntityType.Building)
        {
            var container = Event<EnableEditorCustomToolEvent>.Broadcast(new EnableEditorCustomToolEvent(true)).container;

            BuildingBase building = m_selectedObject.GetComponent<BuildingBase>();
            if(building != null)
            {
                building.DisplayGenericInfos(container);

                UIElementData.Create<UIElementComboBox>(container).SetElementsFromEnum(typeof(Team)).SetLabel("Team").SetValueChangeFunc(OnTeamChange).SetCurrentElementIndex((int)building.GetTeam());
            }

        }
        else if(type == EntityType.Quest)
        {
            var container = Event<EnableEditorCustomToolEvent>.Broadcast(new EnableEditorCustomToolEvent(true)).container;

            QuestElement element = m_selectedObject.GetComponent<QuestElement>();
            if(element != null)
            {
                UIElementData.Create<UIElementSimpleText>(container).SetText("Quest element: " + element.GetQuestElementType().ToString()).SetAlignment(UIElementAlignment.center);
                UIElementData.Create<UIElementSpace>(container).SetSpace(5);

                UIElementData.Create<UIElementTextInput>(container).SetLabel("Name").SetText(element.GetName()).SetTextChangeFunc(OnNameChange);

                var pos = element.transform.position;
                var posContainer = UIElementData.Create<UIElementFoldable>(container).SetHeaderText("Pos").GetContainer();
                UIElementData.Create<UIElementFloatInput>(posContainer).SetLabel("X").SetValue(pos.x).SetValueChangeFunc(x => { OnPosChange(x, 0); });
                UIElementData.Create<UIElementFloatInput>(posContainer).SetLabel("Y").SetValue(pos.y).SetValueChangeFunc(x => { OnPosChange(x, 1); });
                UIElementData.Create<UIElementFloatInput>(posContainer).SetLabel("Z").SetValue(pos.z).SetValueChangeFunc(x => { OnPosChange(x, 2); });

                if(element.GetQuestElementType() == QuestElementType.Cuboid)
                {
                    var size = element.GetSize();
                    var sizeContainer = UIElementData.Create<UIElementFoldable>(container).SetHeaderText("Size").GetContainer();
                    UIElementData.Create<UIElementFloatInput>(sizeContainer).SetLabel("X").SetValue(size.x).SetValueChangeFunc(x => { OnSizeChange(x, 0); });
                    UIElementData.Create<UIElementFloatInput>(sizeContainer).SetLabel("Y").SetValue(size.y).SetValueChangeFunc(x => { OnSizeChange(x, 1); });
                    UIElementData.Create<UIElementFloatInput>(sizeContainer).SetLabel("Z").SetValue(size.z).SetValueChangeFunc(x => { OnSizeChange(x, 2); });

                    UIElementData.Create<UIElementSpace>(container).SetSpace(5);
                    var rot = element.transform.rotation.eulerAngles;
                    UIElementData.Create<UIElementFloatInput>(container).SetLabel("Rot").SetValue(rot.y).SetIncrement(10).SetValueChangeFunc(OnRotChange);

                }
                else if(element.GetQuestElementType() == QuestElementType.Sphere)
                {
                    var radius = element.GetRadius();
                    UIElementData.Create<UIElementFloatInput>(container).SetLabel("Radius").SetValue(radius).SetValueChangeFunc(x => { OnSizeChange(x, -1); });
                }
            }
        }
        else Event<EnableEditorCustomToolEvent>.Broadcast(new EnableEditorCustomToolEvent(false));
    }

    void OnTeamChange(int index)
    {
        if (m_selectedObject == null)
            return;

        BuildingBase building = m_selectedObject.GetComponent<BuildingBase>();
        if(building != null)
        {
            building.SetTeam((Team)index);
        }
    }

    void OnPosChange(float value, int index)
    {
        if (m_selectedObject == null)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        QuestElement element = m_selectedObject.GetComponent<QuestElement>();
        if(element != null)
        {
            var pos = element.transform.position;
            pos[index] = value;

            var loopPos = GridEx.GetRealPosFromLoop(grid.grid, pos);

            if (grid.grid.LoopX())
                pos.x = loopPos.x;
            if (grid.grid.LoopZ())
                pos.z = loopPos.z;

            element.transform.position = pos;
        }

        m_updateCursorNextFrame = true;
    }

    void OnSizeChange(float value, int index)
    {
        if (m_selectedObject == null)
            return;

        QuestElement element = m_selectedObject.GetComponent<QuestElement>();
        if (element != null)
        {
            if(element.GetQuestElementType() == QuestElementType.Cuboid)
            {
                var size = element.GetSize();
                if(index >= 0 && index < 3)
                {
                    size[index] = value;
                    element.SetSize(size);
                }
            }
            else if(element.GetQuestElementType() == QuestElementType.Sphere)
            {
                if (value > 0)
                    element.SetRadius(value);
            }
        }

        m_updateCursorNextFrame = true;
    }

    void OnRotChange(float value)
    {
        if (m_selectedObject == null)
            return;

        QuestElement element = m_selectedObject.GetComponent<QuestElement>();
        if (element != null)
        {
            if (element.GetQuestElementType() == QuestElementType.Cuboid)
                element.transform.rotation = Quaternion.Euler(0, value, 0);
        }

        m_updateCursorNextFrame = true;
    }

    void OnNameChange(string name)
    {
        if (m_selectedObject == null)
            return;

        QuestElement element = m_selectedObject.GetComponent<QuestElement>();
        if (element != null)
        {
            element.SetName(name);
        }
    }
}
