using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum EditorToolShape
{
    Cuboid,
    Smooth,
    Sphere
}

public class EditorToolShapeBlock : EditorToolBase
{
    EditorToolShape m_shape;
    GameObject m_cursor;
    MeshFilter m_cursorMesh;

    Vector3Int m_posStart;
    Vector3Int m_posEnd;
    bool m_started = false;
    bool m_placeBlock = false;

    SubscriberList m_subscriberList = new SubscriberList();

    public EditorToolShapeBlock(EditorToolShape shape)
    {
        m_shape = shape;

        m_subscriberList.Add(new Event<IsScrollLockedEvent>.Subscriber(ScrollLocked));
    }

    public override void Begin()
    {
        m_subscriberList.Subscribe();

        CreateCursor();
        SetSimpleCursor();
    }

    public override void Update()
    {
        var mousePos = Input.mousePosition;
        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

        if (cam.camera == null)
            return;

        var ray = cam.camera.ScreenPointToRay(mousePos);

        if (m_started)
            UpdateStarted(ray);
        else UpdateNotStarted(ray);
    }

    void UpdateNotStarted(Ray cursorRay)
    {
        Vector3Int point;
        Vector3Int pointOnCollision;

        bool haveHit = EditorToolSimpleBlock.GetMouseBlockTarget(out point, out pointOnCollision);
        m_cursor.SetActive(haveHit);

        if(haveHit)
        {
            m_cursor.transform.position = pointOnCollision;

            if (Input.GetMouseButtonDown(0))
                StartPlace(point, true);
            else if (Input.GetMouseButtonDown(1))
                StartPlace(pointOnCollision, false);
        }
    }

    void StartPlace(Vector3Int pos, bool place)
    {
        m_posStart = pos;
        m_posEnd = pos;

        m_started = true;
        m_placeBlock = place;

        SetSelectionCursor();
    }

    void UpdateStarted(Ray cursorRay)
    {
        var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        if (cam.camera == null)
            return;

        const int MaxSize = 100;

        Vector3Int oldPos = m_posEnd;

        var plane = new Plane(Vector3.up, m_posStart);

        var mousePos = Input.mousePosition;
        var ray = cam.camera.ScreenPointToRay(mousePos);

        float enter;
        if (!plane.Raycast(ray, out enter))
            return;

        Vector3 hit = ray.GetPoint(enter);
        m_posEnd = new Vector3Int(Mathf.RoundToInt(hit.x), Mathf.RoundToInt(hit.y), Mathf.RoundToInt(hit.z));
        m_posEnd.y = oldPos.y;

        m_posEnd.y += Mathf.FloorToInt(Input.mouseScrollDelta.y);

        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(m_posStart[i] - m_posEnd[i]) > MaxSize)
                m_posEnd[i] = m_posStart[i] + MaxSize * ((m_posEnd[i] - m_posStart[i]) >= 0 ? 1 : -1);
        }

        if (m_posEnd != oldPos)
            SetSelectionCursor();

        Vector3Int min = new Vector3Int(Mathf.Min(m_posStart.x, m_posEnd.x), Mathf.Min(m_posStart.y, m_posEnd.y), Mathf.Min(m_posStart.z, m_posEnd.z));
        m_cursor.transform.position = min;
        m_cursor.transform.position -= Vector3.one * 0.5f;

        int button = m_placeBlock ? 0 : 1;
        if(Input.GetMouseButtonUp(button) || !Input.GetMouseButton(button))
        {
            SetBlocks();
            EndPlace();
        }
    }

    void EndPlace()
    {
        m_posStart = Vector3Int.zero;
        m_posEnd = Vector3Int.zero;

        m_started = false;
        m_placeBlock = false;

        SetSimpleCursor();
    }

    public override void End()
    {
        m_subscriberList.Unsubscribe();

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
        obj.transform.localScale = Vector3.one;

        var filter = obj.AddComponent<MeshFilter>();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = Global.instance.editorDatas.cursorMaterial;

        if (filter.mesh != null)
            GameObject.Destroy(filter.mesh);

        m_cursor = obj;
        m_cursor.SetActive(false);
        m_cursorMesh = filter;
    }

    void SetSimpleCursor()
    {
        if (m_cursorMesh.mesh != null)
            GameObject.Destroy(m_cursorMesh.mesh);
        m_cursorMesh.mesh = WireframeMesh.SimpleCube(Vector3.one * 1.1f, Color.white);
    }

    void SetSelectionCursor()
    {
        if (m_cursorMesh.mesh != null)
            GameObject.Destroy(m_cursorMesh.mesh);

        Vector3Int min = new Vector3Int(Mathf.Min(m_posStart.x, m_posEnd.x), Mathf.Min(m_posStart.y, m_posEnd.y), Mathf.Min(m_posStart.z, m_posEnd.z));
        Vector3Int max = new Vector3Int(Mathf.Max(m_posStart.x, m_posEnd.x), Mathf.Max(m_posStart.y, m_posEnd.y), Mathf.Max(m_posStart.z, m_posEnd.z));
        Vector3Int size = max - min + Vector3Int.one;

        if (m_shape == EditorToolShape.Cuboid || m_shape == EditorToolShape.Smooth)
            m_cursorMesh.mesh = WireframeMesh.Cuboid(size, Color.white);
        else if (m_shape == EditorToolShape.Sphere)
            m_cursorMesh.mesh = WireframeMesh.Sphere(size, Color.white);
    }

    void SetBlocks()
    {
        if(m_shape == EditorToolShape.Smooth)
        {
            SetBlocksSmooth();
            return;
        }    

        Vector3Int min = new Vector3Int(Mathf.Min(m_posStart.x, m_posEnd.x), Mathf.Min(m_posStart.y, m_posEnd.y), Mathf.Min(m_posStart.z, m_posEnd.z));
        Vector3Int max = new Vector3Int(Mathf.Max(m_posStart.x, m_posEnd.x), Mathf.Max(m_posStart.y, m_posEnd.y), Mathf.Max(m_posStart.z, m_posEnd.z));
        Vector3Int size = max - min + Vector3Int.one;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        Vector3Int minIndex = Grid.PosToChunkIndex(min);
        Vector3Int maxIndex = Grid.PosToChunkIndex(max);

        for(int i = minIndex.x; i <= maxIndex.x; i++)
        {
            for(int j = minIndex.y; j <= maxIndex.y; j++)
            {
                if (j < 0 || j >= grid.grid.Height())
                    continue;
                
                for(int k = minIndex.z; k <= maxIndex.z; k++)
                {
                    Vector3Int chunkPos = GridEx.GetPosFromLoop(grid.grid, new Vector3Int(i, j, k));
                    if (!grid.grid.LoopX() && chunkPos.x != i)
                        continue;
                    if (!grid.grid.LoopZ() && chunkPos.z != k)
                        continue;

                    var chunk = grid.grid.Get(chunkPos);

                    for(int x = 0; x < Grid.ChunkSize; x++)
                    {
                        for(int y = 0; y < Grid.ChunkSize; y++)
                        {
                            for(int z = 0; z < Grid.ChunkSize; z++)
                            {
                                Vector3Int pos = new Vector3Int(i * Grid.ChunkSize + x, j * Grid.ChunkSize + y, k * Grid.ChunkSize + z);
                                if (pos.x < min.x || pos.x > max.x || pos.y < min.y || pos.y > max.y || pos.z < min.z || pos.z > max.z)
                                    continue;

                                if (m_shape == EditorToolShape.Sphere && !IsInSphere(pos))
                                    continue;

                                var block = new Block(BlockType.ground);
                                if (!m_placeBlock)
                                {
                                    if (pos.y == 0)
                                        block.type = BlockType.water;
                                    else block.type = BlockType.air;
                                }

                                chunk.Set(x, y, z, block);
                            }
                        }
                    }
                }
            }
        }

        if(EditorGridBehaviour.instance != null)
            EditorGridBehaviour.instance.SetRegionDirty(new BoundsInt(min, size));
    }

    bool IsInSphere(Vector3Int pos)
    {
        Vector3Int min = new Vector3Int(Mathf.Min(m_posStart.x, m_posEnd.x), Mathf.Min(m_posStart.y, m_posEnd.y), Mathf.Min(m_posStart.z, m_posEnd.z));
        Vector3Int max = new Vector3Int(Mathf.Max(m_posStart.x, m_posEnd.x), Mathf.Max(m_posStart.y, m_posEnd.y), Mathf.Max(m_posStart.z, m_posEnd.z));
        Vector3Int size = max - min + Vector3Int.one;

        Vector3 center = new Vector3(size.x - 1, size.y - 1, size.z - 1) / 2;
        Vector3 circleSize = center + Vector3.one * 0.5f;

        Vector3 p = pos - min;
        p -= center;

        return WireframeMesh.IsPosOnSphere(p, circleSize);
    }

    void SetBlocksSmooth()
    {
        Vector3Int min = new Vector3Int(Mathf.Min(m_posStart.x, m_posEnd.x), Mathf.Min(m_posStart.y, m_posEnd.y), Mathf.Min(m_posStart.z, m_posEnd.z));
        Vector3Int max = new Vector3Int(Mathf.Max(m_posStart.x, m_posEnd.x), Mathf.Max(m_posStart.y, m_posEnd.y), Mathf.Max(m_posStart.z, m_posEnd.z));

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        int gridSize = GridEx.GetRealSize(grid.grid);
        int gridHeight = GridEx.GetRealHeight(grid.grid);

        if(!grid.grid.LoopX())
        {
            min.x = Mathf.Clamp(min.x, 0, gridSize - 1);
            max.x = Mathf.Clamp(max.x, 0, gridSize - 1);
        }
        if(!grid.grid.LoopZ())
        {
            min.z = Mathf.Clamp(min.z, 0, gridSize - 1);
            max.z = Mathf.Clamp(max.z, 0, gridSize - 1);
        }

        min.y = Mathf.Clamp(min.y, 0, gridHeight - 1);
        max.y = Mathf.Clamp(max.y, 0, gridHeight - 1);
        
        Vector3Int size = max - min + Vector3Int.one;
        if (size.x <= 0 || size.y <= 0 || size.z <= 0)
            return;

        Matrix<Block> lastBlocks = new Matrix<Block>(size.x, size.y, size.z);
        for(int i = 0; i < size.x; i++)
        {
            for(int j = 0; j < size.y; j++)
            {
                for(int k = 0; k < size.z; k++)
                {
                    Vector3Int pos = GridEx.GetRealPosFromLoop(grid.grid, min + new Vector3Int(i, j, k));
                    lastBlocks.Set(i, j, k, GridEx.GetBlock(grid.grid, pos));
                }
            }
        }

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    int total = 0;
                    int blocks = 0;

                    for(int x = -1; x<= 1; x++)
                    {
                        for(int y = -1; y <= 1; y++)
                        {
                            for(int z = -1; z <= 1; z++)
                            {
                                Vector3Int point = new Vector3Int(i + x, j + y, k + z);
                                if (point.x < 0 || point.x >= size.x || point.y < 0 || point.y >= size.y || point.z < 0 || point.z >= size.z)
                                    continue;

                                var block = lastBlocks.Get(point.x, point.y, point.z);
                                int blockNb = (x == 0 && y == 0 && z == 0) ? 2 : 1;

                                total += blockNb;
                                if (block.type == BlockType.ground)
                                    blocks += blockNb;
                            }
                        }
                    }

                    Vector3Int pos = GridEx.GetRealPosFromLoop(grid.grid, min + new Vector3Int(i, j, k));

                    Block b = new Block(BlockType.ground);
                    if(blocks < total / 2)
                    {
                        if (pos.y == 0)
                            b.type = BlockType.water;
                        else b.type = BlockType.air;
                    }

                    GridEx.SetBlock(grid.grid, pos, b);
                }
            }
        }

        if (EditorGridBehaviour.instance != null)
            EditorGridBehaviour.instance.SetRegionDirty(new BoundsInt(min, size));
    }

    void ScrollLocked(IsScrollLockedEvent e)
    {
        e.scrollLocked |= m_started;
    }
}
