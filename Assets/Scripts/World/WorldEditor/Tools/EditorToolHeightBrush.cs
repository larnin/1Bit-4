using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolHeightBrush : EditorToolBase
{
    float m_toolSpeed = 10;
    float m_radius = 10;
    GameObject m_cursor;

    Vector3Int m_size = -Vector3Int.one;

    Matrix<float> m_heights;
    bool m_lastPace = false;
    Vector3 m_point;

    UndoElementBlocks m_undo = null;

    public void SetRadius(float radius)
    {
        m_radius = radius;
        if(m_cursor != null)
            UpdateCursor();
    }

    public void SetSpeed(float speed)
    {
        m_toolSpeed = speed;
    }

    public override void Begin()
    {
        if (EditorGridBehaviour.instance == null)
            return;

        UpdateCursor();

        UpdateGrid();
    }

    public override void Update()
    {
        if (EditorGridBehaviour.instance == null)
            return;

        UpdateGrid();

        var overUI = Event<IsMouseOverUIEvent>.Broadcast(new IsMouseOverUIEvent());
        if (overUI.overUI)
        {
            m_cursor.SetActive(false);
            return;
        }

        bool haveHit = GetMouseTarget(out m_point);
        m_cursor.SetActive(haveHit);

        if(haveHit)
        {
            m_cursor.transform.position = m_point;

            if (Input.GetMouseButton(0))
                Clicking(true);
            else if (Input.GetMouseButton(1))
                Clicking(false);
            else if(m_undo != null)
            {
                if (UndoList.instance != null)
                    UndoList.instance.AddStep(m_undo);
                m_undo = null;
            }
        }
    }

    public override void End()
    {
        if (m_cursor != null)
        {
            GameObject.Destroy(m_cursor);
            m_cursor = null;
        }
    }

    bool GetMouseTarget(out Vector3 outPos)
    {
        outPos = Vector3.zero;

        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (cam.camera == null)
            return false;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return false;

        var mousePos = Input.mousePosition;

        var ray = cam.camera.ScreenPointToRay(mousePos);

        float enter;
        var plane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));
        bool haveHit = plane.Raycast(ray, out enter);

        if (!haveHit)
            return false;

        outPos = ray.GetPoint(enter);
        outPos.y += 0.1f;

        return true;
    }

    void UpdateCursor()
    {
        if (m_cursor != null)
        {
            GameObject.Destroy(m_cursor);
            m_cursor = null;
        }

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

        filter.mesh = WireframeMesh.Cylinder(Vector3.zero, m_radius, 5, 12, 6, Color.white);

        m_cursor = obj;
        m_cursor.SetActive(false);
    }

    void UpdateGrid()
    {
        var grid = EditorGridBehaviour.instance.GetGrid();
        if (grid == null)
            return;

        Vector3Int size = new Vector3Int(GridEx.GetRealSize(grid), GridEx.GetRealHeight(grid), GridEx.GetRealSize(grid));

        if (size == m_size && m_heights != null)
            return;

        m_size = size;

        m_heights = new Matrix<float>(m_size.x, m_size.z);
    }

    void Clicking(bool place)
    {
        if (place != m_lastPace)
            m_heights.SetAll(0);

        m_lastPace = place;

        var grid = EditorGridBehaviour.instance.GetGrid();
        if (grid == null)
            return;

        Vector2 point2D = new Vector2(m_point.x, m_point.z);

        int minX = Mathf.FloorToInt(point2D.x - m_radius);
        int minY = Mathf.FloorToInt(point2D.y - m_radius);
        int maxX = Mathf.CeilToInt(point2D.x + m_radius);
        int maxY = Mathf.CeilToInt(point2D.y + m_radius);

        if (m_undo == null)
            m_undo = new UndoElementBlocks();
        List<Vector3Int> updateChunks = new List<Vector3Int>();

        int gridHeight = GridEx.GetRealHeight(grid);

        for (int i = minX; i <= maxX; i++)
        {
            for(int j = minY; j <= maxY; j++)
            {
                var loopPos = GridEx.GetRealPosFromLoop(grid, new Vector3Int(i, 0, j));
                if (loopPos.x != i && !grid.LoopX())
                    continue;
                if (loopPos.z != j && !grid.LoopZ())
                    continue;

                float dist = (new Vector2(i, j) - point2D).magnitude / m_radius;
                if (dist > 1)
                    continue;

                float value = Mathf.Exp(-5 * dist * dist);

                value *= Time.deltaTime * m_toolSpeed;
                if (!place)
                    value *= -1;

                float current = m_heights.Get(loopPos.x, loopPos.z);
                float next = current + value;
                int count = (int) next;
                next -= count;
                m_heights.Set(loopPos.x, loopPos.z, next);

                if (count == 0)
                    continue;

                int currentHeight = GetHeight(grid, new Vector2Int(i, j));
                int targetHeight = currentHeight + count;
                int realHeight = GridEx.GetHeight(grid, new Vector2Int(i, j));

                if (targetHeight < 0)
                    targetHeight = 0;
                if (targetHeight >= gridHeight)
                    targetHeight = gridHeight - 1;

                if(count > 0)
                {
                    if(currentHeight == 0)
                        SetBlock(grid, new Vector3Int(i, 0, j), BlockType.ground, m_undo, updateChunks);

                    for(int k = currentHeight + 1; k <= targetHeight; k++)
                        SetBlock(grid, new Vector3Int(i, k, j), BlockType.ground, m_undo, updateChunks);
                }
                else
                {
                    if(realHeight > currentHeight)
                    {
                        for (int k = realHeight; k > currentHeight; k--)
                            SetBlock(grid, new Vector3Int(i, k, j), BlockType.air, m_undo, updateChunks);
                    }

                    for(int k = currentHeight; k > targetHeight; k--)
                        SetBlock(grid, new Vector3Int(i, k, j), BlockType.air, m_undo, updateChunks);

                    if(currentHeight == 0)
                        SetBlock(grid, new Vector3Int(i, 0, j), BlockType.water, m_undo, updateChunks);
                }
            }
        }

        foreach (var chunk in updateChunks)
            EditorGridBehaviour.instance.SetChunkDirty(chunk);
    }

    void SetBlock(Grid grid, Vector3Int pos, BlockType type, UndoElementBlocks undo, List<Vector3Int> updateChunks)
    {
        var oldBlock = GridEx.GetBlock(grid, pos);
        var newBlock = new Block(type);
        GridEx.SetBlock(grid, pos, newBlock);

        undo.AddBlock(pos, oldBlock, newBlock);

        var chunk = Grid.PosToChunkIndex(pos);
        if (!updateChunks.Contains(chunk))
            updateChunks.Add(chunk);
    }

    int GetHeight(Grid grid, Vector2Int pos)
    {
        int height = GridEx.GetHeight(grid, pos);
        if (height <= 0)
            return height;

        for(int i = height; i > 0; i++)
        {
            var block = GridEx.GetBlock(grid, new Vector3Int(pos.x, i, pos.y));
            if (block.type == BlockType.ground || block.type == BlockType.water)
                return i;
        }

        return 0;
    }
}