using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public struct NavigationQueryResult
{
    public Vector3Int nextPos;
    public Vector2Int targetPos;
}

public class NavigationSurface
{
    struct NavigationElement
    {
        public int height;
        public Vector2Int nextPos;
        public Vector2Int leftPos;
        public Vector2Int rightPos;
        public Vector2Int target;
        public float distance;
    }

    NavigationProfile m_profile;

    public NavigationProfile profile { get { return m_profile; } set { m_profile = value; Rebuild(); } }

    static readonly object m_navigationGridLock = new object();
    Matrix<NavigationElement> m_navigationGrid;

    static readonly object m_jobStateLock = new object();
    bool m_jobPended = false;
    bool m_jobStarted = false;
    bool m_jobNeedRestart = false;
    Matrix<NavigationElement> m_pendingNavigationGrid;
    Grid m_grid;

    public void Rebuild()
    {
        lock (m_jobStateLock)
        {
            if (m_jobStarted)
            {
                m_jobNeedRestart = true;
                return;
            }
            if (m_jobPended)
                return;
        }

        m_grid = Event<GetGridEvent>.Broadcast(new GetGridEvent()).grid;

        ThreadPool.StartJob(RebuildJob, OnJobDone, 1, this);
    }

    void RebuildJob()
    {
        if (m_grid == null)
            return;

        int size = GridEx.GetRealSize(m_grid);

        var navGrid = new Matrix<NavigationElement>(size, size);
        var defaultNavElem = new NavigationElement();
        defaultNavElem.height = -1;
        defaultNavElem.nextPos = new Vector2Int(-1, -1);
        defaultNavElem.leftPos = new Vector2Int(-1, -1);
        defaultNavElem.rightPos = new Vector2Int(-1, -1);
        defaultNavElem.target = new Vector2Int(-1, -1);
        defaultNavElem.distance = -1;
        navGrid.SetAll(defaultNavElem);

        Matrix<int> heights = new Matrix<int>(size, size);

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                int height = GridEx.GetHeight(m_grid, new Vector2Int(i, j));
                var block = GridEx.GetBlock(m_grid, new Vector3Int(i, height, j));

                bool canWalk = block.type == BlockType.ground;
                if (m_profile.canWalkOnWater && block.type == BlockType.water)
                    canWalk = true;

                if (!canWalk)
                    heights.Set(i, j, -1);
                else heights.Set(i, j, height);
            }
        }

        MakeNavToTower(navGrid, heights);

        if (BuildingList.instance != null)
        {
            Team team = TeamEx.GetOppositeTeam(m_profile.team);
            var buildings = BuildingList.instance.GetAllBuilding(team);

            foreach(var b in buildings)
            {
                if (b.GetBuildingType() == BuildingType.Tower)
                    continue;

                MakeNavToBuilding(navGrid, heights, b, m_profile.buildingDetectionDistance);
            }
        }

        AddLeftRightPath(navGrid, heights);

        m_pendingNavigationGrid = navGrid;
    }

    void MakeNavToTower(Matrix<NavigationElement> navGrid, Matrix<int> heights)
    {
        if (BuildingList.instance == null)
            return;

        Team team = TeamEx.GetOppositeTeam(m_profile.team);

        var towers = BuildingList.instance.GetAllBuilding(BuildingType.Tower, team);

        if (towers.Count == 0)
            return;

        MakeNavToBuilding(navGrid, heights, towers[0]);
    }
    
    struct OpenPos
    {
        public Vector2Int previous;
        public Vector2Int current;
        public float distance;
    }

    void MakeNavToBuilding(Matrix<NavigationElement> navGrid, Matrix<int> heights, BuildingBase b, int maxDistance = -1)
    {
        if (maxDistance == 0)
            return;

        List<OpenPos> openPos = new List<OpenPos>();

        var buildingPos = b.GetGroundCenterThread();
        Vector2Int startPos = new Vector2Int(Mathf.RoundToInt(buildingPos.x), Mathf.RoundToInt(buildingPos.z));
        OpenPos startOpenPos = new OpenPos();
        startOpenPos.current = startPos;
        startOpenPos.previous = new Vector2Int(-1, -1);
        startOpenPos.distance = 0;
        openPos.Add(startOpenPos);

        Matrix<float> exploredDistance = new Matrix<float>(navGrid.width, navGrid.depth);
        exploredDistance.SetAll(-1);
        exploredDistance.Set(startPos.x, startPos.y, 0);

        NavigationElement startElem = new NavigationElement();
        startElem.distance = 0;
        startElem.height = heights.Get(startPos.x, startPos.y);
        startElem.nextPos = new Vector2Int(-1, -1);
        startElem.leftPos = new Vector2Int(-1, -1);
        startElem.rightPos = new Vector2Int(-1, -1);
        startElem.target = startPos;
        navGrid.Set(startPos.x, startPos.y, startElem);

        while (openPos.Count > 0)
        {
            var elem = openPos[0];
            openPos.RemoveAt(0);

            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;

                    Vector2Int newPos = elem.current + new Vector2Int(i, j);
                    Vector2Int newLoopPos = GridEx.GetRealPosFromLoop(m_grid, newPos);
                    if (newLoopPos.x != newPos.x && !m_grid.LoopX())
                        continue;
                    if (newLoopPos.y != newPos.y && !m_grid.LoopZ())
                        continue;
                    
                    float cost = 1;
                    if (!IsValidMove(heights, newLoopPos, elem.current, out cost))
                        continue;

                    float dist = 1;
                    if (i != 0 && j != 0)
                        dist = 1.5f;
                    dist *= cost;
                    dist += elem.distance;

                    float exp = exploredDistance.Get(newLoopPos.x, newLoopPos.y);
                    if (exp >= 0 && exp < dist)
                        continue;

                    var currentElem = navGrid.Get(newLoopPos.x, newLoopPos.y);
                    if (currentElem.distance >= 0 && currentElem.distance <= dist)
                        continue;

                    if (maxDistance >= 0 && dist > maxDistance)
                        continue;

                    OpenPos newOpenPos = new OpenPos();
                    newOpenPos.current = newLoopPos;
                    newOpenPos.previous = elem.current;
                    newOpenPos.distance = dist;
                    bool added = false;
                    for(int k = 0; k < openPos.Count; k++)
                    {
                        if(openPos[k].distance > newOpenPos.distance)
                        {
                            openPos.Insert(k, newOpenPos);
                            added = true;
                            break;
                        }
                    }    
                    if(!added)
                        openPos.Add(newOpenPos);

                    NavigationElement newElem = new NavigationElement();
                    newElem.distance = dist;
                    newElem.height = heights.Get(newLoopPos.x, newLoopPos.y);
                    newElem.nextPos = elem.current;
                    newElem.leftPos = new Vector2Int(-1, -1);
                    newElem.rightPos = new Vector2Int(-1, -1);
                    newElem.target = startPos;
                    navGrid.Set(newLoopPos.x, newLoopPos.y, newElem);

                    exploredDistance.Set(newLoopPos.x, newLoopPos.y, dist);
                }
            }
        }
    }

    void AddLeftRightPath(Matrix<NavigationElement> navGrid, Matrix<int> heights)
    {
        for(int i = 0; i < navGrid.width; i++)
        {
            for(int j = 0; j < navGrid.depth; j++)
            {
                if (heights.Get(i, j) < 0)
                    continue;

                var elem = navGrid.Get(i, j);
                if (elem.nextPos.x < 0 || elem.nextPos.y < 0)
                    continue;

                if (elem.distance <= m_profile.minSideDistance)
                    continue;

                //loop
                Vector2Int dir = elem.nextPos - new Vector2Int(i, j);
                if (dir.x > 1)
                    dir.x = -1;
                if (dir.x < -1)
                    dir.x = 1;
                if (dir.y > 1)
                    dir.y = -1;
                if (dir.y < -1)
                    dir.y = 1;

                var leftDir = GetLeftDir(dir);
                if(leftDir.x != 0 || leftDir.y != 0)
                {
                    Vector2Int end;
                    if(GetValidEnd(heights, new Vector2Int(i, j), leftDir, out end))
                    {
                        if (heights.Get(end.x, end.y) >= 0)
                            elem.leftPos = end;
                    }
                }

                var rightDir = GetRightDir(dir);
                if (rightDir.x != 0 || rightDir.y != 0)
                {
                    Vector2Int end;
                    if (GetValidEnd(heights, new Vector2Int(i, j), rightDir, out end))
                    {
                        if (heights.Get(end.x, end.y) >= 0)
                            elem.rightPos = end;
                    }
                }

                navGrid.Set(i, j, elem);
            }
        }
    }

    bool GetValidEnd(Matrix<int> heights, Vector2Int current, Vector2Int dir, out Vector2Int pos)
    {
        pos = new Vector2Int(-1, -1);

        var end = current + dir;
        Vector2Int endLoop = GridEx.GetRealPosFromLoop(m_grid, end);
        if (endLoop.x != end.x && !m_grid.LoopX())
            return false;
        if (endLoop.y != end.y && !m_grid.LoopZ())
            return false;

        float cost = 0;
        if (!IsValidMove(heights, current, end, out cost))
            return false;

        pos = end;
        return true;
    }

    static Vector2Int GetLeftDir(Vector2Int dir)
    {
        if (dir.x > 1)
            dir.x = 1;
        if (dir.x < -1)
            dir.x = -1;
        if (dir.y > 1)
            dir.y = 1;
        if (dir.y < -1)
            dir.y = -1;

        if (dir.x == 0 && dir.y == 0)
            return Vector2Int.zero;

        if (dir.x == 0 && dir.y == 1)
            return new Vector2Int(1, 1);
        if (dir.x == 1 && dir.y == 1)
            return new Vector2Int(1, 0);
        if (dir.x == 1 && dir.y == 0)
            return new Vector2Int(1, -1);
        if (dir.x == 1 && dir.y == -1)
            return new Vector2Int(0, -1);
        if (dir.x == 0 && dir.y == -1)
            return new Vector2Int(-1, -1);
        if (dir.x == -1 && dir.y == -1)
            return new Vector2Int(-1, 0);
        if (dir.x == -1 && dir.y == 0)
            return new Vector2Int(-1, 1);
        if (dir.x == -1 && dir.y == 1)
            return new Vector2Int(0, 1);
        return Vector2Int.zero;
    }

    static Vector2Int GetRightDir(Vector2Int dir)
    {
        if (dir.x > 1)
            dir.x = 1;
        if (dir.x < -1)
            dir.x = -1;
        if (dir.y > 1)
            dir.y = 1;
        if (dir.y < -1)
            dir.y = -1;

        if(dir.x == 0 && dir.y == 0)
            return Vector2Int.zero;

        if (dir.x == 0 && dir.y == 1)
            return new Vector2Int(-1, 1);
        if (dir.x == -1 && dir.y == 1)
            return new Vector2Int(-1, 0);
        if (dir.x == -1 && dir.y == 0)
            return new Vector2Int(-1, -1);
        if (dir.x == -1 && dir.y == -1)
            return new Vector2Int(0, -1);
        if (dir.x == 0 && dir.y == -1)
            return new Vector2Int(1, -1);
        if (dir.x == 1 && dir.y == -1)
            return new Vector2Int(1, 0);
        if (dir.x == 1 && dir.y == 0)
            return new Vector2Int(1, 1);
        if (dir.x == 1 && dir.y == 1)
            return new Vector2Int(0, 1);
        return Vector2Int.zero;
    }

    bool IsValidMove(Matrix<int> heights, Vector2Int start, Vector2Int end, out float cost)
    {
        cost = 1;

        int startHeight = GetHeightAt(heights, start);
        int endHeight = GetHeightAt(heights, end);

        if (startHeight < 0 || endHeight < 0)
            return false;

        int heightDiff = endHeight - startHeight;
        if (heightDiff < 0)
        {
            if (m_profile.fallStep < -heightDiff)
                return false;
            cost = m_profile.fallCost;
        }
        if (heightDiff > 0)
        {
            if (m_profile.climbStep < heightDiff)
                return false;
            cost = m_profile.climbCost;
        }

        // need test diagonals
        if(start.x != end.x && start.y != end.y && m_profile.radius == 1)
        {
            Vector2Int pos1 = new Vector2Int(start.x, end.y);
            Vector2Int pos2 = new Vector2Int(end.x, start.y);
            float tempCost = 0;
            if (IsValidMove(heights, start, pos1, out tempCost) && IsValidMove(heights, pos1, end, out tempCost))
                return true;
            if (IsValidMove(heights, start, pos2, out tempCost) && IsValidMove(heights, pos2, end, out tempCost))
                return true;
            return false;
        }

        return true;
    }

    int GetHeightAt(Matrix<int> heights, Vector2Int pos)
    {
        int height = -1;

        for(int i = -m_profile.radius + 1; i <= m_profile.radius - 1; i++)
        {
            for (int j = -m_profile.radius + 1; j <= m_profile.radius - 1; j++)
            {
                Vector2Int testPos = pos + new Vector2Int(i, j);
                Vector2Int testLoopPos = GridEx.GetRealPosFromLoop(m_grid, testPos);
                if (testLoopPos.x != testPos.x && !m_grid.LoopX())
                    continue;
                if (testLoopPos.y != testPos.y && !m_grid.LoopZ())
                    continue;

                int newHeigt = heights.Get(testLoopPos.x, testLoopPos.y);
                if (newHeigt < 0)
                    return -1;

                if (newHeigt > height)
                    height = newHeigt;
            }
        }

        return height;
    }

    void OnJobDone()
    {
        lock(m_navigationGridLock)
        {
            m_navigationGrid = m_pendingNavigationGrid;
            m_pendingNavigationGrid = null;
        }

        bool restart = false;

        lock (m_jobStateLock)
        { 
            m_jobStarted = false;
            m_jobPended = false;
            restart = m_jobNeedRestart;
            m_jobNeedRestart = false;
        }

        if (restart)
            Rebuild();
    }

    public NavigationQueryResult QueryNext(Vector2Int pos, int seed, float deviation)
    {
        NavigationQueryResult result = new NavigationQueryResult();
        result.nextPos = new Vector3Int(pos.x, -1, pos.y);

        lock (m_navigationGridLock)
        {
            if (m_navigationGrid == null)
                return result;

            if (pos.x < 0 || pos.x >= m_navigationGrid.width || pos.y < 0 || pos.y >= m_navigationGrid.depth)
                return result;

            NavigationElement elem = m_navigationGrid.Get(pos.x, pos.y);

            if (elem.nextPos.x < 0 || elem.nextPos.y < 0)
                return result;

            RandomHash hash = new RandomHash(seed);
            hash.Set(pos.x, pos.y);

            bool side = Rand.BernoulliDistribution(Mathf.Abs(deviation), hash);
            if(side)
            {
                if (deviation > 0 && elem.rightPos.x >= 0 && elem.rightPos.y >= 0)
                    result.nextPos = new Vector3Int(elem.rightPos.x, -1, elem.rightPos.y);
                else if (deviation < 0 && elem.leftPos.x >= 0 && elem.leftPos.y >= 0)
                    result.nextPos = new Vector3Int(elem.leftPos.x, -1, elem.leftPos.y);
                else result.nextPos = new Vector3Int(elem.nextPos.x, -1, elem.nextPos.y);
            }
            else result.nextPos = new Vector3Int(elem.nextPos.x, -1, elem.nextPos.y);

            result.nextPos.y = m_navigationGrid.Get(result.nextPos.x, result.nextPos.z).height;
        }

        return result;
    }

    public int GetHeight(Vector2Int pos)
    {
        lock (m_navigationGridLock)
        {
            if (m_navigationGrid == null)
                return -1;

            if (pos.x < 0 || pos.x >= m_navigationGrid.width || pos.y < 0 || pos.y >= m_navigationGrid.depth)
                return -1;

            var elem = m_navigationGrid.Get(pos.x, pos.y);
            return elem.height;
        }
    }

    public void DebugDrawGrid()
    {
        lock (m_navigationGridLock)
        {
            if (m_navigationGrid == null)
                return;

            for(int i = 0; i < m_navigationGrid.width; i++)
            {
                for(int j = 0; j < m_navigationGrid.depth; j++)
                {
                    var elem = m_navigationGrid.Get(i, j);

                    if(elem.nextPos.x >= 0 && elem.nextPos.y >= 0)
                    {
                        Vector3 pos = new Vector3(i, elem.height, j);
                        pos.y += 0.6f;
                        var nextElem = m_navigationGrid.Get(elem.nextPos.x, elem.nextPos.y);

                        Vector2Int nextPosI = elem.nextPos;
                        Vector2Int offset = nextPosI - new Vector2Int(i, j);
                        if (offset.x < -1)
                            nextPosI.x = i + 1;
                        if (offset.x > 1)
                            nextPosI.x = i - 1;
                        if (offset.y < -1)
                            nextPosI.y = j + 1;
                        if (offset.y > 1)
                            nextPosI.y = i - 1;

                        Vector3 nextPos = new Vector3(nextPosI.x, nextElem.height, nextPosI.y);
                        nextPos.y += 0.6f;

                        DebugDraw.Line(pos, nextPos, Color.blue);

                        if (elem.leftPos.x >= 0 && elem.leftPos.y >= 0)
                        {
                            Vector2Int leftPosI = elem.leftPos;
                            offset = leftPosI - new Vector2Int(i, j);
                            if (offset.x < -1)
                                leftPosI.x = i + 1;
                            if (offset.x > 1)
                                leftPosI.x = i - 1;
                            if (offset.y < -1)
                                leftPosI.y = j + 1;
                            if (offset.y > 1)
                                leftPosI.y = i - 1;

                            nextPos = new Vector3(leftPosI.x, nextElem.height, leftPosI.y);
                            nextPos.y += 0.7f;
                            pos.y += 0.1f;

                            DebugDraw.Line(pos, nextPos, Color.cyan);
                        }

                        if (elem.rightPos.x >= 0 && elem.rightPos.y >= 0)
                        {
                            Vector2Int rightPosI = elem.rightPos;
                            offset = rightPosI - new Vector2Int(i, j);
                            if (offset.x < -1)
                                rightPosI.x = i + 1;
                            if (offset.x > 1)
                                rightPosI.x = i - 1;
                            if (offset.y < -1)
                                rightPosI.y = j + 1;
                            if (offset.y > 1)
                                rightPosI.y = i - 1;

                            nextPos = new Vector3(rightPosI.x, nextElem.height, rightPosI.y);
                            nextPos.y += 0.8f;
                            pos.y += 0.1f;

                            DebugDraw.Line(pos, nextPos, Color.magenta);
                        }
                    }
                }
            }
        }
    }

    public void DebugDrawPathFromPos(Vector2Int pos, int seed, float deviation, Color color)
    {
        Vector2Int current = pos;
        int height = GetHeight(current);
        if (height < 0)
            return;

        for(int i = 0; i < 1000; i++)
        {
            var result = QueryNext(current, seed, deviation);

            if (result.nextPos.x == current.x && result.nextPos.z == current.y)
                break;

            Vector3 currentF = new Vector3(current.x, height + 0.6f, current.y);
            Vector3 nextF = new Vector3(result.nextPos.x, result.nextPos.y + 0.6f, result.nextPos.z);

            DebugDraw.Line(currentF, nextF, color);

            current = new Vector2Int(result.nextPos.x, result.nextPos.z);
            height = result.nextPos.y;
        }
    }
}
