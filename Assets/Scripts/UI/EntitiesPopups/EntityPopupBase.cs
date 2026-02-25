using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class EntityPopupBase : MonoBehaviour
{
    bool m_isAwake = false;

    protected GameObject m_entity;

    public void SetEntity(GameObject entity)
    {
        m_entity = entity;
        if(m_isAwake)
            OnInit();
    }

    public Rect GetBounds()
    {
        var transform = GetComponent<RectTransform>();
        if(transform == null)
            return new Rect(Vector2.zero, Vector2.zero);

        return transform.rect;
    }

    protected virtual void Awake()
    {
        m_isAwake = true;
        if (m_entity != null)
            OnInit();
    }

    protected abstract void OnInit();
}
