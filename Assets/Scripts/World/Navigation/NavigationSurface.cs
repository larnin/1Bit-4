using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        Matrix<bool> explored = new Matrix<bool>(navGrid.width, navGrid.height);
        explored.SetAll(false);
        explored.Set(startPos.x, startPos.y, true);

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
                    Vector2Int newLoopPos = GridEx.GetPosFromLoop(m_grid, newPos);
                    if (newLoopPos.x != newPos.x && !m_grid.LoopX())
                        continue;
                    if (newLoopPos.y != newPos.y && !m_grid.LoopZ())
                        continue;

                    if (explored.Get(newLoopPos.x, newLoopPos.y))
                        continue;

                    float dist = 1;
                    if (i != 0 && j != 0)
                        dist = 1.5f;

                    var currentElem = navGrid.Get(newLoopPos.x, newLoopPos.y);
                    if (currentElem.distance >= 0 && currentElem.distance <= dist)
                        continue;

                    if (maxDistance >= 0 && dist > maxDistance)
                        continue;

                    if (!IsValidMove(heights, newLoopPos, elem.current))
                        continue;

                    OpenPos newOpenPos = new OpenPos();
                    newOpenPos.current = newLoopPos;
                    newOpenPos.previous = elem.current;
                    newOpenPos.distance = elem.distance + dist;
                    bool added = false;
                    for(int k = 0; k < openPos.Count; k++)
                    {
                        if(openPos[k].distance < newOpenPos.distance)
                        {
                            openPos.Insert(k, newOpenPos);
                            added = true;
                            break;
                        }
                    }    
                    if(!added)
                        openPos.Add(startOpenPos);

                    NavigationElement newElem = new NavigationElement();
                    newElem.distance = dist;
                    newElem.height = heights.Get(startPos.x, startPos.y);
                    newElem.nextPos = elem.current;
                    newElem.leftPos = new Vector2Int(-1, -1);
                    newElem.rightPos = new Vector2Int(-1, -1);
                    newElem.target = startPos;
                    navGrid.Set(newLoopPos.x, newLoopPos.y, newElem);

                    explored.Set(newLoopPos.x, newLoopPos.y, true);
                }
            }
        }
    }

    bool IsValidMove(Matrix<int> heights, Vector2Int start, Vector2Int end)
    {
        int startHeight = GetHeightAt(heights, start);
        int endHeight = GetHeightAt(heights, end);

        if (startHeight < 0 || endHeight < 0)
            return false;

        int heightDiff = endHeight - startHeight;
        if (heightDiff < 0 && m_profile.fallStep < -heightDiff)
            return false;
        if (heightDiff > 0 && m_profile.climbStep < heightDiff)
            return false;

        // need test diagonals
        if(start.x != end.x && start.y != end.y && m_profile.radius == 1)
        {
            Vector2Int pos1 = new Vector2Int(start.x, end.y);
            Vector2Int pos2 = new Vector2Int(end.x, start.x);
            if (IsValidMove(heights, start, pos1) && IsValidMove(heights, pos1, end))
                return true;
            if (IsValidMove(heights, start, pos2) && IsValidMove(heights, pos2, end))
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
                Vector2Int testLoopPos = GridEx.GetPosFromLoop(m_grid, testPos);
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
}
