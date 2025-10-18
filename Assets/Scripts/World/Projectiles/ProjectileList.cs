using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class ProjectileList : MonoBehaviour
{
    List<ProjectileBase> m_projectiles = new List<ProjectileBase>();

    static ProjectileList m_instance = null;
    public static ProjectileList instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void Register(ProjectileBase projectile)
    {
        m_projectiles.Add(projectile);
    }

    public void UnRegister(ProjectileBase projectile)
    {
        m_projectiles.Remove(projectile);
    }

    public int GetProjectileNb()
    {
        return m_projectiles.Count();
    }

    public ProjectileBase GetProjectileFromIndex(int index)
    {
        if (index < 0 || index >= m_projectiles.Count)
            return null;
        return m_projectiles[index];
    }

    public void Clear()
    {
        //destroying elements can change the list
        var elements = m_projectiles.ToList();
        m_projectiles.Clear();

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
                //todo
                //GameEntity.Create(jsonElement.JsonObject());
            }
        }
    }

    public JsonObject Save()
    {
        JsonObject obj = new JsonObject();

        var jsonArray = new JsonArray();
        obj.AddElement("data", jsonArray);

        foreach (var b in m_projectiles)
        {
            jsonArray.Add(b.Save());
        }

        return obj;
    }
}