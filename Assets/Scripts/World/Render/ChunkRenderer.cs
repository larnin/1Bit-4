using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderer
{
    Grid m_grid;
    Vector3Int m_chunkIndex;

    MeshParams<WorldVertexDefinition, ColliderVertexDefinition> m_meshParams;

    bool m_ended = false;
    bool m_working = false;

    public ChunkRenderer(Grid grid, Vector3Int index)
    {
        m_grid = grid;
        m_chunkIndex = index;
    }

    public Vector3Int GetChunkIndex()
    {
        return m_chunkIndex;
    }

    public void Start()
    {
        if (m_grid == null)
            return;

        if (m_working)
            return;

        m_working = true;
        m_ended = false;

        ThreadPool.StartJob(JobWorker, OnEndJob, 10, this);
    }

    public bool Ended()
    {
        return m_ended && !m_working;
    }

    public List<Material> GetMaterials()
    {
        return m_meshParams.GetNonEmptyMaterials();
    }

    public int GetMeshCount(Material material)
    {
        return m_meshParams.GetMeshCount(material);
    }

    public void ApplyMesh(Mesh mesh, Material material, int index)
    {
        var data = m_meshParams.GetMesh(material, index);

        MeshEx.SetWorldMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        //full chunk layer
        mesh.bounds = new Bounds(new Vector3(Grid.ChunkSize, Grid.ChunkSize, Grid.ChunkSize) / 2, new Vector3(Grid.ChunkSize, Grid.ChunkSize, Grid.ChunkSize));
    }

    public int GetColliderMeshCount()
    {
        return m_meshParams.GetColliderMeshCount();
    }

    public void ApplyColliderMesh(Mesh mesh, int index)
    {
        var data = m_meshParams.GetColliderMesh(index);

        MeshEx.SetColliderMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Triangles));

        //full chunk layer
        mesh.bounds = new Bounds(new Vector3(Grid.ChunkSize, Grid.ChunkSize, Grid.ChunkSize) / 2, new Vector3(Grid.ChunkSize, Grid.ChunkSize, Grid.ChunkSize));
    }

    void JobWorker()
    {
        m_meshParams = new MeshParams<WorldVertexDefinition, ColliderVertexDefinition>();

        var mat = new Matrix<BlockType>(Grid.ChunkSize + 2, Grid.ChunkSize + 2, Grid.ChunkSize + 2);
        var initialPos = Grid.PosInChunkToPos(m_chunkIndex, new Vector3Int(-1, -1, -1));

        GridEx.GetLocalMatrix(m_grid, initialPos, mat);

        NearMatrix3<BlockType> localMat = new NearMatrix3<BlockType>();

        for (int i = 0; i < Grid.ChunkSize; i++)
        {
            for(int j = 0; j < Grid.ChunkSize; j++)
            {
                for(int k = 0; k < Grid.ChunkSize; k++)
                {
                    for(int x = -1; x <= 1; x++)
                    {
                        for(int y = -1; y <= 1; y++)
                        {
                            for(int z = -1; z <= 1; z++)
                            {
                                var block = mat.Get(i + x + 1, j + y + 1, k + z + 1);
                                localMat.Set(block, x, y, z);
                            }
                        }
                    }

                    DrawBlock(new Vector3Int(i, j, k), localMat);
                }
            }
        }

        CopyRenderToCollider();
    }

    void OnEndJob()
    {
        m_working = false;
        m_ended = true;
    }

    void DrawBlock(Vector3Int pos, NearMatrix3<BlockType> mat)
    {
        var type = mat.Get(0, 0, 0);
        if (!CanRender(type))
            return;

        if (!CanRender(mat.Get(0, 1, 0)))
            DrawFace(pos, BlockFace.Top, mat);

        if (!CanRender(mat.Get(0, -1, 0)))
            DrawFace(pos, BlockFace.Bottom, mat);

        if (!CanRender(mat.Get(0, 0, 1)))
            DrawFace(pos, BlockFace.Front, mat);

        if (!CanRender(mat.Get(0, 0, -1)))
            DrawFace(pos, BlockFace.Back, mat);

        if (!CanRender(mat.Get(1, 0, 0)))
            DrawFace(pos, BlockFace.Right, mat);

        if (!CanRender(mat.Get(-1, 0, 0)))
            DrawFace(pos, BlockFace.Left, mat);
    }

    void DrawFace(Vector3Int pos, BlockFace face, NearMatrix3<BlockType> mat)
    {
        var type = mat.Get(0, 0, 0);
        var material = type == BlockType.water ? Global.instance.blockDatas.waterMaterial : Global.instance.blockDatas.defaultMaterial;

        var data = m_meshParams.Allocate(4, 6, material);

        int startIndex = data.indexesSize;
        int startVertice = data.verticesSize;

        data.vertices[startVertice].pos = new Vector3(-0.5f, -0.5f, -0.5f);
        data.vertices[startVertice].uv = new Vector2(0, 0);
        data.vertices[startVertice + 1].pos = new Vector3(-0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 1].uv = new Vector2(0, 1);
        data.vertices[startVertice + 2].pos = new Vector3(0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 2].uv = new Vector2(1, 1);
        data.vertices[startVertice + 3].pos = new Vector3(0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 3].uv = new Vector2(1, 0);

        TransformFace(data.vertices, startVertice, face);
        MoveFace(data.vertices, startVertice, pos);

        data.verticesSize += 4;

        data.indexes[startIndex] = (ushort)startVertice;
        data.indexes[startIndex + 1] = (ushort)(startVertice + 3);
        data.indexes[startIndex + 2] = (ushort)(startVertice + 1);
        data.indexes[startIndex + 3] = (ushort)(startVertice + 1);
        data.indexes[startIndex + 4] = (ushort)(startVertice + 3);
        data.indexes[startIndex + 5] = (ushort)(startVertice + 2);

        data.indexesSize += 6;

        BakeNormals(data, startIndex, 2);
        BakeTangents(data, startIndex, 2);
    }

    void TransformFace(WorldVertexDefinition[] vertices, int index, BlockFace face)
    {
        if (face == BlockFace.Bottom)
            return;

        for(int i = index; i < index + 4; i++)
        {
            if (face == BlockFace.Top)
                vertices[i].pos = Quaternion.Euler(180, 0, 0) * vertices[i].pos;
            else if (face == BlockFace.Left)
                vertices[i].pos = Quaternion.Euler(90, 90, 0) * vertices[i].pos;
            else if(face == BlockFace.Front)
                vertices[i].pos = Quaternion.Euler(90, 180, 0) * vertices[i].pos;
            else if (face == BlockFace.Right)
                vertices[i].pos = Quaternion.Euler(90, 270, 0) * vertices[i].pos;
            else //back
                vertices[i].pos = Quaternion.Euler(90, 0, 0) * vertices[i].pos;
        }
    }

    void MoveFace(WorldVertexDefinition[] vertices, int index, Vector3 offset)
    {
        for (int i = index; i < index + 4; i++)
            vertices[i].pos += offset;
    }

    static void BakeNormals(MeshParamData<WorldVertexDefinition> data, int index, int triangleNb)
    {
        //https://math.stackexchange.com/questions/305642/how-to-find-surface-normal-of-a-triangle

        for (int i = 0; i < triangleNb; i++)
        {
            int i1 = data.indexes[index + i * 3];
            int i2 = data.indexes[index + i * 3 + 1];
            int i3 = data.indexes[index + i * 3 + 2];

            var p1 = data.vertices[i1].pos;
            var p2 = data.vertices[i2].pos;
            var p3 = data.vertices[i3].pos;

            var v = p2 - p1;
            var w = p3 - p1;

            var n = new Vector3(v.y * w.z - v.z * w.y, v.z * w.x - v.x * w.z, v.x * w.y - v.y * w.x);

            data.vertices[i1].normal = n;
            data.vertices[i2].normal = n;
            data.vertices[i3].normal = n;
        }
    }

    static void BakeTangents(MeshParamData<WorldVertexDefinition> data, int index, int triangleNb)
    {
        //https://forum.unity.com/threads/how-to-calculate-mesh-tangents.38984/#post-285069
        //with a small simplification : 
        // Each vertex are linked to only one triangle. If one vertex is on multiple triangle, the combined surface is flat, we don't need to combine tangent

        for (int i = 0; i < triangleNb; i++)
        {
            int i1 = data.indexes[index + i * 3];
            int i2 = data.indexes[index + i * 3 + 1];
            int i3 = data.indexes[index + i * 3 + 2];

            var v1 = data.vertices[i1];
            var v2 = data.vertices[i2];
            var v3 = data.vertices[i3];

            float x1 = v2.pos.x - v1.pos.x;
            float x2 = v3.pos.x - v1.pos.x;
            float y1 = v2.pos.y - v1.pos.y;
            float y2 = v3.pos.y - v1.pos.y;
            float z1 = v2.pos.z - v1.pos.z;
            float z2 = v3.pos.z - v1.pos.z;
            float s1 = v2.uv.x - v1.uv.x;
            float s2 = v3.uv.x - v1.uv.x;
            float t1 = v2.uv.y - v1.uv.y;
            float t2 = v3.uv.y - v1.uv.y;

            float r = 1.0f / (s1 * t2 - s2 * t1);
            var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            var tmp = (sdir - v1.normal * Vector3.Dot(v1.normal, sdir)).normalized;
            var w = (Vector3.Dot(Vector3.Cross(v1.normal, sdir), tdir) < 0.0f) ? -1.0f : 1.0f;

            var tan = new Vector4(tmp.x, tmp.y, tmp.z, w);

            data.vertices[i1].tangent = tan;
            data.vertices[i2].tangent = tan;
            data.vertices[i3].tangent = tan;
        }
    }

    void CopyRenderToCollider()
    {
        var mats = m_meshParams.GetNonEmptyMaterials();
        foreach(var mat in mats)
        {
            int nbMesh = m_meshParams.GetMeshCount(mat);

            for(int i = 0; i < nbMesh; i++)
            {
                var meshData = m_meshParams.GetMesh(mat, i);
                
                var data = m_meshParams.AllocateCollider(meshData.verticesSize, meshData.indexesSize);

                for(int v = 0; v < meshData.verticesSize; v++)
                {
                    data.vertices[v + data.verticesSize].pos = meshData.vertices[v].pos;
                    data.vertices[v + data.verticesSize].normal = meshData.vertices[v].normal;
                }

                for(int index = 0; index < meshData.indexesSize; index++)
                    data.indexes[index + data.indexesSize] = (ushort)(meshData.indexes[index] + data.verticesSize);

                data.verticesSize += meshData.verticesSize;
                data.indexesSize += meshData.indexesSize;
            }
        }
    }

    bool CanRender(BlockType type)
    {
        if (type == BlockType.air)
            return false;

        if (Global.instance.blockDatas.IsCustomBlock(type))
            return false;

        return true;
    }
}
