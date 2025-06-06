﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Grid
{
    public const int ChunkSize = 32;

    Matrix<Matrix<Block>> m_chunks;
    int m_size;
    int m_height;
    bool m_loopX;
    bool m_loopZ;

    public Grid(int size, int height, bool loopX = false, bool loopZ = false)
    {
        m_size = size;
        m_height = height;
        m_loopX = loopX;
        m_loopZ = loopZ;

        m_chunks = new Matrix<Matrix<Block>>(size, height, size);

        for(int i = 0; i < m_size; i++)
        {
            for(int j = 0; j < m_height; j++)
            {
                for(int k = 0; k < m_size; k++)
                {
                    var chunk = new Matrix<Block>(ChunkSize, ChunkSize, ChunkSize);
                    chunk.SetAll(new Block(BlockType.air));
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

    public static Vector2Int PosToChunkIndex(Vector2Int pos)
    {
        Vector2Int index = pos / ChunkSize;
        if (pos.x < 0)
            index.x--;
        if (pos.y < 0)
            index.y--;

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

    public bool LoopX()
    {
        return m_loopX;
    }

    public bool LoopZ()
    {
        return m_loopZ;
    }

    public Matrix<Block> Get(Vector3Int index)
    {
        if (index.x < 0 || index.x >= m_size || index.y < 0 || index.y >= m_height || index.z < 0 || index.z >= m_size)
            return null;

        return m_chunks.Get(index.x, index.y, index.z);
    }
}
