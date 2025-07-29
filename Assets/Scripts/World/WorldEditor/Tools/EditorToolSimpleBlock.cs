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
        var mousePos = Input.mousePosition;
        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

        if (cam.camera == null)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        var ray = cam.camera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        bool haveHit = Physics.Raycast(ray, out hit, float.MaxValue, Global.instance.editorDatas.groundLayer);
        m_cursor.SetActive(haveHit);

        if(haveHit)
        {
            var point = hit.point + hit.normal / 2;
            m_point = new Vector3Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), Mathf.RoundToInt(point.z));
            if(GridEx.GetRealPosFromLoop(grid.grid, m_point) != m_point)
            {
                m_cursor.SetActive(false);
                return;
            }

            point -= hit.normal;
            m_pointOnCollision = new Vector3Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), Mathf.RoundToInt(point.z));

            var block = GridEx.GetBlock(grid.grid, m_pointOnCollision);
            if (block.type == BlockType.water)
                m_point = m_pointOnCollision;

            m_cursor.transform.position = m_pointOnCollision;

            if (Input.GetMouseButtonDown(0))
                SetBlock(m_point, true);
            else if (Input.GetMouseButtonDown(1))
                SetBlock(m_pointOnCollision, false);
        }
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

        var obj = new GameObject("Grid Size");
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

        editor.SetBlock(pos, block);
    }
}
