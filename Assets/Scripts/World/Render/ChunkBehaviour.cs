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
    List<ChunkRenderer> m_oldRenderers = new List<ChunkRenderer>();

    List<GameObject> m_renders = new List<GameObject>();
    List<GameObject> m_colliders = new List<GameObject>();
    List<GameObject> m_customBlocks = new List<GameObject>();

    public void SetChunk(Grid grid, Vector3Int index)
    {
        m_grid = grid;
        m_index = index;

        StartGeneration();
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

    public bool Generated()
    {
        return m_renderer == null;
    }

    private void Update()
    {
        if(m_renderer != null && m_renderer.Ended())
        {
            OnRenderEnd(m_renderer);
            m_renderer = null;
            m_oldRenderers.Clear();
        }

        for(int i = m_oldRenderers.Count - 1; i >= 0; i--)
        {
            if(m_oldRenderers[i].Ended())
            {
                OnRenderEnd(m_oldRenderers[i]);
                for (int j = 0; j <= i; j++)
                    m_oldRenderers.RemoveAt(0);
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
}
