using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingDeath : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    List<Renderer> m_renderables = new List<Renderer>();
    BuildingBase m_building;
    float m_offset = -1;
    float m_speed = 0;

    GameObject m_rubblesObject;
    RubblesInstance m_rubbleInstance;

    GameObject m_particlesObject;
    ParticleSystem m_particleSystem;
    bool m_stoppedParticles = false;

    private void Awake()
    {
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnDeath(DeathEvent e)
    {
        m_renderables = GetComponentsInChildren<Renderer>().ToList();
        m_offset = 0;
        m_building = GetComponent<BuildingBase>();

        InstantiateParticles();
        InstantiateDestroyedBuilding();

        if (ConnexionSystem.instance != null)
            ConnexionSystem.instance.OnBuildingChange();

        var cols = GetComponentsInChildren<Collider>();
        foreach (var col in cols)
            col.enabled = false;
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (m_offset < 0)
            return;

        if (m_building == null)
            return;

        var size = m_building.GetSize().y;

        if (m_offset > size && !m_stoppedParticles)
            m_particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (m_offset < size + 2)
        {
            m_speed += Global.instance.buildingDatas.destructionDatas.DestructionAcceleration * Time.deltaTime;
            if (m_speed > Global.instance.buildingDatas.destructionDatas.DestructionSpeed)
                m_speed = Global.instance.buildingDatas.destructionDatas.DestructionSpeed;

            float deltaMove = m_speed * Time.deltaTime;
            m_offset += deltaMove;
            Vector3 offset = new Vector3(0, -deltaMove, 0);

            foreach (var r in m_renderables)
            {
                if (r == null)
                    continue;

                var pos = r.transform.position + offset;
                r.transform.position = pos;
            }
        }
        else if (m_rubbleInstance.HaveEnded())
        {
            Destroy(gameObject);
            if (m_particlesObject != null)
                Destroy(m_particlesObject, 5);
        }
    }

    void InstantiateParticles()
    {
        if (m_building == null)
            return;

        var size = m_building.GetSize();

        var data = Global.instance.buildingDatas.GetDestructedBuildingDatas(new Vector2Int(size.x, size.z));
        if (data == null)
            return;

        m_particlesObject = Instantiate(data.particlePrefab);
        m_particlesObject.transform.position = transform.position;
        m_particlesObject.transform.rotation = transform.rotation;
        m_particleSystem = m_particlesObject.GetComponentInChildren<ParticleSystem>();
        if(m_particleSystem != null)
            m_particleSystem.Play();
    }

    void InstantiateDestroyedBuilding()
    {
        if (m_building == null)
            return;

        var size = m_building.GetSize();

        m_rubblesObject = new GameObject();
        m_rubblesObject.transform.parent = transform;
        m_rubblesObject.transform.localPosition = Vector3.zero;
        m_rubblesObject.transform.localRotation = Quaternion.identity;
        m_rubbleInstance = m_rubblesObject.AddComponent<RubblesInstance>();
        m_rubbleInstance.SetSize(new Vector2Int(size.x, size.z));
    }
}
