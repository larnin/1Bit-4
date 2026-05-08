using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkBehaviour : MonoBehaviour
{
    Grid m_grid;
    Vector3Int m_index;

    ChunkRenderer m_renderer;
    WaterRenderer m_waterRenderer;
    List<ChunkRenderer> m_oldRenderers = new List<ChunkRenderer>();
    List<WaterRenderer> m_oldWaterRenderers = new List<WaterRenderer>();

    List<GameObject> m_renders = new List<GameObject>();
    Texture m_waterTexture = null;
    Material m_waterMaterial = null;
    GameObject m_waterSurface = null;
    List<GameObject> m_colliders = new List<GameObject>();
    List<GameObject> m_customBlocks = new List<GameObject>();

    public void SetChunk(Grid grid, Vector3Int index)
    {
        m_grid = grid;
        m_index = index;

        StartGeneration();
        StartWaterGeneration();
    }

    void StartGeneration()
    {
        if(m_renderer != null)
        {
            if (!m_renderer.IsGenerating())
                return;

            m_oldRenderers.Add(m_renderer);
            m_renderer = null;
        }

        m_renderer = new ChunkRenderer(m_grid, m_index);
        m_renderer.Start();   
    }

    void StartWaterGeneration()
    {
        if (m_index.y != 0)
            return;

        if(m_waterRenderer != null)
        {
            if (!m_waterRenderer.IsGenerating())
                return;

            m_oldWaterRenderers.Add(m_waterRenderer);
            m_waterRenderer = null;
        }

        m_waterRenderer = new WaterRenderer(m_grid, m_index);
        m_waterRenderer.Start();
    }

    public bool Generated()
    {
        return m_renderer == null && m_waterRenderer == null;
    }

    private void Update()
    {
        UpdateRenderer();
        UpdateWaterRenderer();
    }

    private void OnDestroy()
    {
        if(CustomLightsManager.instance != null && m_waterMaterial != null)
            CustomLightsManager.instance.UnRegisterMaterial(m_waterMaterial, true);
    }

    void UpdateRenderer()
    {
        if (m_renderer != null && m_renderer.Ended())
        {
            OnRenderEnd(m_renderer);
            m_renderer = null;
            m_oldRenderers.Clear();
        }

        for (int i = m_oldRenderers.Count - 1; i >= 0; i--)
        {
            if (m_oldRenderers[i].Ended())
            {
                OnRenderEnd(m_oldRenderers[i]);
                for (int j = 0; j <= i; j++)
                    m_oldRenderers.RemoveAt(0);
                break;
            }
        }
    }

    void UpdateWaterRenderer()
    {
        if (m_waterRenderer != null && m_waterRenderer.Ended())
        {
            OnWaterRenderEnd(m_waterRenderer);
            m_waterRenderer = null;
            m_oldWaterRenderers.Clear();
        }

        for (int i = m_oldWaterRenderers.Count - 1; i >= 0; i--)
        {
            if(m_oldWaterRenderers[i].Ended())
            {
                OnWaterRenderEnd(m_oldWaterRenderers[i]);
                for (int j = 0; j <= i; j++)
                    m_oldWaterRenderers.RemoveAt(0);
                break;
            }
        }
    }

    void OnRenderEnd(ChunkRenderer renderer)
    {
        //clean all
        foreach (var r in m_renders)
            Destroy(r);
        m_renders.Clear();

        foreach (var c in m_colliders)
            Destroy(c);
        m_colliders.Clear();

        foreach (var b in m_customBlocks)
            Destroy(b);
        m_customBlocks.Clear();

        // draw render
        int index = 0;
        var mats = renderer.GetMaterials();
        foreach(var mat in mats)
        {
            int nbMesh = renderer.GetMeshCount(mat);
            for(int i = 0; i < nbMesh; i++)
            {
                var obj = new GameObject("Mesh " + index);
                index++;
                obj.layer = gameObject.layer;
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                var mesh = new Mesh();
                renderer.ApplyMesh(mesh, mat, i);

                var meshRenderer = obj.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = mat;

                var filter = obj.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                m_renders.Add(obj);
            }
        }

        //draw colliders
        int nbColliders = renderer.GetColliderMeshCount();
        for(int i = 0; i < nbColliders; i++)
        {
            var obj = new GameObject("Collider " + i);
            obj.layer = gameObject.layer;
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            var mesh = new Mesh();
            renderer.ApplyColliderMesh(mesh, i);

            var collider = obj.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = false;

            m_colliders.Add(obj);
        }

        InstantiateCustomBlocks();
    }

    void InstantiateCustomBlocks()
    {
        var chunk = m_grid.Get(m_index);

        for(int i = 0; i < Grid.ChunkSize; i++)
        {
            for(int j = 0; j < Grid.ChunkSize; j++)
            {
                for(int k = 0; k < Grid.ChunkSize; k++)
                {
                    var b = Global.instance.blockDatas.GetCustomBlock(chunk.Get(i, j, k).type);
                    if (b == null)
                        continue;

                    var obj = Instantiate(b.prefab);
                    obj.transform.parent = transform;
                    obj.transform.localPosition = new Vector3(i, j, k);
                    Vector3Int realPos = Grid.PosInChunkToPos(m_index, new Vector3Int(i, j, k));
                    obj.transform.localRotation = RotationEx.ToQuaternion(RotationEx.RandomRotation(realPos));
                    obj.transform.localScale = Vector3.one;
                    m_customBlocks.Add(obj);
                }
            }
        }
    }

    void OnWaterRenderEnd(WaterRenderer renderer)
    {
        if (m_waterSurface == null)
            MakeWaterSurface();

        if (m_waterTexture != null)
            Destroy(m_waterTexture);
        m_waterTexture = renderer.GetTexture();
        if (m_waterMaterial != null)
            m_waterMaterial.SetTexture("_ShoreTex", m_waterTexture);
    }

    void MakeWaterSurface()
    {
        m_waterSurface = new GameObject("Water");
        m_waterSurface.layer = gameObject.layer;
        m_waterSurface.transform.parent = transform;
        m_waterSurface.transform.localPosition = Vector3.zero;
        m_waterSurface.transform.localRotation = Quaternion.identity;
        m_waterSurface.transform.localScale = Vector3.one;

        float b = (float)(WaterRenderer.border * WaterRenderer.pixelPerBlock) / (WaterRenderer.TextureWidth());

        float h = 0.25f;
        SimpleMeshParam<WorldVertexDefinition> meshParams = new SimpleMeshParam<WorldVertexDefinition>();
        var data = meshParams.Allocate(4, 6);
        int v = data.verticesSize;
        data.vertices[v].pos = new Vector3(-0.5f, h, -0.5f);
        data.vertices[v].uv = new Vector2(b, b);
        data.vertices[v + 1].pos = new Vector3(Grid.ChunkSize - 0.5f, h, -0.5f);
        data.vertices[v + 1].uv = new Vector2(1 - b, b);
        data.vertices[v + 2].pos = new Vector3(Grid.ChunkSize - 0.5f, h, Grid.ChunkSize - 0.5f);
        data.vertices[v + 2].uv = new Vector2(1 - b, 1 - b);
        data.vertices[v + 3].pos = new Vector3(-0.5f, h, Grid.ChunkSize - 0.5f);
        data.vertices[v + 3].uv = new Vector2(b, 1 - b);

        for (int i = v; i < v + 4; i++)
        {
            data.vertices[i].normal = Vector3.up;
            data.vertices[i].tangent = new Vector4(1, 0, 0, 0);
            data.vertices[i].color = new Color32(255, 255, 255, 255);
        }

        int p = data.indexesSize;
        data.indexes[p] = (ushort)v;
        data.indexes[p + 1] = (ushort)(v + 2);
        data.indexes[p + 2] = (ushort)(v + 1);
        data.indexes[p + 3] = (ushort)v;
        data.indexes[p + 4] = (ushort)(v + 3);
        data.indexes[p + 5] = (ushort)(v + 2);

        data.verticesSize += 4;
        data.indexesSize += 6;

        var mesh = new Mesh();
        MeshEx.SetWorldMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        //full chunk layer
        mesh.bounds = new Bounds(new Vector3(Grid.ChunkSize, Grid.ChunkSize, Grid.ChunkSize) / 2, new Vector3(Grid.ChunkSize, Grid.ChunkSize, Grid.ChunkSize));

        var meshRenderer = m_waterSurface.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Global.instance.blockDatas.waterMaterial);
        if (CustomLightsManager.instance != null)
            CustomLightsManager.instance.RegisterMaterial(meshRenderer.sharedMaterial, true);
        m_waterMaterial = meshRenderer.sharedMaterial;

        var filter = m_waterSurface.AddComponent<MeshFilter>();
        filter.mesh = mesh;
    }
}
