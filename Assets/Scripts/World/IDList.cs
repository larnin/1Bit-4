using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IDList : MonoBehaviour
{
    List<EntityID> m_entities = new List<EntityID>();

    static IDList m_instance = null;
    public static IDList instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void Register(EntityID entity)
    {
        m_entities.Add(entity);
    }

    public void UnRegister(EntityID entity)
    {
        m_entities.Remove(entity);
    }

    public int GetEntityNb()
    {
        return m_entities.Count();
    }

    public EntityID GetEntityFromIndex(int index)
    {
        if (index < 0 || index >= m_entities.Count)
            return null;
        return m_entities[index];
    }

    public EntityID GetEntityFromID(Guid id)
    {
        foreach(var e in m_entities)
        {
            if (e.GetID() == id)
                return e;
        }

        return null;
    }
}
