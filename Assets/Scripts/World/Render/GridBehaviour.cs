using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GridBehaviour : MonoBehaviour
{
    Grid m_grid;
    Matrix<ChunkBehaviour> m_chunks;

    public void SetGrid(Grid grid)
    {
        m_grid = grid;
        CreateChunks();
    }

    public Grid GetGrid()
    {
        return m_grid;
    }

    void CreateChunks()
    {
        int size = m_grid.Size();
        int height = m_grid.Height();

        m_chunks = new Matrix<ChunkBehaviour>(size, height, size);

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < size; k++)
                {
                    CreateOneChunk(new Vector3Int(i, j, k));
                }
            }
        }
    }

    void CreateOneChunk(Vector3Int index)
    {
        var chunkObj = new GameObject("Chunk " + index.x.ToString() + " " + index.y.ToString() + " " + index.z.ToString());

        chunkObj.transform.parent = transform;
        chunkObj.transform.localPosition = new Vector3(index.x, index.y, index.z) * Grid.ChunkSize;
        chunkObj.transform.localRotation = Quaternion.identity;
        chunkObj.transform.localScale = Vector3.one;

        var behaviour = chunkObj.AddComponent<ChunkBehaviour>();
        behaviour.SetChunk(m_grid, index);

        m_chunks.Set(index.x, index.y, index.z, behaviour);
    }

    public int GetGeneratedCount()
    {
        int nb = 0;

        var size = m_chunks.size;

        for(int i = 0; i < size.x; i++)
        {
            for(int j = 0; j < size.y; j++)
            {
                for(int k = 0; k < size.z; k++)
                {
                    if (m_chunks.Get(i, j, k).Generated())
                        nb++;
                }
            }
        }

        return nb;
    }

    public int GetTotalCount()
    {
        return m_chunks.width * m_chunks.height * m_chunks.depth;
    }
}
