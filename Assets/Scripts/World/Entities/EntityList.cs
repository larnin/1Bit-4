using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityList : MonoBehaviour
{
    List<GameEntity> m_entities = new List<GameEntity>();

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

    public GameEntity GetNearestEntity(Vector3 pos, AliveType alive = AliveType.NotSet)
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

    public GameEntity GetNearestEntity(Vector3 pos, Team team, AliveType alive = AliveType.NotSet)
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
