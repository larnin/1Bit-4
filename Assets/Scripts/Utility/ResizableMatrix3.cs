using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResizableMatrix3<T>
{
    int m_width = 0;
    int m_height = 0;
    int m_depth = 0;

    int m_originX = 0;
    int m_originY = 0;
    int m_originZ = 0;

    T m_defaultValue = default(T);

    T[] m_data = null;

    public ResizableMatrix3(int width, int height, int depth, T defaultValue = default(T))
    {
        Reset(width, height, depth, defaultValue);
    }

    public ResizableMatrix3(int width, int height, int depth, int originX, int originY, int originZ, T defaultValue = default(T))
    {
        Reset(width, height, depth, originX, originY, originZ, defaultValue);
    }

    public void Reset(int width, int height, int depth, T defaultValue = default(T))
    {
        Reset(width, height, depth, 0, 0, 0, defaultValue);
    }

    public void Reset(int width, int height, int depth, int originX, int originY, int originZ, T defaultValue = default(T))
    {
        m_defaultValue = defaultValue;
        RecreateGrid(width, height, depth, originX, originY, originZ);
    }

    public int MinX() { return -m_originX; }

    public int MinY() { return -m_originY; }

    public int MinZ() { return -m_originZ; }

    public int MaxX() { return m_width - m_originX - 1; }

    public int MaxY() { return m_height - m_originY - 1; }

    public int MaxZ() { return m_depth - m_originZ - 1; }

    public int Width() { return m_width; }

    public int Height() { return m_height; }

    public int Depth() { return m_depth; }

    public int OriginX() { return m_originX; }

    public int OriginY() { return m_originY; }

    public int OriginZ() { return m_originZ; }

    public void Set(T value, int x, int y, int z)
    {
        if(!IsInGrid(x, y, z))
        {
            int newOriginX = m_originX;
            int newOriginY = m_originY;
            int newOriginZ = m_originZ;
            int newWidth = m_width;
            int newHeight = m_height;
            int newDepth = m_depth;

            if(x < MinX())
            {
                newOriginX = OriginX() + MinX() - x;
                newWidth += MinX() - x;
            }
            else if(x > MaxX())
                newWidth += x - MaxX();

            if (y < MinY())
            {
                newOriginY = OriginY() + MinY() - y;
                newHeight += MinY() - y;
            }
            else if (y > MaxY())
                newHeight += y - MaxY();

            if (z < MinZ())
            {
                newOriginZ = OriginZ() + MinZ() - z;
                newDepth += MinZ() - z;
            }
            else if (z > MaxZ())
                newDepth += z - MaxZ();

            RecreateGrid(newWidth, newHeight, newDepth, newOriginX, newOriginY, newOriginZ);
        }

        int index = PosToIndex(x, y, z);

        m_data[index] = value;

    }

    public T Get(int x, int y, int z)
    {
        if (!IsInGrid(x, y, z))
            return m_defaultValue;

        int index = PosToIndex(x, y, z);
        return m_data[index];
    }

    public bool IsInGrid(int x, int y, int z)
    {
        if (x < MinX() || x > MaxX())
            return false;

        if (y < MinY() || y > MaxY())
            return false;

        if (z < MinZ() || z > MaxZ())
            return false;

        return true;
    }

    void RecreateGrid(int newWidth, int newHeight, int newDepth, int newOriginX, int newOriginY, int newOriginZ)
    {
        T[] newGrid = new T[newWidth * newHeight * newDepth];
        for (int i = 0; i < newWidth * newHeight * newDepth; i++)
            newGrid[i] = m_defaultValue;

        if(m_data != null)
        {
            int minX = Mathf.Max(MinX(), -newOriginX);
            int minY = Mathf.Max(MinY(), -newOriginY);
            int minZ = Mathf.Max(MinZ(), -newOriginZ);
            int maxX = Mathf.Min(MaxX(), newWidth - newOriginX - 1);
            int maxY = Mathf.Min(MaxY(), newHeight - newOriginY - 1);
            int maxZ = Mathf.Min(MaxZ(), newDepth - newOriginZ - 1);

            if(maxX >= minX && maxY >= minY && maxZ >= minZ)
            {
                for(int i = minX; i <= maxX; i++)
                {
                    for(int j = minY; j <= maxY; j++)
                    {
                        for (int k = minZ; k <= maxZ; k++)
                        {
                            int oldIndex = PosToIndex(i, j, k);
                            int newIndex = PosToIndex(i, j, k, newWidth, newHeight, newDepth, newOriginX, newOriginY, newOriginZ);

                            newGrid[newIndex] = m_data[oldIndex];
                        }
                    }
                }
            }
        }

        m_data = newGrid;
        m_width = newWidth;
        m_height = newHeight;
        m_depth = newDepth;
        m_originX = newOriginX;
        m_originY = newOriginY;
        m_originZ = newOriginZ;
    }

    static int PosToIndex(int x, int y, int z, int width, int height, int depth, int originX, int originY, int originZ)
    {
        x += originX;
        y += originY;
        z += originZ;

        if(x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth )
        {
            Debug.LogError("Pos out of bound");
            return 0;
        }

        return x + y * width + z * width * height;
    }

    int PosToIndex(int x, int y, int z)
    {
        return PosToIndex(x, y, z, m_width, m_height, m_depth, m_originX, m_originY, m_originZ);
    }
}

