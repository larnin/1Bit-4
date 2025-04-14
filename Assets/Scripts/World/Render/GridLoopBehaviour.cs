using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GridLoopBehaviour : MonoBehaviour
{
    class GridMesh
    {
        public List<Mesh> m_renderMesh = new List<Mesh>();
        public List<Material> m_materials = new List<Material>();
        public List<Mesh> m_colliders = new List<Mesh>();
    }

    class ChunkPoolMesh
    {
        public GameObject obj;
        public MeshRenderer renderer;
        public MeshFilter filter;
    }

    class ChunkPoolCollider
    {
        public GameObject obj;
        public MeshCollider collider;
    }

    class ChunkPoolCustomBlock
    {
        public BlockType type;
        public GameObject obj;
    }

    class RenderedChunk
    {
        public Vector3Int realPos;
        public Vector3Int gridPos;

        public List<ChunkPoolMesh> m_meshs = new List<ChunkPoolMesh>();
        public List<ChunkPoolCollider> m_colliders = new List<ChunkPoolCollider>();
        public List<ChunkPoolCustomBlock> m_blocks = new List<ChunkPoolCustomBlock>();
    }

    Grid m_grid;

    Matrix<GridMesh> m_meshs;
    List<ChunkRenderer> m_renderJobs = new List<ChunkRenderer>();

    List<ChunkPoolMesh> m_meshsPool = new List<ChunkPoolMesh>();
    List<ChunkPoolCollider> m_collidersPool = new List<ChunkPoolCollider>();
    Dictionary<BlockType, List<ChunkPoolCustomBlock>> m_blocksPool = new Dictionary<BlockType, List<ChunkPoolCustomBlock>>();

    Dictionary<ulong, RenderedChunk> m_renderers = new Dictionary<ulong, RenderedChunk>();

    SubscriberList m_subscriberList = new SubscriberList();

    int m_globalMeshIndex = 0;
    int m_globalColliderIndex = 0;
    int m_globalBlockIndex = 0;

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetGridEvent>.Subscriber(GetGrid));
        m_subscriberList.Add(new Event<SetGridEvent>.Subscriber(SetGrid));
        m_subscriberList.Add(new Event<CameraMoveEvent>.Subscriber(OnCameraMove));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public Grid GetGrid()
    {
        return m_grid;
    }

    void GetGrid(GetGridEvent e)
    {
        e.grid = m_grid;
    }

    void SetGrid(SetGridEvent e)
    {
        m_grid = e.grid;
        CreateMeshs();
    }

    void OnCameraMove(CameraMoveEvent e)
    {
        if (e.camera == null)
            return;

        UpdateGrid(e.camera);
    }

    void CreateMeshs()
    {
        int size = m_grid.Size();
        int height = m_grid.Height();

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < size; k++)
                {
                    var r = new ChunkRenderer(m_grid, new Vector3Int(i, j, k));
                    r.Start();
                    m_renderJobs.Add(r);
                }
            }
        }
    }

    private void Update()
    {
        UpdateWorkingJobs();
    }

    void UpdateWorkingJobs()
    {
        for(int i = 0; i < m_renderJobs.Count; i++)
        {
            if(m_renderJobs[i].Ended())
            {
                OnJobDone(m_renderJobs[i]);
                m_renderJobs.RemoveAt(i);
                i--;
            }
        }
    }

    void OnJobDone(ChunkRenderer r)
    {
        var pos = r.GetChunkIndex();
        var mesh = m_meshs.Get(pos.x, pos.y, pos.z);
        if(mesh == null)
        {
            mesh = new GridMesh();
            m_meshs.Set(pos.x, pos.y, pos.z, mesh);
        }

        foreach (var m in mesh.m_renderMesh)
            Destroy(m);
        mesh.m_renderMesh.Clear();

        foreach (var c in mesh.m_colliders)
            Destroy(c);
        mesh.m_colliders.Clear();

        // draw render
        mesh.m_materials = r.GetMaterials();
        foreach (var mat in mesh.m_materials)
        {
            int nbMesh = r.GetMeshCount(mat);
            for (int i = 0; i < nbMesh; i++)
            {
                var renderMesh = new Mesh();
                r.ApplyMesh(renderMesh, mat, i);
                mesh.m_renderMesh.Add(renderMesh);
            }
        }

        //draw colliders
        int nbColliders = r.GetColliderMeshCount();
        for (int i = 0; i < nbColliders; i++)
        {
            var colliderMesh = new Mesh();
            r.ApplyColliderMesh(colliderMesh, i);
            mesh.m_colliders.Add(colliderMesh);
        }

        RecreateChunksForPosition(pos);
    }

    void RecreateChunksForPosition(Vector3Int pos)
    {
        foreach(var r in m_renderers)
        {
            if(r.Value.gridPos == pos)
            {
                SetChunk(r.Value, pos);
            }
        }
    }

    void ClearChunk(RenderedChunk r)
    {
        foreach(var m in r.m_meshs)
        {
            m.obj.SetActive(false);
            m.filter.mesh = null;
            m_meshsPool.Add(m);
        }

        foreach(var c in r.m_colliders)
        {
            c.obj.SetActive(false);
            c.collider.sharedMesh = null;
            m_collidersPool.Add(c);
        }

        foreach(var b in r.m_blocks)
        {
            b.obj.SetActive(false);
            AddBlockToPool(b);
        }

        r.m_meshs.Clear();
        r.m_colliders.Clear();
        r.m_blocks.Clear();
    }

    void AddBlockToPool(ChunkPoolCustomBlock b)
    {
        b.obj.SetActive(false);

        List<ChunkPoolCustomBlock> list;
        if(!m_blocksPool.TryGetValue(b.type, out list))
        {
            list = new List<ChunkPoolCustomBlock>();
            m_blocksPool[b.type] = list;
        }
        list.Add(b);
    }

    void SetChunk(RenderedChunk r, Vector3Int pos)
    {
        ClearChunk(r);

        var gridPos = GridEx.GetPosFromLoop(m_grid, pos);
        r.gridPos = gridPos;
        r.realPos = pos;

        var mesh = m_meshs.Get(gridPos.x, gridPos.y, gridPos.z);
        if (mesh == null)
            return;

        Vector3 chunkPos = new Vector3(pos.x, pos.y, pos.z) * Grid.ChunkSize;

        for (int i = 0; i < mesh.m_renderMesh.Count; i++)
        {
            var m = GetOrCreateMesh();
            m.filter.mesh = mesh.m_renderMesh[i];
            m.renderer.sharedMaterial = mesh.m_materials[i];
            m.obj.transform.localPosition = chunkPos;
            r.m_meshs.Add(m);
        }

        for(int i = 0; i < mesh.m_colliders.Count; i++)
        {
            var c = GetOrCreateCollider();
            c.collider.sharedMesh = mesh.m_colliders[i];
            c.obj.transform.localPosition = chunkPos;
            r.m_colliders.Add(c);
        }

        var chunk = m_grid.Get(gridPos);

        for (int i = 0; i < Grid.ChunkSize; i++)
        {
            for (int j = 0; j < Grid.ChunkSize; j++)
            {
                for (int k = 0; k < Grid.ChunkSize; k++)
                {
                    var block = chunk.Get(i, j, k);
                    if (!Global.instance.blockDatas.IsCustomBlock(block))
                        continue;

                    var b = GetOrCreateBlock(block);
                    if (b == null)
                        continue;

                    b.obj.transform.localPosition = chunkPos + new Vector3(i, j, k);
                    r.m_blocks.Add(b);
                }
            }
        }
    }

    ChunkPoolMesh GetOrCreateMesh()
    {
        if(m_meshsPool.Count > 0)
        {
            var mPool = m_meshsPool[m_meshsPool.Count - 1];
            m_meshsPool.RemoveAt(m_meshsPool.Count - 1);
            return mPool;
        }

        ChunkPoolMesh m = new ChunkPoolMesh();
        m.obj = new GameObject("Mesh " + m_globalMeshIndex);
        m_globalMeshIndex++;
        m.obj.layer = gameObject.layer;
        m.obj.transform.parent = transform;
        m.obj.transform.localPosition = Vector3.zero;
        m.obj.transform.localRotation = Quaternion.identity;
        m.obj.transform.localScale = Vector3.one;

        m.renderer = m.obj.AddComponent<MeshRenderer>();

        m.filter = m.obj.AddComponent<MeshFilter>();

        return m;
    }

    ChunkPoolCollider GetOrCreateCollider()
    {
        if(m_collidersPool.Count > 0)
        {
            var cPool = m_collidersPool[m_collidersPool.Count - 1];
            m_collidersPool.RemoveAt(m_collidersPool.Count - 1);
            return cPool;
        }

        ChunkPoolCollider c = new ChunkPoolCollider();
        c.obj = new GameObject("Collider " + m_globalColliderIndex);
        m_globalColliderIndex++;
        c.obj.layer = gameObject.layer;
        c.obj.transform.parent = transform;
        c.obj.transform.localPosition = Vector3.zero;
        c.obj.transform.localRotation = Quaternion.identity;
        c.obj.transform.localScale = Vector3.one;

        c.collider = c.obj.AddComponent<MeshCollider>();
        c.collider.convex = false;

        return c;
    }

    ChunkPoolCustomBlock GetOrCreateBlock(BlockType b)
    {
        List<ChunkPoolCustomBlock> list;
        if (m_blocksPool.TryGetValue(b, out list))
        {
            if(list.Count > 0)
            {
                var blockPool = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return blockPool;
            }
        }

        var blockData = Global.instance.blockDatas.GetCustomBlock(b);
        if (blockData == null)
            return null;

        ChunkPoolCustomBlock block = new ChunkPoolCustomBlock();
        block.obj.name = b.ToString() + " " + m_globalBlockIndex;
        m_globalBlockIndex++;
        block.obj = Instantiate(blockData.prefab);
        block.obj.transform.parent = transform;
        block.obj.transform.localPosition = Vector3.zero;
        block.obj.transform.localRotation = Quaternion.identity;
        block.obj.transform.localScale = Vector3.one;
        block.type = b;

        return block;
    }

    void UpdateGrid(Camera c)
    {
        var box = GetFrustrumBox(c);
        var boxMin = box.min;
        var boxMax = box.max;

        var min = Grid.PosToChunkIndex(new Vector3Int(Mathf.RoundToInt(boxMin.x), Mathf.RoundToInt(boxMin.y), Mathf.RoundToInt(boxMin.z)));
        var max = Grid.PosToChunkIndex(new Vector3Int(Mathf.RoundToInt(boxMax.x), Mathf.RoundToInt(boxMax.y), Mathf.RoundToInt(boxMax.z)));

        //Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        var planes = GeometryUtility.CalculateFrustumPlanes(c);
        Vector4[] planeVects = new Vector4[4];
        for(int i = 0; i < 4; i++)
        {
            var normal = planes[i].normal;
            planeVects[i] = new Vector4(normal.x, normal.y, normal.z, planes[i].distance);
        }

        List<ulong> toRemoveIDs = new List<ulong>();

        foreach(var chunk in m_renderers)
        {
            var pos = chunk.Value.realPos;

            if (!IsChunkOnFrustrum(planeVects, pos))
                toRemoveIDs.Add(chunk.Key);
        }

        foreach(var ID in toRemoveIDs)
        {
            ClearChunk(m_renderers[ID]);
            m_renderers.Remove(ID);
        }

        for(int i = min.x; i <= max.x; i++)
        {
            for(int j = min.y; j <= max.y; j++)
            {
                for(int k = min.z; k <= max.z; k++)
                {
                    if (!m_grid.LoopX() && (i < 0 || i >= m_grid.Size()))
                        continue;

                    if (!m_grid.LoopY() && (k < 0 || k >= m_grid.Size()))
                        continue;

                    var ID = Utility.PosToID(new Vector3Int(i, j, k));

                    if (m_renderers.ContainsKey(ID))
                        continue;

                    RenderedChunk chunk = new RenderedChunk();
                    SetChunk(chunk, new Vector3Int(i, j, k));

                    m_renderers.Add(ID, chunk);
                }
            }
        }
    }

    bool IsChunkOnFrustrum(Vector4[] planes, Vector3Int box)
    {
        var min = new Vector3(box.x, box.y, box.z) * Grid.ChunkSize - new Vector3(0.5f, 0.5f, 0.5f);
        var max = new Vector3(box.x + 1, box.y + 1, box.z + 1) * Grid.ChunkSize - new Vector3(0.5f, 0.5f, 0.5f);

        for (int i = 0; i < 4; i++)
        {
            int v = 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, min.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, min.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, max.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, max.y, min.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, min.y, max.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, min.y, max.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(min.x, max.y, max.z, 1.0f)) < 0) ? 1 : 0;
            v += (Vector4.Dot(planes[i], new Vector4(max.x, max.y, max.z, 1.0f)) < 0) ? 1 : 0;
            if (v == 8)
                return false;
        }

        return true;
    }

    Bounds GetFrustrumBox(Camera c)
    {
        float screenWidth = Screen.width - 1;
        float screenHeight = Screen.height - 1;
        var rays = new Ray[] { 
            c.ScreenPointToRay(Vector3.zero), 
            c.ScreenPointToRay(new Vector3(screenWidth, 0, 0)), 
            c.ScreenPointToRay(new Vector3(0, screenHeight, 0)),
            c.ScreenPointToRay(new Vector3(screenWidth, screenHeight, 0))};

        Bounds b = new Bounds();
        bool boundsSet = false;

        int height = GridEx.GetRealHeight(m_grid);

        var planes = new Plane[] { new Plane(Vector3.up, new Vector3(0, -0.5f, 0)), new Plane(Vector3.up, new Vector3(0, height - 0.5f, 0)) };

        foreach(var ray in rays)
        {
            foreach(var p in planes)
            {
                float enter;
                if (p.Raycast(ray, out enter))
                {
                    var pos = ray.GetPoint(enter);
                    if (!boundsSet)
                        b = new Bounds(pos, Vector3.zero);
                    else b.Encapsulate(pos);
                    boundsSet = true;
                }
            }
        }

        return b;
    }
}
