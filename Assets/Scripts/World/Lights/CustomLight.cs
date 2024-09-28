using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CustomLight : MonoBehaviour
{
    [SerializeField] float m_radius;

    bool added = false;

    private void OnEnable()
    {
        Add();
    }

    private void OnDisable()
    {
        var manager = CustomLightsManager.instance;
        if (manager != null)
            manager.UnRegister(this);

        added = false;
    }

    private void Update()
    {
        if (!added)
            Add();
    }

    void Add()
    {
        var manager = CustomLightsManager.instance;
        if (manager != null)
        {
            added = true;
            manager.Register(this);
        }
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