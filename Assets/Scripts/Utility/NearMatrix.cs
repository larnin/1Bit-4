using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NearMatrix<T>
{
    T[] m_data = new T[9];

    public void Set(T value, int x, int y)
    {
        if (x < -1 || x > 1)
            return;
        if (y < -1 || y > 1)
            return;

        m_data[PosToIndex(x, y)] = value;
    }

    public T Get(int x, int y)
    {
        if (x < -1 || x > 1)
            return default(T);
        if (y < -1 || y > 1)
            return default(T);

        return m_data[PosToIndex(x, y)];
    }
    public void Reset(T value = default(T))
    {
        for (int i = 0; i < 9; i++)
            m_data[i] = value;
    }

    int PosToIndex(int x, int y)
    {
        return x + 1 + (y + 1) * 3;
    }
}
