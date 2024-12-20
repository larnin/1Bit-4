﻿using UnityEngine;
using System.Collections;

public class DisplayLife : MonoBehaviour
{
    const string lifeParam = "_FillPercent";

    [SerializeField] float m_barHeight = 1;
    [SerializeField] float m_barScale = 1;

    LifeComponent m_lifeComponent;
    GameObject m_lifebarInstance;
    Renderer m_barRenderer;

    void Start()
    {
        m_lifeComponent = GetComponent<LifeComponent>();
    }
    
    void Update()
    {
        if(m_lifeComponent == null)
        {
            if (m_lifebarInstance != null)
                Destroy(m_lifebarInstance);
            return;
        }

        float fLife = m_lifeComponent.GetLifePercent();
        if(fLife >= 1)
        {
            if (m_lifebarInstance != null)
                Destroy(m_lifebarInstance);
        }
        else if(m_lifebarInstance == null)
        {
            Vector3 pos = new Vector3(0, m_barHeight, 0);

            var type = GameSystem.GetEntityType(gameObject);
            if(type == EntityType.Building)
            {
                var building = GetComponent<BuildingBase>();
                if(building != null)
                {
                    var point = building.GetGroundCenter();
                    pos += point - transform.position;
                }
            }

            m_lifebarInstance = Instantiate(Global.instance.buildingDatas.LifebarPrefab);
            m_lifebarInstance.transform.parent = transform;
            m_lifebarInstance.transform.localPosition = pos;
            m_lifebarInstance.transform.localScale = new Vector3(m_barScale, m_barScale, m_barScale);
            m_barRenderer = m_lifebarInstance.GetComponentInChildren<Renderer>();
        }

        if(m_lifebarInstance != null)
        {
            GetCameraEvent cam = new GetCameraEvent();
            Event<GetCameraEvent>.Broadcast(cam);
            if (cam.camera != null)
            {
                var rot = Quaternion.LookRotation(-cam.camera.transform.forward).eulerAngles;

                m_lifebarInstance.transform.rotation = Quaternion.Euler(0, rot.y, 0);
            }

            var mats = m_barRenderer.materials;
            foreach(var mat in mats)
                mat.SetFloat(lifeParam, fLife);
            m_barRenderer.materials = mats;
        }
    }
}
