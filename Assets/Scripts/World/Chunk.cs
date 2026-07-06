using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk
{
    Matrix<Block> m_blocks;
    Matrix<int> m_heights;
    Vector3Int m_pos;

    public Chunk(Vector3Int pos)
    {
        m_pos = pos;
        
        m_blocks = new Matrix<Block>(Grid.ChunkSize, Grid.ChunkSize, Grid.ChunkSize);
        m_blocks.SetAll(new Block(BlockType.air));

        m_heights = new Matrix<int>(Grid.ChunkSize, Grid.ChunkSize);
        m_heights.SetAll(-1);
    }

    public Vector3Int GetPos()
    {
        return m_pos;
    }

    public Block Get(int x, int y, int z)
    {
        return m_blocks.Get(x, y, z);
    }

    public void Set(int x, int y, int z, Block value)
    {
        m_blocks.Set(x, y, z, value);

        int currentHeight = m_heights.Get(x, z);

        if(value.type == BlockType.air)
        {
            if(y == currentHeight)
            {
                y--;
                for(int h = y; h <= 0; h-- )
                {
                    var b = m_blocks.Get(x, h, z);
                    if (b.type != BlockType.air)
                        break;
                    y--;
                }
                m_heights.Set(x, z, y);
            }
        }
        else
        {
            if (currentHeight < y)
                m_heights.Set(x, z, y);
        }
    }

    public int GetHeight(int x, int z)
    {
        return m_heights.Get(x, z);
    }
}
