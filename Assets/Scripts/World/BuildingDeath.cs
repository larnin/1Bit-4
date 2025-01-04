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
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (m_offset < 0)
            return;

        if (m_building == null)
            return;

        m_speed += Global.instance.buildingDatas.DestructionAcceleration * Time.deltaTime;
        if (m_speed > Global.instance.buildingDatas.DestructionSpeed)
            m_speed = Global.instance.buildingDatas.DestructionSpeed;

        float deltaMove = m_speed * Time.deltaTime;
        m_offset += deltaMove;
        Vector3 offset = new Vector3(0, -deltaMove, 0);

        foreach(var r in m_renderables)
        {
            var pos = r.transform.position + offset;
            r.transform.position = pos;
        }

        var size = m_building.GetSize().y;

        if (m_offset > size + 2)
            Destroy(gameObject);
    }

    void InstantiateParticles()
    {
        if (m_building == null)
            return;

        var size = m_building.GetSize();

        var data = Global.instance.buildingDatas.GetDestructedBuildingDatas(new Vector2Int(size.x, size.z));
        if (data == null)
            return;

        var obj = Instantiate(data.particlePrefab);

        obj.transform.position = transform.position;

        var comp = obj.GetComponent<BuildingDeathParticles>();

        if (comp != null)
        {
            float durationToMaxSpeed = Global.instance.buildingDatas.DestructionSpeed / Global.instance.buildingDatas.DestructionAcceleration;
            float distAtMaxSpeed = Global.instance.buildingDatas.DestructionAcceleration * durationToMaxSpeed * durationToMaxSpeed / 2;

            float duration = 0;
            if (size.y < distAtMaxSpeed)
                duration = Mathf.Sqrt(2 * size.y / Global.instance.buildingDatas.DestructionAcceleration);
            else
            {
                duration = Mathf.Sqrt(2 * distAtMaxSpeed / Global.instance.buildingDatas.DestructionAcceleration);
                duration += (size.y - distAtMaxSpeed) / Global.instance.buildingDatas.DestructionSpeed;
            }
            comp.SetDuration(duration);
        }
    }

    void InstantiateDestroyedBuilding()
    {
        if (m_building == null)
            return;

        var size = m_building.GetSize();

        var buildingData = Global.instance.buildingDatas.GetBuilding(BuildingType.DestroyedBuilding);
        if (buildingData == null || buildingData.prefab == null)
            return;

        var obj = Instantiate(buildingData.prefab);
        obj.transform.parent = BuildingList.instance.transform;
        obj.transform.position = transform.position;

        var b = obj.GetComponent<BuildingDestroyed>();
        if (b != null)
            b.SetSize(new Vector2Int(size.x, size.z));
    }
}
