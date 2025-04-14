using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GridEx
{
    public static int GetHeight(Grid grid, Vector2Int pos)
    {
        Vector3Int chunkIndex = Grid.PosToChunkIndex(new Vector3Int(pos.x, 0, pos.y));
        Vector3Int posInChunk = Grid.PosToPosInChunk(new Vector3Int(pos.x, 0, pos.y));

        for(int i = grid.Height() - 1; i >= 0; i--)
        {
            var chunk = grid.Get(new Vector3Int(chunkIndex.x, i, chunkIndex.z));
            if (chunk == null)
                return -1;

            for(int j = Grid.ChunkSize - 1; j >= 0; j--)
            {
                if(chunk.Get(posInChunk.x, j, posInChunk.z) != BlockType.air)
                {
                    Vector3Int outPos = Grid.PosInChunkToPos(new Vector3Int(chunkIndex.x, i, chunkIndex.z), new Vector3Int(posInChunk.x, j, posInChunk.z));
                    return outPos.y;
                }
            }
        }

        return -1;
    }

    public static BlockType GetBlock(Grid grid, Vector3Int pos)
    {
        Vector3Int chunkIndex = Grid.PosToChunkIndex(pos);
        Vector3Int posInChunk = Grid.PosToPosInChunk(pos);

        var chunk = grid.Get(chunkIndex);
        if (chunk == null)
            return BlockType.air;
        return chunk.Get(posInChunk.x, posInChunk.y, posInChunk.z);
    }

    public static void SetBlock(Grid grid, Vector3Int pos, BlockType block)
    {
        Vector3Int chunkIndex = Grid.PosToChunkIndex(pos);
        Vector3Int posInChunk = Grid.PosToPosInChunk(pos);

        var chunk = grid.Get(chunkIndex);
        if (chunk == null)
            return;
        chunk.Set(posInChunk.x, posInChunk.y, posInChunk.z, block);
    }

    public static void GetNearMatrix(Grid grid, Vector3Int pos, NearMatrix3<BlockType> mat)
    {
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                for(int k = -1; k <= 1; k++)
                {
                    Vector3Int newPos = pos + new Vector3Int(i, j, k);

                    mat.Set(GetBlock(grid, newPos), i, j, k);
                }
            }
        }
    }

    public static void GetLocalMatrix(Grid grid, Vector3Int pos, Matrix<BlockType> mat)
    {
        Vector3Int maxPos = pos + mat.size - new Vector3Int(1, 1, 1);

        Vector3Int minIndex = Grid.PosToChunkIndex(pos);
        Vector3Int maxIndex = Grid.PosToChunkIndex(maxPos);

        Vector3Int minInChunk = Grid.PosToPosInChunk(pos);
        Vector3Int maxInChunk = Grid.PosToPosInChunk(maxPos);

        for(int i = minIndex.x; i <= maxIndex.x; i++)
        {
            for(int j = minIndex.y; j <= maxIndex.y; j++)
            {
                for(int k = minIndex.z; k <= maxIndex.z; k++)
                {
                    var chunk = grid.Get(new Vector3Int(i, j, k));

                    Vector3Int min = new Vector3Int(0, 0, 0);
                    Vector3Int max = new Vector3Int(Grid.ChunkSize - 1, Grid.ChunkSize - 1, Grid.ChunkSize - 1);

                    if (i == minIndex.x)
                        min.x = minInChunk.x;
                    if (i == maxIndex.x)
                        max.x = maxInChunk.x;
                    if (j == minIndex.y)
                        min.y = minInChunk.y;
                    if (j == maxIndex.y)
                        max.y = maxInChunk.y;
                    if (k == minIndex.z)
                        min.z = minInChunk.z;
                    if (k == maxIndex.z)
                        max.z = maxInChunk.z;

                    for(int x = min.x; x <= max.x; x++)
                    {
                        for(int y = min.y; y <= max.y; y++)
                        {
                            for(int z = min.z; z <= max.z; z++)
                            {
                                var realPos = Grid.PosInChunkToPos(new Vector3Int(i, j, k), new Vector3Int(x, y, z)) - pos;

                                if (chunk == null)
                                    mat.Set(realPos.x, realPos.y, realPos.z, BlockType.air);
                                else mat.Set(realPos.x, realPos.y, realPos.z, chunk.Get(x, y, z));
                            }
                        }
                    }

                }
            }
        }
    }

    public static int GetRealSize(Grid grid)
    {
        return grid.Size() * Grid.ChunkSize;
    }

    public static int GetRealHeight(Grid grid)
    {
        return grid.Height() * Grid.ChunkSize;
    }

    public static Vector3Int GetRealPosFromLoop(Grid grid, Vector3Int pos)
    {
        int size = GetRealSize(grid);

        return new Vector3Int(LoopPos(pos.x, size), pos.y, LoopPos(pos.z, size));
    }

    public static Vector3Int GetPosFromLoop(Grid grid, Vector3Int pos)
    {
        int size = grid.Size();

        return new Vector3Int(LoopPos(pos.x, size), pos.y, LoopPos(pos.z, size));
    }

    static int LoopPos(int pos, int size)
    {
        if (pos >= 0)
            return pos % size;
        return -(((-pos + 1) % size) - 1);
    }
}

