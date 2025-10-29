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
        NeedGround,
        NeedSurface,
        InvalidPosition,
        UnknowError,
    }

    BlockType m_type;
    GameObject m_instance;

    bool m_cursorValid = false;
    CursorState m_cursorState;
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
        m_cursorState = GetCursorState();
        UpdateCross();
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

    CursorState GetCursorState()
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return CursorState.InvalidPosition;

        var loopPos = GridEx.GetRealPosFromLoop(grid.grid, m_cursorPos);
        if (grid.grid.LoopX())
            m_cursorPos.x = loopPos.x;
        if (grid.grid.LoopZ())
            m_cursorPos.z = loopPos.z;

        var size = GridEx.GetRealSize(grid.grid);
        if (m_cursorPos.x < 0 || m_cursorPos.z < 0 || m_cursorPos.x >= size || m_cursorPos.z >= size)
            return CursorState.InvalidPosition;

        var downPos = m_cursorPos;
        downPos.y--;

        if (downPos.y < 0 || downPos.y >= GridEx.GetRealHeight(grid.grid) - 1)
            return CursorState.InvalidPosition;
        
        var block = GridEx.GetBlock(grid.grid, downPos);

        if (m_type == BlockType.crystal)
        {
            if (block.type != BlockType.ground)
                return CursorState.NeedGround;

            return CursorState.Valid;

        }
        else if(m_type == BlockType.Titanium)
        {
            if (block.type != BlockType.ground && block.type != BlockType.Titanium)
                return CursorState.NeedGround;

            return CursorState.Valid;
        }
        else if(m_type == BlockType.oil)
        {
            m_cursorPos = downPos;

            if (block.type != BlockType.ground)
                return CursorState.NeedGround;

            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;

                    var ground = GridEx.GetBlock(grid.grid, downPos + new Vector3Int(i, 0, j));
                    if (ground.type != BlockType.ground)
                        return CursorState.NeedSurface;
                    var air = GridEx.GetBlock(grid.grid, downPos + new Vector3Int(i, 1, j));
                    if (air.type != BlockType.air)
                        return CursorState.NeedSurface;
                }
            }
            return CursorState.Valid;
        }

        return CursorState.UnknowError;
    }

    void UpdateCross()
    {
        if (m_cursorState == CursorState.Valid || !m_cursorValid)
        {
            EnableCross(false);
            return;
        }

        EnableCross(true, GetCursorStateText());
    }

    void EnableCross(bool enabled, string message = "")
    {
        if (DisplayIconsV2.instance == null || m_instance == null)
            return;


        if (enabled)
            DisplayIconsV2.instance.Register(m_instance, 0, "Cross", message);
        else DisplayIconsV2.instance.Unregister(m_instance.gameObject);
    }

    string GetCursorStateText()
    {
        switch(m_cursorState)
        {
            case CursorState.NeedGround:
                return "Need ground";
            case CursorState.InvalidPosition:
                return "Invalid position";
            case CursorState.NeedSurface:
                return "Need flat surface";
            case CursorState.Valid:
            case CursorState.UnknowError:
            default:
                break;
        }

        return "";
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
        if (!m_cursorValid || m_cursorState != CursorState.Valid)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        if (EditorGridBehaviour.instance == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        var oldBlock = GridEx.GetBlock(grid.grid, m_cursorPos);

        EditorGridBehaviour.instance.SetBlock(m_cursorPos, new Block(m_type));

        if (UndoList.instance != null)
        {
            var undo = new UndoElementBlocks();
            undo.AddBlock(m_cursorPos, oldBlock, new Block(m_type));
            UndoList.instance.AddStep(undo);
        }
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
