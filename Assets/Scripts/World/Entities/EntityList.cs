using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityList : MonoBehaviour
{
    List<GameEntity> m_entities = new List<GameEntity>();
    Matrix<List<GameEntity>> m_chunks;

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
    }

    void RefreshChunks()
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent()).grid;

        if(grid == null)
            return;

        int size = grid.Size();

        m_chunks = new Matrix<List<GameEntity>>(size, size);
        for(int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
                m_chunks.Set(i, j, new List<GameEntity>());
        }

        foreach(var e in m_entities)
        {
            var chunkIndex = Grid.PosToChunkIndex(e.transform.position);

            m_chunks.Get(chunkIndex.x, chunkIndex.z).Add(e);
        }
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

    GameEntity GetNearestEntity(Vector3 pos, AliveType alive = AliveType.NotSet)
    {
        float bestDist = 0;
        GameEntity bestEntity = null;

        foreach(var e in m_entities)
        {
            if (!Utility.IsAliveFilter(e.gameObject, alive))
                continue;

            float dist = (pos - e.transform.position).sqrMagnitude;

            if(dist < bestDist || bestEntity == null)
            {
                bestDist = dist;
                bestEntity = e;
            }
        }

        return bestEntity;
    }

    GameEntity GetNearestEntity(Vector3 pos, Team team, AliveType alive = AliveType.NotSet)
    {
        float bestDist = 0;
        GameEntity bestEntity = null;

        foreach (var e in m_entities)
        {
            if (e.GetTeam() != team)
                continue;

            if (!Utility.IsAliveFilter(e.gameObject, alive))
                continue;

            float dist = (pos - e.transform.position).sqrMagnitude;

            if (dist < bestDist || bestEntity == null)
            {
                bestDist = dist;
                bestEntity = e;
            }
        }

        return bestEntity;
    }

    public GameEntity GetNearestEntity(Vector3 pos, float maxDistance, Team team, AliveType alive = AliveType.NotSet)
    {
        if (m_chunks == null)
            return null;

        Vector3 minPos = new Vector3(pos.x - maxDistance, pos.y, pos.z - maxDistance);
        Vector3 maxPos = new Vector3(pos.x + maxDistance, pos.y, pos.z + maxDistance);

        Vector3Int minChunk = Grid.PosToChunkIndex(minPos);
        Vector3Int maxChunk = Grid.PosToChunkIndex(maxPos);

        if (minChunk.x < 0)
            minChunk.x = 0;
        if (minChunk.z < 0)
            minChunk.z = 0;
        if (maxChunk.x >= m_chunks.width)
            maxChunk.x = m_chunks.width - 1;
        if (maxChunk.z >= m_chunks.depth)
            maxChunk.z = m_chunks.depth - 1;

        float bestDist = maxDistance * maxDistance;
        GameEntity bestEntity = null;

        for(int i = minChunk.x; i <= maxChunk.x; i++)
        {
            for(int j = minChunk.z; j <= maxChunk.z; j++)
            {
                var list = m_chunks.Get(i, j);
                if (list == null)
                    continue;

                foreach(var e in list)
                {
                    if (e == null)
                        continue;
                    float dist = (e.transform.position - pos).sqrMagnitude;
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
}
