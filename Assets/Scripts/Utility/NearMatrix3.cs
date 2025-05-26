using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NearMatrix3<T>
{
    T[] m_data = new T[27];

    public NearMatrix3() { }

    public NearMatrix3(T[] data)
    {
        int maxSize = Math.Min(data.Length, m_data.Length);
        for (int i = 0; i < maxSize; i++)
            m_data[i] = data[i];
    }

    public void Set(T value, int x, int y, int z)
    {
        if (x < -1 || x > 1)
            return;
        if (y < -1 || y > 1)
            return;
        if (z < -1 || z > 1)
            return;

        m_data[PosToIndex(x, y, z)] = value;
    }

    public T Get(int x, int y, int z)
    {
        if (x < -1 || x > 1)
            return default(T);
        if (y < -1 || y > 1)
            return default(T);
        if (z < -1 || z > 1)
            return default(T);

        return m_data[PosToIndex(x, y, z)];
    }

    public void Reset(T value = default(T))
    {
        for (int i = 0; i < 27; i++)
            m_data[i] = value;
    }

    int PosToIndex(int x, int y, int z)
    {
        return x + 1 + (z + 1) * 3 + (y + 1) * 9;
    }

    public NearMatrix<T> GetLayerMatrix(int y)
    {
        NearMatrix<T> mat = new NearMatrix<T>();
        if (y < -1 || y > 1)
            return mat;

        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                mat.Set(Get(x, y, z), x, z);
            }
        }

        return mat;
    }

    public NearMatrix3<T> Copy()
    {
        return new NearMatrix3<T>(m_data);
    }
}
