using System;
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
}
