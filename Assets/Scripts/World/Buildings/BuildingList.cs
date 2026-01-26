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
    Dictionary<ulong, List<BuildingBase>> m_buildingsPos = new Dictionary<ulong, List<BuildingBase>>();
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
                    var id = Utility.PosToID(new Vector3Int(x, y, z));
                    if (!m_buildingsPos.ContainsKey(id))
                    {
                        var list = new List<BuildingBase>();
                        list.Add(building);
                        m_buildingsPos.Add(id, list);
                    }
                    else m_buildingsPos[id].Add(building);
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
                    var id = Utility.PosToID(new Vector3Int(x, y, z));
                    if(m_buildingsPos.ContainsKey(id))
                    {
                        var list = m_buildingsPos[id];
                        if (list != null)
                            list.Remove(building);
                        if (list.Count == 0 || list == null)
                            m_buildingsPos.Remove(id);
                    }
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

    public List<BuildingBase> GetAllBuilding()
    {
        List<BuildingBase> buildings = new List<BuildingBase>();

        foreach (var building in m_buildings)
            buildings.Add(building);

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
        float bestDistance = 0;
        BuildingBase bestBuilding = null;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return null;

        int x = grid.grid.LoopX() ? 1 : 0;
        int y = grid.grid.LoopZ() ? 1 : 0;
        var size = GridEx.GetRealSize(grid.grid);

        foreach (var building in m_buildings)
        {
            if (condition != null && !condition(building))
                continue;

            Vector3 buildingPos = building.GetPos();
            Vector3 buildingSize = building.GetSize();

            for (int i = -x; i <= x; i++)
            {
                for (int j = -y; j <= y; j++)
                {
                    Vector3 loopPos = buildingPos + new Vector3(i, 0, j) * size;

                    float dist = GetSqrDistance(pos, loopPos, buildingSize);

                    if (dist < bestDistance || bestBuilding == null)
                    {
                        bestBuilding = building;
                        bestDistance = dist;
                    }
                }
            }
        }

        return bestBuilding;
    }

    BuildingBase GetNearestBuildingInRadius(Vector3 pos, float radius, Func<BuildingBase, bool> condition)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return null;

        Vector2Int posMin = new Vector2Int(Mathf.FloorToInt(pos.x - radius), Mathf.FloorToInt(pos.z - radius));
        Vector2Int posMax = new Vector2Int(Mathf.CeilToInt(pos.x + radius), Mathf.CeilToInt(pos.z + radius));

        Vector2Int chunkMin = Grid.PosToChunkIndex(posMin);
        Vector2Int chunkMax = Grid.PosToChunkIndex(posMax);

        var size = grid.grid.Size();
        
        if(!grid.grid.LoopX())
        {
            if (posMin.x < 0)
                posMin.x = 0;
            if (posMax.x >= size)
                posMax.x = size - 1;
        }
        if(!grid.grid.LoopZ())
        {
            if (posMin.y < 0)
                posMin.y = 0;
            if (posMax.y >= size)
                posMax.y = size - 1;
        }

        BuildingBase best = null;
        float bestDistance = radius;

        for (int i = posMin.x; i <= posMax.x; i++)
        {
            for(int j = posMin.y; j <= posMax.y; j++)
            {
                Vector2Int chunkPos = GridEx.GetPosFromLoop(grid.grid, new Vector2Int(i, j));

                var b = GetNearestBuilding(GetChunk(chunkPos), pos, condition);
                if (b == null)
                    continue;

                Vector3 buildingPos = b.GetPos();
                Vector3 buildingSize = b.GetSize();

                float dist = GetSqrDistance(pos, buildingPos, buildingSize);

                if(dist < bestDistance)
                {
                    bestDistance = dist;
                    best = b;
                }
            }
        }

        return best;
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

    public BuildingBase GetNearestBuildingInRadius(Vector3 pos, float radius, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuildingInRadius(pos, radius, x => { return Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public BuildingBase GetNearestBuildingInRadius(Vector3 pos, float radius, BuildingType type, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuildingInRadius(pos, radius, x => { return x.GetBuildingType() == type && Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public BuildingBase GetNearestBuildingInRadius(Vector3 pos, float radius, Team team, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuildingInRadius(pos, radius, x => { return x.GetTeam() == team && Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public BuildingBase GetNearestBuildingInRadius(Vector3 pos, float radius, BuildingType type, Team team, AliveType alive = AliveType.NotSet)
    {
        return GetNearestBuildingInRadius(pos, radius, x => { return x.GetBuildingType() == type && x.GetTeam() == team && Utility.IsAliveFilter(x.gameObject, alive); });
    }

    public int GetBuildingNbAt(Vector3Int pos)
    {
        List<BuildingBase> list;
        if (!m_buildingsPos.TryGetValue(Utility.PosToID(pos), out list))
            return 0;
        if (list == null)
            return 0;
        return list.Count;
    }

    public BuildingBase GetFirstBuildingAt(Vector3Int pos)
    {
        return GetBuildingAt(pos);
    }

    public BuildingBase GetNextBuildingAt(Vector3Int pos, BuildingBase b)
    {
        List<BuildingBase> list;
        if (!m_buildingsPos.TryGetValue(Utility.PosToID(pos), out list))
            return null;
        if (list == null)
            return null;
        if (list.Count == 0)
            return null;

        if (b == null)
            return list[0];

        for(int i = 0; i < list.Count; i++)
        {
            if(list[i] == b)
            {
                if (i == list.Count - 1)
                    return list[0];
                return list[i + 1];
            }
        }

        return list[0];
    }

    public BuildingBase GetBuildingAt(Vector3Int pos, int index = 0)
    {
        List<BuildingBase> list;
        if (!m_buildingsPos.TryGetValue(Utility.PosToID(pos), out list))
            return null;
        if (list == null)
            return null;
        if (list.Count <= index || index < 0)
            return null;
        return list[index];
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
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        int size = grid.grid.Size();

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

    public void Clear()
    {
        //destroying elements can change the list
        var elements = m_buildings.ToList();
        m_buildings.Clear();

        foreach (var e in elements)
        {
            Destroy(e.gameObject);
        }
    }

    public void Load(JsonObject obj)
    {
        Clear();

        var jsonData = obj.GetElement("data");
        if (jsonData == null || !jsonData.IsJsonArray())
            return;

        var jsonArray = jsonData.JsonArray();
        foreach (var jsonElement in jsonArray)
        {
            if (jsonElement.IsJsonObject())
            {
                BuildingBase.Create(jsonElement.JsonObject());
            }
        }
    }

    public JsonObject Save()
    {
        JsonObject obj = new JsonObject();

        var jsonArray = new JsonArray();
        obj.AddElement("data", jsonArray);

        foreach (var b in m_buildings)
        {
            jsonArray.Add(b.Save());
        }

        return obj;
    }
}
