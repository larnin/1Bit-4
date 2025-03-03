using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    class BuildingChunk
    {
        public List<BuildingBase> buildings = new List<BuildingBase>();
        public Vector2Int pos;
    }

    List<BuildingBase> m_buildings = new List<BuildingBase>();
    Dictionary<ulong, BuildingBase> m_buildingsPos = new Dictionary<ulong, BuildingBase>();
    Dictionary<ulong, BuildingChunk> m_chunks = new Dictionary<ulong, BuildingChunk>();

    SubscriberList m_subscriberList = new SubscriberList();

    static BuildingList m_instance = null;
    public static BuildingList instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;

        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnGenerationEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;

        m_subscriberList.Unsubscribe();
    }

    public void Register(BuildingBase building)
    {
        m_buildings.Add(building);

        var bounds = building.GetBounds();
        var min = bounds.min;
        var max = bounds.max;

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    m_buildingsPos.Add(Utility.PosToID(new Vector3Int(x, y, z)), building);
                }
            }
        }

        AddInChunks(building);

        if (building.GetTeam() == Team.Player && ConnexionSystem.instance != null)
            ConnexionSystem.instance.OnBuildingAdd(building);

        Event<BuildingListAddEvent>.Broadcast(new BuildingListAddEvent(building));
    }

    public void UnRegister(BuildingBase building)
    {
        m_buildings.Remove(building);

        var bounds = building.GetBounds();
        var min = bounds.min;
        var max = bounds.max;

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    m_buildingsPos.Remove(Utility.PosToID(new Vector3Int(x, y, z)));
                }
            }
        }

        RemoveFromChunks(building);

        if (building.GetTeam() == Team.Player && ConnexionSystem.instance != null)
            ConnexionSystem.instance.OnBuildingRemove(building);

        Event<BuildingListRemoveEvent>.Broadcast(new BuildingListRemoveEvent(building));
    }

    public int GetBuildingNb()
    {
        return m_buildings.Count;
    }

    public BuildingBase GetBuildingFromIndex(int index)
    {
        if (index < 0 || index >= m_buildings.Count)
            return null;

        return m_buildings[index];
    }

    public BuildingBase GetFirstBuilding(BuildingType type)
    {
        foreach (var building in m_buildings)
        {
            if (building.GetBuildingType() == type)
                return building;
        }

        return null;
    }

    List<BuildingBase> GetAllBuilding(Func<BuildingBase, bool> condition)
    {
        List<BuildingBase> buildings = new List<BuildingBase>();

        foreach (var building in m_buildings)
        {
            if(condition != null && condition(building))
                buildings.Add(building);
        }

        return buildings;
    }

    public List<BuildingBase> GetAllBuilding(BuildingType type)
    {
        return GetAllBuilding(x => { return x.GetBuildingType() == type; });
    }

    public List<BuildingBase> GetAllBuilding(Team team)
    {
        return GetAllBuilding(x => { return x.GetTeam() == team; });
    }

    public List<BuildingBase> GetAllBuilding(BuildingType type,  Team team)
    {
        return GetAllBuilding(x => { return x.GetBuildingType() == type && x.GetTeam() == team; });
    }

    BuildingBase GetNearestBuilding(Vector3 pos, Func<BuildingBase, bool> condition)
    {
        if (m_buildings.Count < 32)
            return GetNearestBuildingNaive(pos, condition);
        return GetNearestBuildingOptimised(pos, condition);
    }

    BuildingBase GetNearestBuildingNaive(Vector3 pos, Func<BuildingBase, bool> condition)
    {
        float bestDistance = 0;
        BuildingBase bestBuilding = null;

        foreach (var building in m_buildings)
        {
            if (condition != null && !condition(building))
                continue;

            Vector3 buildingPos = building.GetPos();
            Vector3 buildingSize = building.GetSize();

            float dist = GetSqrDistance(pos, buildingPos, buildingSize);

            if (dist < bestDistance || bestBuilding == null)
            {
                bestBuilding = building;
                bestDistance = dist;
            }
        }

        return bestBuilding;
    }

    BuildingBase GetNearestBuildingOptimised(Vector3 pos, Func<BuildingBase, bool> condition)
    {
        GetGridEvent e = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(e);
        if (e.grid == null)
            return null;

        int size = e.grid.Size();

        Vector2Int posInt = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));

        //first find the first nearest building by browsing the map with a snail patern
        int index = 0;
        Vector2Int currentCase = Grid.PosToChunkIndex(posInt);
        Rotation direction = Rotation.rot_0;
        int stepNb = 1;
        int remainingStep = 1;
        bool firstPhase = true;

        BuildingBase nearestBuilding = null;
        HashSet<ulong> visitedChunks = new HashSet<ulong>(m_chunks.Count);

        while(index < m_chunks.Count)
        {
            if(currentCase.x >= 0 && currentCase.y >= 0 && currentCase.x < size && currentCase.y < size)
            {
                visitedChunks.Add(Utility.PosToID(currentCase));
                nearestBuilding = GetNearestBuilding(GetChunk(currentCase), pos, condition);
                if (nearestBuilding != null)
                    break;
                index++;
            }

            currentCase += RotationEx.ToVectorInt(direction);
            remainingStep--;

            if(remainingStep <= 0)
            {
                //snail browse
                direction = RotationEx.Add(direction, Rotation.rot_90);
                if (!firstPhase)
                    stepNb++;
                remainingStep = stepNb;
                firstPhase = !firstPhase;
            }
        }

        if (nearestBuilding == null)
            return nearestBuilding;

        //next get all the chunk in the radius of the nearest element
        float dist = (nearestBuilding.GetPos() - pos).MagnitudeXZ();
        float sqrDist = dist * dist;

        List<BuildingChunk> toCheckChunks = new List<BuildingChunk>(m_chunks.Count);
        int radiusChunk = Mathf.CeilToInt(dist / Grid.ChunkSize);

        currentCase = Grid.PosToChunkIndex(posInt);
        int minX = Mathf.Max(0, currentCase.x - radiusChunk);
        int maxX = Mathf.Min(size - 1, currentCase.x + radiusChunk);
        int minY = Mathf.Max(0, currentCase.y - radiusChunk);
        int maxY = Mathf.Min(size - 1, currentCase.y + radiusChunk);

        for(int i = minX; i <= maxX; i++)
        {
            for(int j = minY; j <= maxY; j++)
            {
                if (visitedChunks.Contains(Utility.PosToID(new Vector2Int(i, j))))
                    continue;

                var chunk = GetChunk(new Vector2Int(i, j));
                int minChunkX = i * Grid.ChunkSize;
                int maxChunkX = minChunkX + Grid.ChunkSize - 1;
                int minChunkY = j * Grid.ChunkSize;
                int maxChunkY = minChunkY + Grid.ChunkSize - 1;

                Vector2Int chunkPoint = new Vector2Int(minChunkX, minChunkY);
                if(posInt.x > chunkPoint.x)
                {
                    if (posInt.x < maxChunkX)
                        chunkPoint.x = posInt.x;
                    else chunkPoint.x = maxChunkX;
                }
                if(posInt.y > chunkPoint.y)
                {
                    if (posInt.y < maxChunkY)
                        chunkPoint.y = posInt.y;
                    else chunkPoint.y = maxChunkY;
                }

                float chunkPointDist = (chunkPoint - posInt).sqrMagnitude;
                if (chunkPointDist < sqrDist)
                    toCheckChunks.Add(chunk);
            }
        }

        //finally, check if a target is better on this radius
        foreach(var c in toCheckChunks)
        {
            var b = GetNearestBuilding(c, pos, condition);
            if (b == null)
                continue;

            float newBuildingDist = (nearestBuilding.GetPos() - pos).sqrMagnitude;
            if(newBuildingDist < sqrDist)
            {
                sqrDist = newBuildingDist;
                nearestBuilding = b;
            }
        }

        return nearestBuilding;
    }

    static BuildingBase GetNearestBuilding(BuildingChunk chunk, Vector3 pos, Func<BuildingBase, bool> condition)
    {
        float bestDistance = 0;
        BuildingBase bestBuilding = null;

        foreach (var building in chunk.buildings)
        {
            if (condition != null && !condition(building))
                continue;

            Vector3 buildingPos = building.GetPos();
            Vector3 buildingSize = building.GetSize();

            float dist = GetSqrDistance(pos, buildingPos, buildingSize);

            if (dist < bestDistance || bestBuilding == null)
            {
                bestBuilding = building;
                bestDistance = dist;
            }
        }

        return bestBuilding;
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuilding(pos, x => { return Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, BuildingType type, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuilding(pos, x => { return x.GetBuildingType() == type && Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, Team team, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuilding(pos, x => { return x.GetTeam() == team && Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, BuildingType type, Team team, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuilding(pos, x => { return x.GetBuildingType() == type && x.GetTeam() == team && Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public BuildingBase GetBuildingAt(Vector3Int pos)
    {
        BuildingBase b;
        if (!m_buildingsPos.TryGetValue(Utility.PosToID(pos), out b))
            return null;
        return b;
    }

    static float GetSqrDistance(Vector3 pos, Vector3 itemPos, Vector3 itemSize)
    {
        Vector3 dir = itemPos - pos;
        if (dir.x > 0)
        {
            if (dir.x < itemSize.x)
                dir.x = 0;
            else dir.x -= itemSize.x;
        }
        if (dir.y > 0)
        {
            if (dir.y < itemSize.y)
                dir.y = 0;
            else dir.y -= itemSize.y;
        }
        if (dir.z > 0)
        {
            if (dir.z < itemSize.z)
                dir.z = 0;
            else dir.z -= itemSize.z;
        }

        return  dir.sqrMagnitude;
    }

    void OnGenerationEnd(GenerationFinishedEvent e)
    {
        InitializeBuildingChunks();
    }

    void InitializeBuildingChunks()
    {
        GetGridEvent e = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(e);

        int size = e.grid.Size();

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                var chunk = new BuildingChunk();
                chunk.pos = new Vector2Int(i, j);
                m_chunks.Add(Utility.PosToID(chunk.pos), chunk);
            }
        }

        foreach (var b in m_buildings)
            AddInChunks(b);
    }

    BuildingChunk GetChunk(Vector2Int pos)
    {
        var ID = Utility.PosToID(pos);
        BuildingChunk chunk = null;
        m_chunks.TryGetValue(ID, out chunk);

        return chunk;
    }

    BuildingChunk GetChunkAt(Vector3Int pos)
    {
        var chunkPos = Grid.PosToChunkIndex(pos);
        return GetChunk(new Vector2Int(chunkPos.x, chunkPos.z));
    }

    void AddInChunks(BuildingBase building)
    {
        var pos = building.GetPos();

        var chunk = GetChunkAt(pos);
        if (chunk != null)
            chunk.buildings.Add(building);
    }

    void RemoveFromChunks(BuildingBase building)
    {
        var pos = building.GetPos();

        var chunk = GetChunkAt(pos);
        if (chunk != null)
            chunk.buildings.Remove(building);
    }
}
