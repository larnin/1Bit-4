using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CustomLight : MonoBehaviour
{
    [SerializeField] float m_radius;

    bool m_added = false;

    private void OnEnable()
    {
        Add();
    }

    private void OnDisable()
    {
        Remove();
    }

    private void OnDestroy()
    {
        Remove();
    }

    private void Update()
    {
        if (!m_added)
            Add();
    }

    void Add()
    {
        var manager = CustomLightsManager.instance;
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

        var manager = CustomLightsManager.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
    }

    public void SetRadius(float radius)
    {
        m_radius = radius;
    }

    public float GetRadius()
    {
        return m_radius;
    }
}