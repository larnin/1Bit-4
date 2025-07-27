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
        return MakeMesh(meshParams, new Bounds(center, size));
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
                min[i] = j;
                max[i] = j;

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

                data.verticesSize += 8;
                data.indexesSize += 4;
            }
        }

        return MakeMesh(meshParams, new Bounds(new Vector3(size.x, size.y, size.z) / 2, size));
    }

    public static Mesh Sphere(Vector3Int size, Color32 color)
    {
        return null;
    }
    static Mesh MakeMesh(SimpleMeshParam<WireframeVertexDefinition> meshParams, Bounds bounds)
    {
        var data = meshParams.GetMesh(0);

        Mesh mesh = new Mesh();
        MeshEx.SetWorldMeshParams(mesh, data.verticesSize, data.indexesSize);

        mesh.SetVertexBufferData(data.vertices, 0, 0, data.verticesSize);
        mesh.SetIndexBufferData(data.indexes, 0, 0, data.indexesSize);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, data.indexesSize, MeshTopology.Lines));

        mesh.bounds = bounds;

        return mesh;
    }
}
