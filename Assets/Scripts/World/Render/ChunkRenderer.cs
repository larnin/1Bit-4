using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkRenderer
{
    Grid m_grid;
    Vector3Int m_chunkIndex;
    bool m_haveGeneratedSomething = false;

    MeshParams<WorldVertexDefinition, ColliderVertexDefinition> m_meshParams;

    bool m_ended = false;
    bool m_working = false;

    public ChunkRenderer(Grid grid, Vector3Int index)
    {
        m_grid = grid;
        m_chunkIndex = index;
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

    public bool HaveGeneratedSomething()
    {
        return m_haveGeneratedSomething;
    }

    public void ApplyMesh(Mesh mesh)
    {

    }

    public void ApplyColliderMesh(Mesh mesh)
    {

    }

    void JobWorker()
    {

    }

    void OnEndJob()
    {
        m_working = false;
        m_ended = true;
    }
}
