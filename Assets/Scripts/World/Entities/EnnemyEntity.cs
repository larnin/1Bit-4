using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnnemyEntity : MonoBehaviour
{
    bool m_added = false;

    public virtual void OnEnable()
    {
        Add();
    }

    public virtual void OnDisable()
    {
        Remove();
    }

    private void OnDestroy()
    {
        Remove();
    }

    public virtual void Update()
    {
        if (!m_added)
            Add();
    }

    void Add()
    {
        var manager = EntityList.instance;
        if (manager != null)
        {
            m_added = true;
            manager.Register(this);
        }
    }

    void Remove()
    {
        if (!m_added)
            return;

        var manager = EntityList.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
    }
}
