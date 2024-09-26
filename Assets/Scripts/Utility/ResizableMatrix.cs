using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResizableMatrix<T>
{
    int m_width = 0;
    int m_height = 0;

    int m_originX = 0;
    int m_originY = 0;

    T m_defaultValue = default(T);

    T[] m_data = null;

    public ResizableMatrix(int width, int height, T defaultValue = default(T))
    {
        m_defaultValue = defaultValue;
        RecreateGrid(width, height, 0, 0);
    }

    public ResizableMatrix(int width, int height, int originX, int originY, T defaultValue = default(T))
    {
        m_defaultValue = defaultValue;
        RecreateGrid(width, height, originX, originY);
    }

    public int MinX() { return -m_originX; }

    public int MinY() { return -m_originY; }

    public int MaxX() { return m_width - m_originX - 1; }

    public int MaxY() { return m_height - m_originY - 1; }

    public int Width() { return m_width; }

    public int Height() { return m_height; }

    public int OriginX() { return m_originX; }

    public int OriginY() { return m_originY; }

    public void Set(T value, int x, int y)
    {
        if(!IsInGrid(x, y))
        {
            int newOriginX = m_originX;
            int newOriginY = m_originY;
            int newWidth = m_width;
            int newHeight = m_height;

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

            RecreateGrid(newWidth, newHeight, newOriginX, newOriginY);
        }

        int index = PosToIndex(x, y);

        m_data[index] = value;

    }

    public T Get(int x, int y)
    {
        if (!IsInGrid(x, y))
            return m_defaultValue;

        int index = PosToIndex(x, y);
        return m_data[index];
    }

    public bool IsInGrid(int x, int y)
    {
        if (x < MinX() || x > MaxX())
            return false;

        if (y < MinY() || y > MaxY())
            return false;

        return true;
    }

    void RecreateGrid(int newWidth, int newHeight, int newOriginX, int newOriginY)
    {
        T[] newGrid = new T[newWidth * newHeight];
        for (int i = 0; i < newWidth * newHeight; i++)
            newGrid[i] = m_defaultValue;

        if(m_data != null)
        {
            int minX = Mathf.Max(MinX(), -newOriginX);
            int minY = Mathf.Max(MinY(), -newOriginY);
            int maxX = Mathf.Min(MaxX(), newWidth - newOriginX - 1);
            int maxY = Mathf.Min(MaxY(), newHeight - newOriginY - 1);

            if(maxX >= minX && maxY >= minY)
            {
                for(int i = minX; i <= maxX; i++)
                {
                    for(int j = minY; j <= maxY; j++)
                    {
                        int oldIndex = PosToIndex(i, j);
                        int newIndex = PosToIndex(i, j, newWidth, newHeight, newOriginX, newOriginY);

                        newGrid[newIndex] = m_data[oldIndex];
                    }
                }
            }
        }

        m_data = newGrid;
        m_width = newWidth;
        m_height = newHeight;
        m_originX = newOriginX;
        m_originY = newOriginY;
    }

    static int PosToIndex(int x, int y, int width, int height, int originX, int originY)
    {
        x += originX;
        y += originY;

        if(x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogError("Pos out of bound");
            return 0;
        }

        return x + y * width;
    }

    int PosToIndex(int x, int y)
    {
        return PosToIndex(x, y, m_width, m_height, m_originX, m_originY);
    }
}

