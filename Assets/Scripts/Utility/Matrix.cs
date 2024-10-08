﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

public class Matrix<T>
{
    int m_width;
    int m_depth;
    int m_height;

    public int width { get { return m_width; } }
    public int depth { get { return m_depth; } }
    public int height { get { return m_height; } }
    public Vector3Int size { get { return new Vector3Int(m_width, m_height, m_depth); } }

    T[] m_data;

    public Matrix(int width, int height, int depth)
    {
        m_width = width;
        m_height = height;
        m_depth = depth;

        m_data = new T[m_width * m_height * m_depth];
    }

    public Matrix(int width, int depth)
    {
        m_width = width;
        m_height = 1;
        m_depth = depth;

        m_data = new T[m_width * m_height * m_depth];
    }

    public T Get(int x, int z)
    {
        return m_data[PosToIndex(x, 0, z)];
    }

    public T Get(int x, int y, int z)
    {
        return m_data[PosToIndex(x, y, z)];
    }

    public void Set(int x, int z, T value)
    {
        Set(x, 0, z, value);
    }

    public void Set(int x, int y, int z, T value)
    {
        m_data[PosToIndex(x, y, z)] = value;
    }

    public void SetAll(T value = default(T))
    {
        for (int i = 0; i < m_data.Length; i++)
            m_data[i] = value;
    }

    int PosToIndex(int x, int y, int z)
    {
        Assert.IsTrue(x >= 0 && x < m_width && y >= 0 && y < m_height && z >= 0 && z < m_depth);
        
        return (x * m_height + y) * m_depth + z;
    }
}
