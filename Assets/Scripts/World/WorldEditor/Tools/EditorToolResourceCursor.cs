using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolResourceCursor : EditorToolBase
{
    enum CursorState
    {
        Valid,
        OnWater,
        NeedGround,
        NeedSurface,
        UnknowError,
    }

    BlockType m_type;
    GameObject m_instance;

    bool m_cursorValid = false;
    Vector3Int m_cursorPos = Vector3Int.zero;

    public void SetResourceType(BlockType type)
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
            GameObject.Destroy(m_instance);
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

        Vector3 pos;
        m_cursorValid = PlaceBuildingCursor.LoopCursorRatcast(ray, Global.instance.editorDatas.groundLayer, out pos);
        m_cursorPos = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
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
        //if (!m_cursorValid)
        //    return;

        //if (!Input.GetMouseButtonDown(0))
        //    return;

        //var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        //if (grid.grid == null)
        //    return;

        //var pos = GridEx.GetRealPosFromLoop(grid.grid, m_cursorPos);

        //var prefab = Global.instance.editorDatas.GetQuestElementPrefab(m_type);
        //if (prefab == null)
        //    return;

        //var instance = GameObject.Instantiate(prefab);
        //if (QuestElementList.instance != null)
        //    instance.transform.parent = QuestElementList.instance.transform;

        //instance.transform.position = m_cursorPos;
    }

    void CreateInstance()
    {
        if (m_instance != null)
            GameObject.Destroy(m_instance);

        var block = Global.instance.blockDatas.GetCustomBlock(m_type);
        if (block == null || block.prefab == null)
            return;

        m_instance = GameObject.Instantiate(block.prefab);

        m_instance.transform.parent = m_holder.transform;
        m_instance.transform.position = Vector3.zero;
        m_instance.transform.rotation = Quaternion.identity;
        m_instance.transform.localScale = Vector3.one;

        var components = m_instance.GetComponentsInChildren<Collider>();
        foreach (var c in components)
            GameObject.Destroy(c);

        m_instance.SetActive(false);
    }
}
