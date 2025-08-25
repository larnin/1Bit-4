using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class WireframeMesh
{
    public static Mesh SimpleCube(Vector3 size, Color32 color)
    {
        return SimpleCube(size, size / 2, color);
    }

    public static Mesh SimpleCube(Vector3 size, Vector3 center, Color32 color)
    {
        SimpleMeshParam<WireframeVertexDefinition> meshParams = new SimpleMeshParam<WireframeVertexDefinition>();
        
        SetSimpleCubeData(meshParams, size, center, color);
        return MakeMesh(meshParams, new Bounds(center + size / 2.0f, size));
    }

    static void SetSimpleCubeData(SimpleMeshParam<WireframeVertexDefinition> meshParams, Vector3 size, Vector3 center, Color32 color)
    {
        var data = meshParams.Allocate(8, 24);

        int v = data.verticesSize;
        int id = data.indexesSize;

        data.vertices[v].pos = Vector3.zero;
        data.vertices[v + 1].pos = new Vector3(size.x, 0, 0);
        data.vertices[v + 2].pos = new Vector3(0, size.y, 0);
        data.vertices[v + 3].pos = new Vector3(size.x, size.y, 0);
        data.vertices[v + 4].pos = new Vector3(0, 0, size.z);
        data.vertices[v + 5].pos = new Vector3(size.x, 0, size.z);
        data.vertices[v + 6].pos = new Vector3(0, size.y, size.z);
        data.vertices[v + 7].pos = new Vector3(size.x, size.y, size.z);

        for (int i = 0; i < 8; i++)
        {
            data.vertices[v + i].pos -= center;
            data.vertices[v + i].color = color;
        }

        data.indexes[id] = 0;
        data.indexes[id + 1] = 1;
        data.indexes[id + 2] = 1;
        data.indexes[id + 3] = 3;
        data.indexes[id + 4] = 3;
        data.indexes[id + 5] = 2;
        data.indexes[id + 6] = 2;
        data.indexes[id + 7] = 0;
                     
        data.indexes[id + 8] = 4;
        data.indexes[id + 9] = 5;
        data.indexes[id + 10] = 5;
        data.indexes[id + 11] = 7;
        data.indexes[id + 12] = 7;
        data.indexes[id + 13] = 6;
        data.indexes[id + 14] = 6;
        data.indexes[id + 15] = 4;
                     
        data.indexes[id + 16] = 0;
        data.indexes[id + 17] = 4;
        data.indexes[id + 18] = 1;
        data.indexes[id + 19] = 5;
        data.indexes[id + 20] = 2;
        data.indexes[id + 21] = 6;
        data.indexes[id + 22] = 3;
        data.indexes[id + 23] = 7;

        data.verticesSize += 8;
        data.indexesSize += 24;
    }

    public static Mesh Cuboid(Vector3Int size, Color32 color)
    {
        SimpleMeshParam<WireframeVertexDefinition> meshParams = new SimpleMeshParam<WireframeVertexDefinition>();

        SetSimpleCubeData(meshParams, size, Vector3.zero, color);

        Vector3 sizeF = size;
        
        for(int i = 0; i < 3; i++)
        {
            int nbStep = size[i] - 1;
            var data = meshParams.Allocate(nbStep * 4, nbStep * 8);
            for(int j = 0; j < nbStep; j++)
            {
                Vector3 min = Vector3.zero;
                Vector3 max = sizeF;
                min[i] = j + 1;
                max[i] = j + 1;

                data.vertices[data.verticesSize].pos = min;
                data.vertices[data.verticesSize + 1].pos = i == 2 ? new Vector3(min.x, max.y, max.z) : new Vector3(min.x, min.y, max.z);
                data.vertices[data.verticesSize + 2].pos = i == 2 ? new Vector3(max.x, min.y, min.z) : new Vector3(max.x, max.y, min.z);
                data.vertices[data.verticesSize + 3].pos = max;

                data.vertices[data.verticesSize].color = color;
                data.vertices[data.verticesSize + 1].color = color;
                data.vertices[data.verticesSize + 2].color = color;
                data.vertices[data.verticesSize + 3].color = color;

                data.indexes[data.indexesSize] = (ushort)data.verticesSize;
                data.indexes[data.indexesSize + 1] = (ushort)(data.verticesSize + 1);
                data.indexes[data.indexesSize + 2] = (ushort)(data.verticesSize + 1);
                data.indexes[data.indexesSize + 3] = (ushort)(data.verticesSize + 3);
                data.indexes[data.indexesSize + 4] = (ushort)(data.verticesSize + 3);
                data.indexes[data.indexesSize + 5] = (ushort)(data.verticesSize + 2);
                data.indexes[data.indexesSize + 6] = (ushort)(data.verticesSize + 2);
                data.indexes[data.indexesSize + 7] = (ushort)data.verticesSize;

                data.verticesSize += 4;
                data.indexesSize += 8;
            }
        }

        return MakeMesh(meshParams, new Bounds(new Vector3(size.x, size.y, size.z) / 2, size));
    }

    public static Mesh VoxelSphere(Vector3Int size, Color32 color)
    {
        SimpleMeshParam<WireframeVertexDefinition> meshParams = new SimpleMeshParam<WireframeVertexDefinition>();

        Vector3 center = new Vector3(size.x - 1, size.y - 1, size.z - 1) / 2;
        Vector3 circleSize = center + Vector3.one * 0.5f;

        for(int i = -1; i <= size.x; i++)
        {
            for(int j = -1; j <= size.y; j++)
            {
                for (int k = -1; k <= size.z; k++)
                {
                    Vector3 p = new Vector3(i, j, k) - center;

                    bool b = IsPosOnSphere(p, circleSize);
                    bool b001 = IsPosOnSphere(p + new Vector3(0, 0, 1), circleSize);
                    bool b010 = IsPosOnSphere(p + new Vector3(0, 1, 0), circleSize);
                    bool b011 = IsPosOnSphere(p + new Vector3(0, 1, 1), circleSize);
                    bool b100 = IsPosOnSphere(p + new Vector3(1, 0, 0), circleSize);
                    bool b101 = IsPosOnSphere(p + new Vector3(1, 0, 1), circleSize);
                    bool b110 = IsPosOnSphere(p + new Vector3(1, 1, 0), circleSize);

                    int edge1 = CountFlags(b, b100, b110, b010);
                    int edge2 = CountFlags(b, b100, b101, b001);
                    int edge3 = CountFlags(b, b001, b011, b010);

                    Vector3 min = p + center;

                    if (edge1 > 0 && edge1 < 4)
                        AddEdge(meshParams, min + new Vector3(1, 1, 0), min + Vector3.one, color);

                    if (edge2 > 0 && edge2 < 4)
                        AddEdge(meshParams, min + new Vector3(1, 0, 1), min + Vector3.one, color);

                    if (edge3 > 0 && edge3 < 4)
                        AddEdge(meshParams, min + new Vector3(0, 1, 1), min + Vector3.one, color);
                }
            }
        }

        return MakeMesh(meshParams, new Bounds(new Vector3(size.x, size.y, size.z) / 2, size));
    }

    public static Mesh Sphere(float radius, int circleNb, int segmentNb, Color32 color)
    {
        return Sphere(new Vector3(radius, radius, radius), circleNb, segmentNb, color);
    }

    public static Mesh Sphere(Vector3 radius, int circleNb, int segmentNb, Color32 color)
    {
        if (segmentNb < 2)
            segmentNb = 2;
        segmentNb *= 2;

        SimpleMeshParam<WireframeVertexDefinition> meshParams = new SimpleMeshParam<WireframeVertexDefinition>();

        var data = meshParams.Allocate(circleNb * segmentNb, circleNb * (2 * segmentNb - 1) * 2);

        float segmentSection = Mathf.PI / segmentNb * 2;

        for(int i = 0; i < circleNb; i++)
        {
            float angle = Mathf.PI / circleNb * i;

            for(int j = 0; j < segmentNb; j++)
            {
                float segmentAngle = segmentSection * j;

                Vector3 pos = new Vector3(Mathf.Sin(segmentAngle), Mathf.Cos(segmentAngle), 0);
                pos.z = pos.x * Mathf.Sin(angle);
                pos.x *= Mathf.Cos(angle);

                data.vertices[data.verticesSize].pos = new Vector3(pos.x * radius.x, pos.y * radius.y, pos.z * radius.z);
                data.vertices[data.verticesSize].color = color;
                data.verticesSize++;
            }
        }

        for (int i = 0; i < circleNb; i++)
        {
            for (int j = 0; j < segmentNb; j++)
            {
                int index = i * segmentNb + j;
                int index2 = j == 0 ? (i + 1) * segmentNb - 1 : index - 1;

                data.indexes[data.indexesSize] = (ushort)index;
                data.indexes[data.indexesSize + 1] = (ushort)index2;
                data.indexesSize += 2;
            }
        }

        for(int i = 1; i < segmentNb; i++)
        {
            for(int j = 0; j < circleNb; j++)
            {
                int j2 = j == 0 ? circleNb - 1 : j - 1;
                int i2 = j == 0 ? segmentNb - i: i;

                int index = j * segmentNb + i;
                int index2 = j2 * segmentNb + i2;

                data.indexes[data.indexesSize] = (ushort)index;
                data.indexes[data.indexesSize + 1] = (ushort)index2;
                data.indexesSize += 2;
            }
        }

        return MakeMesh(meshParams, new Bounds(Vector3.zero, radius));
    }

    static int CountFlags(params bool[] values)
    {
        int nb = 0;
        foreach(bool v in values)
        {
            if (v)
                nb++;
        }
        return nb;
    }

    static void AddEdge(SimpleMeshParam<WireframeVertexDefinition> meshParams, Vector3 pos1, Vector3 pos2, Color color)
    {
        var data = meshParams.Allocate(2, 2);

        data.vertices[data.verticesSize].pos = pos1;
        data.vertices[data.verticesSize].color = color;
            
        data.vertices[data.verticesSize + 1].pos = pos2;
        data.vertices[data.verticesSize + 1].color = color;

        data.indexes[data.indexesSize] = (ushort)data.verticesSize;
        data.indexes[data.indexesSize + 1] = (ushort)(data.verticesSize + 1);

        data.indexesSize += 2;
        data.verticesSize += 2;
    }

    public static bool IsPosOnSphere(Vector3 pos, Vector3 halfSize)
    {
        float r = new Vector3(pos.x / halfSize.x, pos.y / halfSize.y, pos.z / halfSize.z).sqrMagnitude;
        return r <= 1;
    }

    static Mesh MakeMesh(SimpleMeshParam<WireframeVertexDefinition> meshParams, Bounds bounds)
    {
        var data = meshParams.GetMesh(0);

        Mesh mesh = new Mesh();
        MeshEx.SetWireframeMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Lines));

        mesh.bounds = bounds;

        return mesh;
    }
}
