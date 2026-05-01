using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaterRenderer
{
    const int pixelPerBlock = 4;
    Grid m_grid;
    Vector3Int m_chunkIndex;

    Color32[] m_pixels;

    bool m_ended = false;
    bool m_working = false;

    bool m_generating = false;
    readonly object m_generatingLock = new object();

    struct Segment
    {
        public Vector2 pos1;
        public Vector2 pos2;
    }

    struct Border
    {
        public List<Vector2> points;
    }

    struct Vertex
    {
        public Vector2 pos;
        public int dist;
    }

    struct Quad
    {
        public Vector2Int pos;
        public int[] vertices;
    }

    public WaterRenderer(Grid grid, Vector3Int index)
    {
        m_grid = grid;
        m_chunkIndex = index;
    }

    public Vector3Int GetChunkIndex()
    {
        return m_chunkIndex;
    }

    public void Start()
    {
        if (m_grid == null)
            return;

        if (m_working)
            return;

        m_working = true;
        m_ended = false;

        ThreadPool.StartJob(JobWorker, OnEndJob, 10, this);
    }

    public bool Ended()
    {
        return m_ended && !m_working;
    }

    public bool IsGenerating()
    {
        lock (m_generatingLock)
        {
            return m_generating;
        }
    }

    public Texture2D GetTexture()
    {
        lock (m_generatingLock)
        {
            if (m_generating)
                return null;
        }

        Texture2D t = new Texture2D(Grid.ChunkSize * pixelPerBlock, Grid.ChunkSize * pixelPerBlock);
        t.SetPixels32(m_pixels);

        return t;
    }

    void JobWorker()
    {
        lock (m_generatingLock)
        {
            m_generating = true;
        }

        int shoreDistance = Global.instance.blockDatas.waterShoreDistance;

        var mat = new Matrix<Block>(Grid.ChunkSize + 2 * shoreDistance, 1, Grid.ChunkSize + 2 * shoreDistance);
        var initialPos = Grid.PosInChunkToPos(m_chunkIndex, new Vector3Int(-shoreDistance, 0, -shoreDistance));

        GridEx.GetLocalMatrix(m_grid, initialPos, mat);

        List<Segment> allOutsideSegments = GetAllOutsideSegments(mat);

        m_pixels = new Color32[Grid.ChunkSize * pixelPerBlock * Grid.ChunkSize * pixelPerBlock];

        MakePixels(Grid.ChunkSize * pixelPerBlock, allOutsideSegments, mat);
    }

    List<Segment> GetAllOutsideSegments(Matrix<Block> mat)
    {
        int size = mat.width;

        List<Segment> allOutsideSegments = new List<Segment>();

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (mat.Get(i, 0, j).type != BlockType.ground)
                        continue;

                    var dir = RotationEx.ToVectorInt((Rotation)k);
                    if ((i == 0 && dir.x < 0) || (j == 0 && dir.y < 0) || (i == size - 1 && dir.x > 0) || (j == size - 1 && dir.y > 0))
                        continue;

                    if (mat.Get(i + dir.x, 0, j + dir.y).type != BlockType.water)
                        continue;

                    var othoDir = new Vector2Int(dir.y, -dir.x);
                    var segment = new Segment();
                    segment.pos1 = new Vector2(i + (dir.x - othoDir.x) / 2.0f, j + (dir.y - othoDir.y) / 2.0f);
                    segment.pos2 = new Vector2(i + (dir.x + othoDir.x) / 2.0f, j + (dir.y + othoDir.y) / 2.0f);
                    allOutsideSegments.Add(segment);
                }
            }
        }

        return allOutsideSegments;
    }

    void MakePixels(int size, List<Segment> segments, Matrix<Block> mat)
    {
        float maxSqrDist = Global.instance.blockDatas.waterShoreDistance;
        maxSqrDist *= maxSqrDist;

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                Vector2 pixPos = GetPixelPos(new Vector2Int(i, j));
                Vector2Int matPos = new Vector2Int(Mathf.RoundToInt(pixPos.x), Mathf.RoundToInt(pixPos.y));
                int pixIndex = GetPixelIndex(new Vector2Int(i, j));

                if (mat.Get(matPos.x, matPos.y).type != BlockType.water)
                    m_pixels[pixIndex] = new Color32(255, 255, 255, 255);
                else
                {
                    float bestLight = 0;
                    foreach(var s in segments)
                    {
                        float sqrDist = Utility.SqrDistanceToSegment(s.pos1, s.pos2, pixPos);
                        if(sqrDist < maxSqrDist)
                        {
                            float normDist = Mathf.Sqrt(sqrDist) / Global.instance.blockDatas.waterShoreDistance;
                            float light = 1 - normDist;
                            if (light > bestLight)
                                bestLight = light;
                        }
                    }
                    byte color = (byte)(bestLight * 255);
                    m_pixels[pixIndex] = new Color32(color, color, color, color);
                }
            }
        }
    }

    int GetPixelIndex(Vector2Int pix)
    {
        return pix.x + pix.y * Grid.ChunkSize * pixelPerBlock;
    }

    //relative to the chunk matrix
    Vector2 GetPixelPos(Vector2Int pix)
    {
        int shoreDistance = Global.instance.blockDatas.waterShoreDistance;
        float pixelSize = 1.0f / pixelPerBlock;

        pix.x += shoreDistance * pixelPerBlock;
        pix.y += shoreDistance * pixelPerBlock;

        Vector2 pos = new Vector2(pix.x * pixelSize + pixelSize / 2 - 0.5f, pix.y * pixelSize + pixelSize / 2 - 0.5f);

        return pos;
    }

    void OnEndJob()
    {
        m_working = false;
        m_ended = true;

        lock (m_generatingLock)
        {
            m_generating = false;
        }
    }
}
