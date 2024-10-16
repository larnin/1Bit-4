using UnityEngine;
using System.Collections;

public class DisplayLife : MonoBehaviour
{
    const string lifeParam = "FillPercent";

    [SerializeField] GameObject m_lifebarPrefab;
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
        fLife = Random.value;
        if(fLife >= 1)
        {
            if (m_lifebarInstance != null)
                Destroy(m_lifebarInstance);
        }
        else if(m_lifebarInstance == null)
        {
            m_lifebarInstance = Instantiate(m_lifebarPrefab);
            m_lifebarInstance.transform.parent = transform;
            m_lifebarInstance.transform.localPosition = new Vector3(0, m_barHeight, 0);
            m_barRenderer = m_lifebarInstance.GetComponentInChildren<Renderer>();
        }

        if(m_lifebarInstance != null)
        {
            GetCameraEvent cam = new GetCameraEvent();
            Event<GetCameraEvent>.Broadcast(cam);
            if (cam.camera != null)
                m_lifebarInstance.transform.LookAt(cam.camera.transform);

            var mats = m_barRenderer.materials;
            foreach(var mat in mats)
                mat.SetFloat(lifeParam, fLife);
            m_barRenderer.materials = mats;
        }
    }
}
