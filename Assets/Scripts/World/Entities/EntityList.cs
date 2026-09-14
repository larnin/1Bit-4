using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

public class EntityList : MonoBehaviour
{
    [SerializeField] int m_avoidanceResolution = 2;

    List<GameEntity> m_entities = new List<GameEntity>();
    Matrix<List<GameEntity>> m_chunks;
    Matrix<GameEntity> m_avoidanceMap;

    List<Vector3> m_tempVector = new List<Vector3>();

    static EntityList m_instance = null;
    public static EntityList instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    private void LateUpdate()
    {
        RefreshChunks();
        RefreshAvoidance();
    }

    void RefreshChunks()
    {
        var grid = GridEx.GetCurrentGrid();

        if (grid == null)
            return;

        int size = grid.Size();

        if (m_chunks == null || m_chunks.width != size)
        { 
            m_chunks = new Matrix<List<GameEntity>>(size, size);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    m_chunks.Set(i, j, new List<GameEntity>());
            }
        }
        else
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    m_chunks.Get(i, j).Clear();
            }
        }

        foreach(var e in m_entities)
        {
            var chunkIndex = Grid.PosToChunkIndex(e.transform.position);

            m_chunks.Get(chunkIndex.x, chunkIndex.z).Add(e);
        }
    }

    void RefreshAvoidance()
    {
        var grid = GridEx.GetCurrentGrid();

        if (grid == null)
            return;

        int size = GridEx.GetRealSize(grid) * m_avoidanceResolution;

        var oldAvoidance = m_avoidanceMap;

        m_avoidanceMap = new Matrix<GameEntity>(size, size);
        m_avoidanceMap.SetAll(null);

        foreach (var e in m_entities)
        {
            var pos = e.transform.position;
            GetAvoidanceOffsets(e, m_tempVector);
            foreach(var p in m_tempVector)
            {
                var point = GetAvoidancePoint(p + pos);
                if (grid.LoopX())
                    point.x = GridEx.LoopPos(point.x, size);
                if (grid.LoopZ())
                    point.y = GridEx.LoopPos(point.y, size);

                if (point.x < 0 || point.y < 0 || point.x >= size || point.y > size)
                    continue;

                var oldEntity = oldAvoidance != null ? oldAvoidance.Get(point.x, point.y) : null;
                if (oldEntity == e || m_avoidanceMap.Get(point.x, point.y) == null)
                    m_avoidanceMap.Set(point.x, point.y, e);
            }
        }
    }

    public GameEntity GetFilledAvoidance(Vector3 pos)
    {
        var grid = GridEx.GetCurrentGrid();

        if (grid == null)
            return null;

        int size = GridEx.GetRealSize(grid) * m_avoidanceResolution;
        if (m_avoidanceMap == null || m_avoidanceMap.width != size)
            return null;

        Vector2Int point = GetAvoidancePoint(pos);

        if (grid.LoopX())
            point.x = GridEx.LoopPos(point.x, size);
        if (grid.LoopZ())
            point.y = GridEx.LoopPos(point.y, size);

        if (point.x < 0 || point.y < 0 || point.x >= size || point.y > size)
            return null;

        return m_avoidanceMap.Get(point.x, point.y);
    }

    public void Register(GameEntity entity)
    {
        m_entities.Add(entity);
    }

    public void UnRegister(GameEntity entity)
    {
        m_entities.Remove(entity);
    }

    public int GetEntityNb()
    {
        return m_entities.Count();
    }

    public GameEntity GetEntityFromIndex(int index)
    {
        if (index < 0 || index >= m_entities.Count)
            return null;
        return m_entities[index];
    }

    public GameEntity GetNearestEntity(Vector3 pos, float maxDistance, Team team, AliveType alive = AliveType.NotSet)
    {
        if (m_chunks == null)
            return null;

        Grid grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return null;

        Vector3 minPos = new Vector3(pos.x - maxDistance, pos.y, pos.z - maxDistance);
        Vector3 maxPos = new Vector3(pos.x + maxDistance, pos.y, pos.z + maxDistance);

        Vector3Int minChunk = Grid.PosToChunkIndex(minPos);
        Vector3Int maxChunk = Grid.PosToChunkIndex(maxPos);

        if (!grid.LoopX())
        {
            if (minChunk.x < 0)
                minChunk.x = 0;
            if (maxChunk.x >= m_chunks.width)
                maxChunk.x = m_chunks.width - 1;
        }
        if(!grid.LoopZ())
        {
            if (minChunk.z < 0)
                minChunk.z = 0;
            if (maxChunk.z >= m_chunks.depth)
                maxChunk.z = m_chunks.depth - 1;
        }

        float bestDist = maxDistance * maxDistance;
        GameEntity bestEntity = null;

        for (int i = minChunk.x; i <= maxChunk.x; i++)
        {
            for (int j = minChunk.z; j <= maxChunk.z; j++)
            {
                var chunkPos = GridEx.GetPosFromLoop(grid, new Vector2Int(i, j));

                var list = m_chunks.Get(chunkPos.x, chunkPos.y);
                if (list == null)
                    continue;

                foreach (var e in list)
                {
                    if (e == null)
                        continue;

                    float dist = GridEx.GetDistance(grid, e.transform.position, pos);
                    if (dist >= bestDist)
                        continue;

                    if (e.GetTeam() != team)
                        continue;

                    if (!Utility.IsAliveFilter(e.gameObject, alive))
                        continue;

                    bestDist = dist;
                    bestEntity = e;
                }
            }
        }

        return bestEntity;
    }

    public void Clear()
    {
        //destroying elements can change the list
        var elements = m_entities.ToList();
        m_entities.Clear();

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
                GameEntity.Create(jsonElement.JsonObject());
            }
        }
    }

    public JsonObject Save()
    {
        JsonObject obj = new JsonObject();

        var jsonArray = new JsonArray();
        obj.AddElement("data", jsonArray);

        foreach (var b in m_entities)
        {
            jsonArray.Add(b.Save());
        }

        return obj;
    }

    void GetAvoidanceOffsets(GameEntity entity, List<Vector3> outVect)
    {
        outVect.Clear();

        int avoidanceDiameter = entity.GetAvoidanceDiameter();
        if (avoidanceDiameter == 1)
        {
            outVect.Add(Vector3.zero);
            return;
        }
        float avoidanceSize = 1.0f / m_avoidanceResolution;
        if(avoidanceDiameter == 2)
        {
            float d = avoidanceSize / 2;
            outVect.Add(new Vector3(-d, 0, -d));
            outVect.Add(new Vector3(-d, 0, d));
            outVect.Add(new Vector3(d, 0, -d));
            outVect.Add(new Vector3(d, 0, d));
            return;
        }

        float origin = avoidanceSize * 0.5f * avoidanceDiameter;
        float maxDist = (avoidanceDiameter + 1.0f) * avoidanceSize / 2.0f;
        maxDist *= maxDist;

        for (int i = 0; i < avoidanceDiameter; i++)
        {
            for(int j = 0; j < avoidanceDiameter; j++)
            {
                Vector3 pos = new Vector3(-origin + i * avoidanceSize, 0, -origin + j * avoidanceSize);
                if (pos.sqrMagnitude > maxDist)
                    continue;
                outVect.Add(pos);
            }
        }
    }

    Vector2Int GetAvoidancePoint(Vector3 pos)
    {
        pos *= m_avoidanceResolution;
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
    }
}
