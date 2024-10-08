﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityList : MonoBehaviour
{
    List<EnnemyEntity> m_entities = new List<EnnemyEntity>();

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

    public void Register(EnnemyEntity entity)
    {
        m_entities.Add(entity);
    }

    public void UnRegister(EnnemyEntity entity)
    {
        m_entities.Remove(entity);
    }

    public int GetEntityNb()
    {
        return m_entities.Count();
    }

    public EnnemyEntity GetEntityFromIndex(int index)
    {
        if (index < 0 || index >= m_entities.Count)
            return null;
        return m_entities[index];
    }

    public EnnemyEntity GetNearestEntity(Vector3 pos)
    {
        float bestDist = 0;
        EnnemyEntity bestEntity = null;

        foreach(var e in m_entities)
        {
            float dist = (pos - e.transform.position).sqrMagnitude;

            if(dist < bestDist || bestEntity == null)
            {
                bestDist = dist;
                bestEntity = e;
            }
        }

        return bestEntity;
    }
}
