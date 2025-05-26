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

        var mat = new Matrix<Block>(Grid.ChunkSize + 2, Grid.ChunkSize + 2, Grid.ChunkSize + 2);
        var initialPos = Grid.PosInChunkToPos(m_chunkIndex, new Vector3Int(-1, -1, -1));

        GridEx.GetLocalMatrix(m_grid, initialPos, mat);

        NearMatrix3<Block> localMat = new NearMatrix3<Block>();

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

    void DrawBlock(Vector3Int pos, NearMatrix3<Block> mat)
    {
        if (!CanRender(mat.Get(0, 0, 0).type))
            return;

        if (BlockEx.IsComplexeBlock(mat.Get(0, 0, 0).type))
        {
            DrawComplexeBlock(pos, mat);
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            var face = (BlockFace)i;
            var oppositeFace = BlockEx.OppositeFace(face);

            Vector3Int offset = -BlockEx.BlockFaceToDirection(oppositeFace);

            if (!IsFullFace(mat.Get(offset.x, offset.y, offset.z), oppositeFace))
                DrawFace(pos, face, mat);
        }
    }

    void DrawComplexeBlock(Vector3Int pos, NearMatrix3<Block> mat)
    {
        var current = mat.Get(0, 0, 0);
        for (int i = 0; i < 6; i++)
        {
            var face = (BlockFace)i;
            var oppositeFace = BlockEx.OppositeFace(face);
            
            Vector3Int offset = -BlockEx.BlockFaceToDirection(oppositeFace);

            if (IsFullFace(current, face) && !IsFullFace(mat.Get(offset.x, offset.y, offset.z), oppositeFace))
                DrawFace(pos, face, mat);
        }

        var rot = BlockEx.GetRotationFromData(current.data);
        var shape = BlockEx.GetShapeFromData(current.data);

        int verticeNb = 0;
        int triangleNb = 0;
        MeshParamData<WorldVertexDefinition> data = null;
        switch (shape)
        {
            case BlockShape.CornerL:
                DrawCornerL(out data, out verticeNb, out triangleNb);
                break;
            case BlockShape.HalfTriangle:
                DrawHalfTriangle(out data, out verticeNb, out triangleNb);
                break;
            case BlockShape.CornerS:
                DrawCornerS(out data, out verticeNb, out triangleNb);
                break;
            default: //nothing more to do
                return;
        }

        TransformVertices(data.vertices, data.verticesSize, verticeNb, BlockRotationToQuaternion(rot));
        MoveVertices(data.vertices, data.verticesSize, verticeNb, pos);

        BakeNormals(data, data.indexesSize, triangleNb);
        BakeTangents(data, data.indexesSize, triangleNb);

        data.verticesSize += verticeNb;
        data.indexesSize += triangleNb * 3;
    }

    void DrawCornerL(out MeshParamData<WorldVertexDefinition> data, out int verticeNb, out int triangleNb)
    {
        // 4 triangles
        verticeNb = 12;
        triangleNb = 4;

        var material = Global.instance.blockDatas.defaultMaterial;

        data = m_meshParams.Allocate(verticeNb, triangleNb * 3, material);

        int startIndex = data.indexesSize;
        int startVertice = data.verticesSize;

        data.vertices[startVertice].pos = new Vector3(0.5f, 0.5f, -0.5f);
        data.vertices[startVertice].uv = new Vector2(0, 0);
        data.vertices[startVertice + 1].pos = new Vector3(0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 1].uv = new Vector2(0, 1);
        data.vertices[startVertice + 2].pos = new Vector3(0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 2].uv = new Vector2(1, 0);

        data.vertices[startVertice + 3].pos = new Vector3(0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 3].uv = new Vector2(0, 0);
        data.vertices[startVertice + 4].pos = new Vector3(-0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 4].uv = new Vector2(0, 1);
        data.vertices[startVertice + 5].pos = new Vector3(-0.5f, 0.5f, 0.5f);
        data.vertices[startVertice + 5].uv = new Vector2(1, 0);

        data.vertices[startVertice + 6].pos = new Vector3(0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 6].uv = new Vector2(0, 0);
        data.vertices[startVertice + 7].pos = new Vector3(-0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 7].uv = new Vector2(0, 1);
        data.vertices[startVertice + 8].pos = new Vector3(-0.5f, 0.5f, 0.5f);
        data.vertices[startVertice + 8].uv = new Vector2(1, 0);

        data.vertices[startVertice + 9].pos = new Vector3(0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 9].uv = new Vector2(0, 0);
        data.vertices[startVertice + 10].pos = new Vector3(0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 10].uv = new Vector2(0, 1);
        data.vertices[startVertice + 11].pos = new Vector3(-0.5f, 0.5f, 0.5f);
        data.vertices[startVertice + 11].uv = new Vector2(1, 0);

        for (int i = 0; i < triangleNb * 3; i++)
            data.indexes[startIndex + i] = (ushort)(startVertice + i);
    }

    void DrawHalfTriangle(out MeshParamData<WorldVertexDefinition> data, out int verticeNb, out int triangleNb)
    {
        // 1 rect & 2 triangles
        verticeNb = 10;
        triangleNb = 4;

        var material = Global.instance.blockDatas.defaultMaterial;

        data = m_meshParams.Allocate(verticeNb, triangleNb * 3, material);

        int startIndex = data.indexesSize;
        int startVertice = data.verticesSize;

        data.vertices[startVertice].pos = new Vector3(0.5f, 0.5f, -0.5f);
        data.vertices[startVertice].uv = new Vector2(0, 0);
        data.vertices[startVertice + 1].pos = new Vector3(0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 1].uv = new Vector2(0, 1);
        data.vertices[startVertice + 2].pos = new Vector3(0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 2].uv = new Vector2(1, 0);

        data.vertices[startVertice + 3].pos = new Vector3(-0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 3].uv = new Vector2(0, 0);
        data.vertices[startVertice + 4].pos = new Vector3(-0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 4].uv = new Vector2(0, 1);
        data.vertices[startVertice + 5].pos = new Vector3(-0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 5].uv = new Vector2(1, 0);

        data.vertices[startVertice + 6].pos = new Vector3(0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 6].uv = new Vector2(0, 0);
        data.vertices[startVertice + 7].pos = new Vector3(0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 7].uv = new Vector2(0, 1);
        data.vertices[startVertice + 8].pos = new Vector3(-0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 8].uv = new Vector2(1, 1);
        data.vertices[startVertice + 9].pos = new Vector3(-0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 9].uv = new Vector2(1, 0);

        for (int i = 0; i < 6; i++)
            data.indexes[startIndex + i] = (ushort)(startVertice + i);

        startVertice += 6;
        startIndex += 6;

        data.indexes[startIndex] = (ushort)startVertice;
        data.indexes[startIndex + 1] = (ushort)(startVertice + 3);
        data.indexes[startIndex + 2] = (ushort)(startVertice + 1);
        data.indexes[startIndex + 3] = (ushort)(startVertice + 1);
        data.indexes[startIndex + 4] = (ushort)(startVertice + 3);
        data.indexes[startIndex + 5] = (ushort)(startVertice + 2);
    }

    void DrawCornerS(out MeshParamData<WorldVertexDefinition> data, out int verticeNb, out int triangleNb)
    {
        // 4 triangles
        verticeNb = 12;
        triangleNb = 4;

        var material = Global.instance.blockDatas.defaultMaterial;

        data = m_meshParams.Allocate(verticeNb, triangleNb * 3, material);

        int startIndex = data.indexesSize;
        int startVertice = data.verticesSize;

        data.vertices[startVertice].pos = new Vector3(-0.5f, 0.5f, -0.5f);
        data.vertices[startVertice].uv = new Vector2(0, 0);
        data.vertices[startVertice + 1].pos = new Vector3(-0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 1].uv = new Vector2(0, 1);
        data.vertices[startVertice + 2].pos = new Vector3(-0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 2].uv = new Vector2(1, 0);

        data.vertices[startVertice + 3].pos = new Vector3(0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 3].uv = new Vector2(0, 0);
        data.vertices[startVertice + 4].pos = new Vector3(-0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 4].uv = new Vector2(0, 1);
        data.vertices[startVertice + 5].pos = new Vector3(-0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 5].uv = new Vector2(1, 0);

        data.vertices[startVertice + 6].pos = new Vector3(0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 6].uv = new Vector2(0, 0);
        data.vertices[startVertice + 7].pos = new Vector3(-0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 7].uv = new Vector2(0, 1);
        data.vertices[startVertice + 8].pos = new Vector3(-0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 8].uv = new Vector2(1, 0);

        data.vertices[startVertice + 9].pos = new Vector3(0.5f, -0.5f, -0.5f);
        data.vertices[startVertice + 9].uv = new Vector2(0, 0);
        data.vertices[startVertice + 10].pos = new Vector3(-0.5f, 0.5f, -0.5f);
        data.vertices[startVertice + 10].uv = new Vector2(0, 1);
        data.vertices[startVertice + 11].pos = new Vector3(-0.5f, -0.5f, 0.5f);
        data.vertices[startVertice + 11].uv = new Vector2(1, 0);

        for (int i = 0; i < triangleNb * 3; i++)
            data.indexes[startIndex + i] = (ushort)(startVertice + i);
    }

    void DrawFace(Vector3Int pos, BlockFace face, NearMatrix3<Block> mat)
    {
        var type = mat.Get(0, 0, 0).type;
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

    void TransformVertices(WorldVertexDefinition[] vertices, int index, int count, Quaternion rot)
    {
        for (int i = index; i < index + count; i++)
            vertices[i].pos = rot * vertices[i].pos;
    }

    void MoveFace(WorldVertexDefinition[] vertices, int index, Vector3 offset)
    {
        for (int i = index; i < index + 4; i++)
            vertices[i].pos += offset;
    }

    void MoveVertices(WorldVertexDefinition[] vertices, int index, int count, Vector3 offset)
    {
        for (int i = index; i < index + count; i++)
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

    static bool CanRender(BlockType type)
    {
        if (type == BlockType.air)
            return false;

        if (Global.instance.blockDatas.IsCustomBlock(type))
            return false;

        return true;
    }

    static Dictionary<BlockShape, List<bool>> fullFaces = new Dictionary<BlockShape, List<bool>>()
    {
        //Top, Bottom, Front, Back, Left, Right

       [BlockShape.Full] = new List<bool>() { true, true, true, true, true, true },
       [BlockShape.CornerL] = new List<bool>() { false, true, false, true, false, true },
       [BlockShape.HalfTriangle] = new List<bool>() { false, true, false, true, false, false },
       [BlockShape.CornerS] = new List<bool>() { false, false, false, false, false, false}
    };

    static bool IsFullFace(Block block, BlockFace face)
    {
        if (!CanRender(block.type))
            return false;

        if (!BlockEx.IsComplexeBlock(block.type))
            return true;

        var rot = BlockEx.GetRotationFromData(block.data);
        var shape = BlockEx.GetShapeFromData(block.data);

        return shape == BlockShape.Full;

        face = ApplyRotationToFace(face, rot);

        return fullFaces[shape][(int)face];
    }

    static List<BlockFace> faceOrderBase = new List<BlockFace>() { BlockFace.Front, BlockFace.Left, BlockFace.Back, BlockFace.Right };
    static List<BlockFace> faceOrderVert = new List<BlockFace>() { BlockFace.Top, BlockFace.Front, BlockFace.Bottom, BlockFace.Back };
    static List<BlockFace> faceOrderFlip = new List<BlockFace>() { BlockFace.Top, BlockFace.Left, BlockFace.Bottom, BlockFace.Right };

    static BlockFace ApplyRotationToFace(BlockFace face, BlockRotation rot)
    {
        List<BlockFace> currentList = null;
        Rotation simpleRot = Rotation.rot_0;
        if (rot <= BlockRotation.rot_270)
        {
            currentList = faceOrderBase;
            simpleRot = (Rotation)rot;
        }
        else if (rot <= BlockRotation.rot_vert_270)
        {
            currentList = faceOrderVert;
            simpleRot = (Rotation)(rot - BlockRotation.rot_vert_0);
        }
        else
        {
            currentList = faceOrderFlip;
            simpleRot = (Rotation)(rot - BlockRotation.rot_flip_0);
        }

        if (!currentList.Contains(face))
            return face;

        Rotation currentRot = (Rotation)currentList.IndexOf(face);
        currentRot = RotationEx.Add(currentRot, simpleRot);

        return currentList[(int)currentRot];
    }

    static Quaternion BlockRotationToQuaternion(BlockRotation rot)
    {
        switch(rot)
        {
            case BlockRotation.rot_0:
                return Quaternion.Euler(0, 0, 0);
            case BlockRotation.rot_90:
                return Quaternion.Euler(0, 90, 0);
            case BlockRotation.rot_180:
                return Quaternion.Euler(0, 180, 0);
            case BlockRotation.rot_270:
                return Quaternion.Euler(0, 270, 0);

            case BlockRotation.rot_vert_0:
                return Quaternion.Euler(90, 0, 0);
            case BlockRotation.rot_vert_90:
                return Quaternion.Euler(90, 90, 0);
            case BlockRotation.rot_vert_180:
                return Quaternion.Euler(90, 180, 0);
            case BlockRotation.rot_vert_270:
                return Quaternion.Euler(90, 270, 0);

            case BlockRotation.rot_flip_0:
                return Quaternion.Euler(0, 0, 90);
            case BlockRotation.rot_flip_90:
                return Quaternion.Euler(0, 90, 90);
            case BlockRotation.rot_flip_180:
                return Quaternion.Euler(0, 180, 90);
            case BlockRotation.rot_flip_270:
                return Quaternion.Euler(0, 270, 90);
        }

        return Quaternion.identity;
    }
}
