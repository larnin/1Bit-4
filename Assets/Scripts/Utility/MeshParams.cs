﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class MeshParamData<T> where T : struct
{
    public const int maxVertexSize = ushort.MaxValue - 5;

    public T[] vertices = null;
    public int verticesSize = 0;
    public ushort[] indexes = null;
    public int indexesSize = 0;

    public bool CanAllocate(int vertexSize)
    {
        return vertexSize + verticesSize <= maxVertexSize;
    }
}

public class MeshParams<T, U> where T : struct where U : struct
{
    public Dictionary<Material, List<MeshParamData<T>>> m_data = new Dictionary<Material, List<MeshParamData<T>>>();
    public List<MeshParamData<U>> m_colliderData = new List<MeshParamData<U>>();

    public MeshParamData<T> Allocate(int vertexSize, int indexSize, Material material)
    {
        Assert.IsTrue(vertexSize <= MeshParamData<T>.maxVertexSize);

        List<MeshParamData<T>> data = null;
        m_data.TryGetValue(material, out data);

        if (data == null)
        {
            data = new List<MeshParamData<T>>();
            m_data[material] = data;
            data.Add(new MeshParamData<T>());
            MeshParamTools.AllocateVerticesArray(data[0], vertexSize);
            MeshParamTools.AllocateIndexArray(data[0], indexSize);
        }

        var element = data[data.Count - 1];

        if (!element.CanAllocate(vertexSize))
        {
            var newData = new MeshParamData<T>();
            data.Add(newData);
            MeshParamTools.AllocateVerticesArray(newData, vertexSize);
            MeshParamTools.AllocateIndexArray(newData, indexSize);
            element = newData;
        }

        if (element.verticesSize + vertexSize > element.vertices.Length)
            MeshParamTools.IncreaseVerticesArray(element, vertexSize);
        if (element.indexesSize + indexSize > element.indexes.Length)
            MeshParamTools.IncreaseIndexArray(element, indexSize);

        return element;
    }

    public MeshParamData<U> AllocateCollider(int vertexSize, int indexSize)
    {
        Assert.IsTrue(vertexSize <= MeshParamData<T>.maxVertexSize);

        if (m_colliderData.Count == 0)
        {
            m_colliderData.Add(new MeshParamData<U>());
            MeshParamTools.AllocateVerticesArray(m_colliderData[0], vertexSize);
            MeshParamTools.AllocateIndexArray(m_colliderData[0], indexSize);
        }

        var element = m_colliderData[m_colliderData.Count - 1];

        if (!element.CanAllocate(vertexSize))
        {
            var newData = new MeshParamData<U>();
            m_colliderData.Add(newData);
            MeshParamTools.AllocateVerticesArray(newData, vertexSize);
            MeshParamTools.AllocateIndexArray(newData, indexSize);
            element = newData;
        }

        if (element.verticesSize + vertexSize > element.vertices.Length)
            MeshParamTools.IncreaseVerticesArray(element, vertexSize);
        if (element.indexesSize + indexSize > element.indexes.Length)
            MeshParamTools.IncreaseIndexArray(element, indexSize);

        return element;
    }

    //only clean index but not resize the buffers
    //only remove the sub buffers if there are more than one MeshParamData for one material
    public void ResetSize()
    {
        foreach (var d in m_data)
        {
            while (d.Value.Count > 1)
                d.Value.RemoveAt(1);

            d.Value[0].indexesSize = 0;
            d.Value[0].verticesSize = 0;
        }

        while (m_colliderData.Count > 1)
            m_colliderData.RemoveAt(1);

        if (m_colliderData.Count > 0)
        {
            m_colliderData[0].indexesSize = 0;
            m_colliderData[0].verticesSize = 0;
        }
    }

    //clear all buffers
    public void Reset()
    {
        m_data.Clear();
        m_colliderData.Clear();
    }

    public List<Material> GetNonEmptyMaterials()
    {
        List<Material> materials = new List<Material>();

        foreach (var d in m_data)
        {
            if (d.Value[0].vertices.Count() > 0 && d.Value[0].indexes.Count() > 0)
                materials.Add(d.Key);
        }

        return materials;
    }

    public int GetMeshCount(Material material)
    {
        List<MeshParamData<T>> data = null;
        m_data.TryGetValue(material, out data);

        if (data == null)
            return 0;

        int nb = 0;
        foreach (var d in data)
            if (d.verticesSize > 0 && d.indexesSize > 0)
                nb++;

        return nb;
    }

    public MeshParamData<T> GetMesh(Material material, int index)
    {
        List<MeshParamData<T>> data = null;
        m_data.TryGetValue(material, out data);
        if (data == null)
            return null;
        if (data.Count <= index)
            return null;
        return data[index];
    }

    public int GetColliderMeshCount()
    {
        if (m_colliderData.Count == 1 && (m_colliderData[0].vertices.Length == 0 || m_colliderData[0].indexes.Length == 0))
            return 0;

        return m_colliderData.Count();
    }

    public MeshParamData<U> GetColliderMesh(int index)
    {
        Assert.IsTrue(index >= 0 && index < GetColliderMeshCount());

        return m_colliderData[index];
    }
}

public class SimpleMeshParam<T> where T : struct
{
    public List<MeshParamData<T>> m_data = new List<MeshParamData<T>>();

    public MeshParamData<T> Allocate(int vertexSize, int indexSize)
    {
        Assert.IsTrue(vertexSize <= MeshParamData<T>.maxVertexSize);

        if (m_data.Count == 0)
        {
            m_data.Add(new MeshParamData<T>());
            MeshParamTools.AllocateVerticesArray(m_data[0], vertexSize);
            MeshParamTools.AllocateIndexArray(m_data[0], indexSize);
        }

        var element = m_data[m_data.Count - 1];

        if (!element.CanAllocate(vertexSize))
        {
            var newData = new MeshParamData<T>();
            m_data.Add(newData);
            MeshParamTools.AllocateVerticesArray(newData, vertexSize);
            MeshParamTools.AllocateIndexArray(newData, indexSize);
            element = newData;
        }

        if (element.verticesSize + vertexSize > element.vertices.Length)
            MeshParamTools.IncreaseVerticesArray(element, vertexSize);
        if (element.indexesSize + indexSize > element.indexes.Length)
            MeshParamTools.IncreaseIndexArray(element, indexSize);

        return element;
    }

    //only clean index but not resize the buffers
    //only remove the sub buffers if there are more than one MeshParamData for one material
    public void ResetSize()
    {
        while (m_data.Count > 1)
            m_data.RemoveAt(1);

        if (m_data.Count > 1)
        {
            m_data[0].indexesSize = 0;
            m_data[0].verticesSize = 0;
        }
    }

    //clear all buffers
    public void Reset()
    {
        m_data.Clear();
    }

    public int GetMeshCount()
    {
        if (m_data.Count == 1 && (m_data[0].vertices.Length == 0 || m_data[0].indexes.Length == 0))
            return 0;

        return m_data.Count();
    }

    public MeshParamData<T> GetMesh(int index)
    {
        Assert.IsTrue(index >= 0 && index < GetMeshCount());

        return m_data[index];
    }
}

static class MeshParamTools
{
    const int allocSize = 1000;

    public static void AllocateVerticesArray<V>(MeshParamData<V> data, int addVertices) where V : struct
    {
        Assert.IsTrue(data.vertices == null);
        int newSize = addVertices + allocSize;
        if (newSize > MeshParamData<V>.maxVertexSize)
            newSize = MeshParamData<V>.maxVertexSize;

        data.vertices = new V[newSize];
    }

    public static void IncreaseVerticesArray<V>(MeshParamData<V> data, int addVertices) where V : struct
    {
        Assert.IsTrue(data.vertices != null);

        int newSize = data.vertices.Length + addVertices + allocSize;
        if (newSize > MeshParamData<V>.maxVertexSize)
            newSize = MeshParamData<V>.maxVertexSize;

        var newArray = new V[newSize];

        for (int i = 0; i < data.verticesSize; i++)
            newArray[i] = data.vertices[i];
        data.vertices = newArray;
    }

    public static void AllocateIndexArray<V>(MeshParamData<V> data, int addIndexes) where V : struct
    {
        Assert.IsTrue(data.indexes == null);

        data.indexes = new ushort[addIndexes + allocSize];
    }

    public static void IncreaseIndexArray<V>(MeshParamData<V> data, int addIndexes) where V : struct
    {
        Assert.IsTrue(data.indexes != null);

        int newSize = data.indexes.Length + addIndexes + allocSize;

        var newIndex = new ushort[newSize];

        for (int i = 0; i < data.indexesSize; i++)
            newIndex[i] = data.indexes[i];
        data.indexes = newIndex;
    }
}
