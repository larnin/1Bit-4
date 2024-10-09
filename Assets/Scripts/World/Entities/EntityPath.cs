using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum EntityPathStatus
{
    Stopped,
    Generating,
    Following,
}

public class EntityPath
{
    Vector3Int m_current;
    Vector3Int m_target;
    EntityPathStatus m_status;

    readonly object m_pointsLock = new object();
    List<Vector3Int> m_points = new List<Vector3Int>();
    int m_currentPoint = 0;

    Vector3Int m_nextCurrent;
    Vector3Int m_nextTarget;
    bool m_nextTargetSet = false;

    public EntityPathStatus GetStatus()
    {
        return m_status;
    }

    public void SetTarget(Vector3 current, Vector3 target)
    {
        SetTarget(new Vector3Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Mathf.RoundToInt(current.z)),
            new Vector3Int(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Mathf.RoundToInt(target.z)));
    }

    public void SetTarget(Vector3Int current, Vector3Int target)
    {
        if(m_status != EntityPathStatus.Generating)
        {
            m_current = current;
            m_target = target;
            StartJob();
        }
        else if(target != m_target)
        {
            m_nextCurrent = current;
            m_nextTarget = target;
            m_nextTargetSet = true;
        }
    }

    public Vector3Int GetNextPoint(Vector3 pos)
    {
        Vector3Int posInt = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        lock (m_pointsLock)
        {
            if(m_nextTargetSet && m_status != EntityPathStatus.Generating)
            {
                m_current = m_nextCurrent;
                m_target = m_nextTarget;
                m_nextTargetSet = false;
                StartJob();
            }

            if (m_status != EntityPathStatus.Following)
                return posInt;

            if (m_currentPoint < 0 || m_currentPoint >= m_points.Count)
            {
                m_status = EntityPathStatus.Stopped;
                return posInt;
            }

            if (posInt == m_points[m_currentPoint])
            {
                m_currentPoint++;
                if (m_currentPoint >= m_points.Count)
                {
                    m_status = EntityPathStatus.Stopped;
                    return posInt;
                }
            }

            return m_points[m_currentPoint];
        }
    }

    void StartJob()
    {
        m_status = EntityPathStatus.Generating;
        ThreadPool.StartJob(GeneratePathJob, EndJob, 1, this);
    }

    class PathStep
    {
        public float weight;
        public float targetWeight;
        public Vector3Int pos;
        public int previousIndex;
    }

    void GeneratePathJob()
    {
        var test = m_target - m_current;
        if(Mathf.Abs(test.x) + Mathf.Abs(test.y) + Mathf.Abs(test.z) <= 1)
        {
            SetEmptyPath(m_target);
            return;
        }

        GetGridEvent grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        if(grid.grid == null)
        {
            SetEmptyPath(m_target);
            return;
        }
        Matrix<bool> visitedPoint = new Matrix<bool>(GridEx.GetRealSize(grid.grid), GridEx.GetRealHeight(grid.grid), GridEx.GetRealSize(grid.grid));
        visitedPoint.SetAll(false);
        List<PathStep> path = new List<PathStep>(GridEx.GetRealSize(grid.grid) * GridEx.GetRealSize(grid.grid));
        List<PathStep> openList = new List<PathStep>();

        PathStep startStep = new PathStep();
        startStep.pos = m_current;
        startStep.previousIndex = -1;
        startStep.weight = 0;
        startStep.targetWeight = 0;
        openList.Add(startStep);
        visitedPoint.Set(m_current.x, m_current.y, m_current.z, true);

        while(openList.Count > 0)
        {
            var step = openList[0];
            openList.RemoveAt(0);

            path.Add(step);

            bool found = false;

            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    for(int k = -1; k <= 1; k++)
                    {
                        if (i == 0 && k == 0)
                            continue;

                        Vector3Int target = step.pos + new Vector3Int(i, j, k);
                        
                        //if (path.Exists((x) => { return x.pos == target; }))
                        //    continue;
                        //if (openList.Exists((x) => { return x.pos == target; }))
                        //    continue;

                        if (!IsPosValid(grid.grid, target))
                            continue;
                        if (visitedPoint.Get(target.x, target.y, target.z))
                            continue;

                        PathStep newStep = new PathStep();
                        newStep.pos = target;
                        newStep.previousIndex = path.Count - 1;
                        newStep.weight = step.weight + (new Vector3Int(i, j, k)).magnitude;
                        newStep.targetWeight = (m_target - target).magnitude;

                        if(target == m_target)
                        {
                            found = true;
                            path.Add(newStep);
                            break;
                        }

                        Insert(openList, newStep);
                        visitedPoint.Set(target.x, target.y, target.z, true);
                    }

                    if (found)
                        break;
                }
                if (found)
                    break;
            }
            if (found)
                break;
        }

        List<Vector3Int> newPath = new List<Vector3Int>();

        int index = path.Count - 1;
        while(index > 0)
        {
            newPath.Insert(0, path[index].pos);
            index = path[index].previousIndex;
        }

        lock(m_pointsLock)
        {
            m_points = newPath;
            m_currentPoint = 0;
        }
    }

    bool IsPosValid(Grid grid, Vector3Int pos)
    {
        if (BuildingList.instance != null && BuildingList.instance.GetBuildingAt(pos) != null)
            return false;

        var block = GridEx.GetBlock(grid, pos);
        if (block != BlockType.air)
            return false;

        var ground = GridEx.GetBlock(grid, new Vector3Int(pos.x, pos.y - 1, pos.z));
        if (ground == BlockType.air)
            return false;

        return true;
    }

    void Insert(List<PathStep> steps, PathStep step)
    {
        float weight = step.weight + step.targetWeight;

        for(int i = 0; i < steps.Count; i++)
        {
            var s = steps[i];
            if(s.weight + s.targetWeight > weight)
            {
                steps.Insert(i, step);
                return;
            }
        }

        steps.Add(step);
    }

    void SetEmptyPath(Vector3Int pos)
    {
        lock(m_pointsLock)
        {
            m_points.Clear();
            m_points.Add(pos);
        }
    }

    void EndJob()
    {
        lock (m_pointsLock)
        {
            m_status = EntityPathStatus.Following;
        }
    }
}
