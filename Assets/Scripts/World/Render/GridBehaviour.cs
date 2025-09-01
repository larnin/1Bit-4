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

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetGridEvent>.Subscriber(GetGrid));
        m_subscriberList.Add(new Event<SetChunkDirtyEvent>.Subscriber(SetChunkDirty));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public void SetGrid(Grid grid)
    {
        m_grid = grid;
        CreateChunks();
    }

    public Grid GetGrid()
    {
        return m_grid;
    }

    void GetGrid(GetGridEvent e)
    {
        e.grid = m_grid;
    }

    public void SetChunksDirty(BoundsInt bounds)
    {
        Vector3Int min = bounds.min;
        Vector3Int max = bounds.max;

        int size = m_grid.Size();
        int height = m_grid.Height();

        for(int i = min.x - 1; i <= max.x + 1; i++)
        {
            for(int j = min.y - 1; j <= max.y + 1; j++)
            {
                for(int k = min.z - 1; k <= max.z + 1; k++)
                {
                    Vector3Int loopPos = GridEx.GetPosFromLoop(m_grid, new Vector3Int(i, j, k));
                    if (!m_grid.LoopX() && loopPos.x != i)
                        continue;
                    if (!m_grid.LoopZ() && loopPos.z != k)
                        continue;

                    if (j < 0 || j >= height)
                        continue;

                    var behaviour = m_chunks.Get(loopPos.x, loopPos.y, loopPos.z);
                    behaviour.SetChunk(m_grid, loopPos);
                }
            }
        }
    }

    public void SetChunkDirty(Vector3Int pos)
    {
        int size = m_grid.Size();
        int height = m_grid.Height();


        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    Vector3Int chunk = pos + new Vector3Int(i, j, k);

                    if (chunk.x < 0 || chunk.y < 0 || chunk.z < 0 || chunk.x >= size || chunk.y >= height || chunk.z >= size)
                        continue;

                    var behaviour = m_chunks.Get(chunk.x, chunk.y, chunk.z);
                    behaviour.SetChunk(m_grid, chunk);
                }
            }
        }
    }

    void SetChunkDirty(SetChunkDirtyEvent e)
    {
        SetChunkDirty(e.chunk);
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

        chunkObj.layer = gameObject.layer;
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

    public void ResizeGrid(int size, int height, Action<Matrix<Block>, Vector3Int> populateNewChunkCallback = null)
    {
        Grid newGrid = new Grid(size, height, m_grid.LoopX(), m_grid.LoopZ());

        Matrix<ChunkBehaviour> newChunks = new Matrix<ChunkBehaviour>(size, height, size);

        int sizeMin = Mathf.Min(size, m_grid.Size());
        int heightMin = Mathf.Min(height, m_grid.Height());

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    var pos = new Vector3Int(i, j, k);

                    if (i < sizeMin && j < heightMin && k < sizeMin)
                    {
                        var oldChunk = m_grid.Get(pos);
                        var newChunk = newGrid.Get(pos);

                        for (int x = 0; x < Grid.ChunkSize; x++)
                        {
                            for (int y = 0; y < Grid.ChunkSize; y++)
                            {
                                for (int z = 0; z < Grid.ChunkSize; z++)
                                {
                                    newChunk.Set(x, y, z, oldChunk.Get(x, y, z));
                                }
                            }
                        }
                    }
                    else if (populateNewChunkCallback != null)
                    {
                        var newChunk = newGrid.Get(pos);
                        populateNewChunkCallback(newChunk, pos);
                    }
                }
            }
        }

        int sizeMax = Mathf.Max(size, m_grid.Size());
        int heightMax = Mathf.Max(height, m_grid.Height());

        m_grid = newGrid;
        var oldChunks = m_chunks;
        m_chunks = newChunks;

        for (int i = 0; i < sizeMax; i++)
        {
            for (int j = 0; j < heightMax; j++)
            {
                for (int k = 0; k < sizeMax; k++)
                {
                    var pos = new Vector3Int(i, j, k);

                    if (i < sizeMin && j < heightMin && k < sizeMin)
                    {
                        var chunk = oldChunks.Get(i, j, k);
                        chunk.SetChunk(m_grid, pos);
                        m_chunks.Set(i, j, k, chunk);
                    }
                    else if (i < size && j < height && k < size)
                        CreateOneChunk(pos);
                    else
                    {
                        var chunk = oldChunks.Get(i, j, k);
                        Destroy(chunk.gameObject);
                    }
                }
            }
        }
    }
}
