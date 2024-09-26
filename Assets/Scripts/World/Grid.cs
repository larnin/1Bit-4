using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Grid
{
    public const int ChunkSize = 32;

    Matrix<Matrix<BlockType>> m_chunks;
    int m_size;
    int m_height;

    public Grid(int size, int height)
    {
        m_size = size;
        m_height = height;

        m_chunks = new Matrix<Matrix<BlockType>>(size, height, size);

        for(int i = 0; i < m_size; i++)
        {
            for(int j = 0; j < m_height; j++)
            {
                for(int k = 0; k < m_size; k++)
                {
                    var chunk = new Matrix<BlockType>(ChunkSize, ChunkSize, ChunkSize);
                    chunk.SetAll(BlockType.air);
                    m_chunks.Set(i, j, k, chunk);
                }
            }
        }
    }

    public static Vector3Int PosToChunkIndex(Vector3Int pos)
    {
        Vector3Int index = pos / ChunkSize;
        if (pos.x < 0)
            index.x--;
        if (pos.y < 0)
            index.y--;
        if (pos.z < 0)
            index.z--;

        return index;
    }

    public static Vector3Int PosToPosInChunk(Vector3Int pos)
    {
        var index = PosToChunkIndex(pos);
        return pos - index * ChunkSize;
    }

    public static Vector3Int PosInChunkToPos(Vector3Int index, Vector3Int pos)
    {
        return index * ChunkSize + pos;
    }

    public int Size()
    {
        return m_size;
    }

    public int Height()
    {
        return m_height;
    }

    public Matrix<BlockType> Get(Vector3Int index)
    {
        if (index.x < 0 || index.x >= m_size || index.y < 0 || index.y >= m_height || index.z < 0 || index.z >= m_size)
            return null;

        return m_chunks.Get(index.x, index.y, index.z);
    }
}
