using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolSimpleBlock : EditorToolBase
{
    GameObject m_cursor;

    Vector3Int m_point;
    Vector3Int m_pointOnCollision;

    public override void Begin()
    {
        CreateCursor();
    }

    public override void Update()
    {
        var overUI = Event<IsMouseOverUIEvent>.Broadcast(new IsMouseOverUIEvent());
        if (overUI.overUI)
        {
            m_cursor.SetActive(false);
            return;
        }

        bool haveHit = GetMouseBlockTarget(out m_point, out m_pointOnCollision);
        m_cursor.SetActive(haveHit);

        if (haveHit)
        {
            m_cursor.transform.position = m_pointOnCollision;

            if (Input.GetMouseButtonDown(0))
                SetBlock(m_point, true);
            else if (Input.GetMouseButtonDown(1))
                SetBlock(m_pointOnCollision, false);
        }
    }

    public static bool GetMouseBlockTarget(out Vector3Int point, out Vector3Int pointOnCollision)
    {
        point = Vector3Int.zero;
        pointOnCollision = Vector3Int.zero;

        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (cam.camera == null)
            return false;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return false;

        var mousePos = Input.mousePosition;

        var ray = cam.camera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        bool haveHit = Physics.Raycast(ray, out hit, float.MaxValue, Global.instance.editorDatas.groundLayer);
        if (!haveHit)
            return false;

        var hitPoint = hit.point + hit.normal / 2;
        point = new Vector3Int(Mathf.RoundToInt(hitPoint.x), Mathf.RoundToInt(hitPoint.y), Mathf.RoundToInt(hitPoint.z));
        if (GridEx.GetRealPosFromLoop(grid.grid, point) != point)
            return false;

        hitPoint -= hit.normal;
        pointOnCollision = new Vector3Int(Mathf.RoundToInt(hitPoint.x), Mathf.RoundToInt(hitPoint.y), Mathf.RoundToInt(hitPoint.z));

        var block = GridEx.GetBlock(grid.grid, pointOnCollision);
        if (block.type == BlockType.water)
            point = pointOnCollision;

        return true;
    }

    public override void End()
    {
        if (m_cursor != null)
            GameObject.Destroy(m_cursor);
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
        obj.transform.localScale = Vector3.one * 1.1f;
       
        var filter = obj.AddComponent<MeshFilter>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = Global.instance.editorDatas.cursorMaterial;

        if (filter.mesh != null)
            GameObject.Destroy(filter.mesh);

        filter.mesh = WireframeMesh.SimpleCube(Vector3.one, Color.white);

        m_cursor = obj;
        m_cursor.SetActive(false);
    }

    void SetBlock(Vector3Int pos, bool place)
    {
        var editor = EditorGridBehaviour.instance;
        if (editor == null)
            return;

        var block = new Block(BlockType.ground);
        if(!place)
        {
            if (pos.y == 0)
                block.type = BlockType.water;
            else block.type = BlockType.air;
        }

        if (editor.GetGrid() != null)
        {
            var oldBlock = GridEx.GetBlock(editor.GetGrid(), pos);
            editor.SetBlock(pos, block);

            if (UndoList.instance != null)
            {
                var undo = new UndoElementBlocks();
                undo.AddBlock(pos, oldBlock, block);
                UndoList.instance.AddStep(undo);
            }
        }
    }
}
