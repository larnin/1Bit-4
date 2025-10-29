using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolQuestCursor : EditorToolBase
{
    QuestElementType m_type;
    QuestElement m_instance;

    bool m_cursorValid = false;
    Vector3 m_cursorPos = Vector3.zero;

    public void SetQuestElementType(QuestElementType type)
    {
        m_type = type;
        if (m_holder != null)
            CreateInstance();
    }

    public override void Begin()
    {
        CreateInstance();
    }

    public override void Update()
    {
        UpdatePos();
        UpdateInstance();
        UpdateClick();
    }

    public override void End()
    {
        if (m_instance != null)
            GameObject.Destroy(m_instance.gameObject);
    }

    void UpdatePos()
    {
        m_cursorValid = false;

        if (m_instance == null)
            return;

        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (cam.camera == null)
            return;

        var overUI = Event<IsMouseOverUIEvent>.Broadcast(new IsMouseOverUIEvent());
        if (overUI.overUI)
            return;

        var ray = cam.camera.ScreenPointToRay(Input.mousePosition);

        m_cursorValid = PlaceBuildingCursor.LoopCursorRatcast(ray, Global.instance.editorDatas.groundLayer, out m_cursorPos);
    }

    void UpdateInstance()
    {
        if (m_instance == null)
            return;

        m_instance.transform.position = m_cursorPos;
        m_instance.gameObject.SetActive(m_cursorValid);
    }

    void UpdateClick()
    {
        if (!m_cursorValid)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        var pos = GridEx.GetRealPosFromLoop(grid.grid, m_cursorPos);

        var prefab = Global.instance.editorDatas.GetQuestElementPrefab(m_type);
        if (prefab == null)
            return;

        var instance = GameObject.Instantiate(prefab);
        if (QuestElementList.instance != null)
            instance.transform.parent = QuestElementList.instance.transform;

        instance.transform.position = m_cursorPos;

        if (UndoList.instance != null)
        {
            var ID = Event<GetEntityIDEvent>.Broadcast(new GetEntityIDEvent(), instance).id;
            var questElm = instance.GetComponent<QuestElement>();
            if (questElm != null)
            {
                var undo = new UndoElementEntityChange();
                undo.SetPlace(EntityType.Building, ID, questElm.Save());
                UndoList.instance.AddStep(undo);
            }
        }
    }

    void CreateInstance()
    {
        if (m_instance != null)
            GameObject.Destroy(m_instance);

        var prefab = Global.instance.editorDatas.GetQuestElementPrefab(m_type);
        if (prefab == null)
            return;

        var instance = GameObject.Instantiate(prefab);
        m_instance = instance.GetComponent<QuestElement>();
        m_instance.SetAsCursor(true);

        if(m_instance == null)
        {
            GameObject.Destroy(instance);
            return;
        }

        instance.transform.parent = m_holder.transform;
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        instance.SetActive(false);
    }
}
