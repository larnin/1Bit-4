﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceSystem : MonoBehaviour
{
    [Serializable]
    class InitialResource
    {
        public ResourceType type;
        public float count;
    }

    class OneResourceHistory
    {
        public float count;
        public float time;
    }

    class ResourceInfo
    {
        public ResourceType type;
        public float stored;
        public float storageMax;
        public float production;
        public float consumption;

        float time;

        List<OneResourceHistory> m_history = new List<OneResourceHistory>();

        public void UpdateHistory()
        {
            time += Time.deltaTime;

            var newPoint = new OneResourceHistory();
            newPoint.count = production - consumption;
            newPoint.time = time;

            m_history.Insert(0, newPoint);

            for(int i = 0; i < m_history.Count; i++)
            {
                if(m_history[i].time < time - 1)
                {
                    m_history.RemoveAt(i);
                    i--;
                }
            }
        }

        public float GetHistory()
        {
            float value = 0;
            foreach (var h in m_history)
                value += h.count;
            return value;
        }
    }
    
    [SerializeField] List<InitialResource> m_initialResources;

    List<ResourceInfo> m_resources = new List<ResourceInfo>();

    static ResourceSystem m_instance = null;
    public static ResourceSystem instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    private void Start()
    {
        foreach(var r in m_initialResources)
        {
            var resource = GetResourceOrCreate(r.type);
            resource.stored += r.count;
        }
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        UpdateEnergy();
    }

    private void LateUpdate()
    {
        if (GameInfos.instance.paused)
            return;

        UpdateStorage();

        foreach(var r in m_resources)
        {
            r.UpdateHistory();
            if (r.type == ResourceType.Energy)
                continue;
            r.production = 0;
            r.consumption = 0;
        }
    }

    void UpdateEnergy()
    {
        ResourceInfo energy = GetResourceOrCreate(ResourceType.Energy);
        energy.production = 0;
        energy.consumption = 0;
        energy.storageMax = 0;
        energy.stored = 0;

        if (ConnexionSystem.instance == null)
            return;

        int nbBuilding = ConnexionSystem.instance.GetConnectedBuildingNb();
        for (int i = 0; i < nbBuilding; i++)
        {
            var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);
            energy.production += b.EnergyGeneration();
            energy.storageMax += b.EnergyStorageMax();
            energy.stored += b.EnergyStorageValue();
        }

        float consumptionWanted = 0;
        float storageWanted = 0;

        for (int i = 0; i < nbBuilding; i++)
        {
            var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);
            if (b.EnergyPriority() == EnergyUptakePriority.consumption)
                consumptionWanted += b.EnergyUptakeWanted();
            else storageWanted += b.EnergyUptakeWanted();
        }

        float consumptionPercent = 1;
        float storagePercent = 1;
        if (energy.production < consumptionWanted)
        {
            consumptionPercent = energy.production / consumptionWanted;
            storagePercent = 0;
        }
        else if (energy.production < consumptionWanted + storageWanted)
            storagePercent = (energy.production - consumptionWanted) / storageWanted;
        if(consumptionPercent < 1 && energy.stored > 0)
        {
            float deltaConsumption = (consumptionWanted - energy.production) * Time.deltaTime;

            float toRemovePercent = 1;
            if (energy.stored < deltaConsumption)
            {
                float newConsuption = energy.stored / Time.deltaTime + energy.production;
                consumptionPercent = newConsuption / consumptionWanted;
            }
            else
            {
                consumptionPercent = 1;
                toRemovePercent = deltaConsumption / energy.stored;
            }

            for (int i = 0; i < nbBuilding; i++)
            {
                var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);

                float stored = b.EnergyStorageValue();
                if (stored == 0)
                    continue;

                b.ConsumeStoredEnergy(stored * toRemovePercent);
            }
        }

        for (int i = 0; i < nbBuilding; i++)
        {
            var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);
            float value = b.EnergyUptakeWanted();
            if (b.EnergyPriority() == EnergyUptakePriority.consumption)
                value *= consumptionPercent;
            else value *= storagePercent;
            b.EnergyUptake(value);
        }

        energy.consumption = consumptionWanted + storageWanted * storagePercent;
    }

    void UpdateStorage()
    {
        if (BuildingList.instance == null || ConnexionSystem.instance == null)
            return;

        var buildings = BuildingList.instance.GetAllBuilding(BuildingType.Storage, Team.Player);
        int nbStorage = 0;
        foreach(var b in buildings)
        {
            if (ConnexionSystem.instance.IsConnected(b))
                nbStorage++;
        }

        foreach(var r in m_resources)
        {
            if (r.type == ResourceType.Energy)
                continue;

            var data = Global.instance.resourceDatas.GetResource(r.type);
            if (data == null)
                continue;

            r.storageMax = data.storageBase + data.storageIncrease * nbStorage;

            if (r.stored > r.storageMax && r.storageMax > 0)
                r.stored = r.storageMax;
        }
    }

    ResourceInfo GetResourceOrCreate(ResourceType type)
    {
        foreach (var r in m_resources)
        {
            if (r.type == type)
                return r;
        }

        ResourceInfo resource = new ResourceInfo();
        resource.type = type;
        m_resources.Add(resource);

        UpdateStorage();

        return resource;
    }

    public void AddResource(ResourceType type, float count)
    {
        if (type == ResourceType.Energy)
            return;

        var resource = GetResourceOrCreate(type);

        float newCount = resource.stored + count;

        if (newCount > resource.storageMax && resource.storageMax > 0)
            newCount = resource.storageMax;
        resource.production += newCount - resource.stored;
        resource.stored = newCount;
    }

    public void RemoveResource(ResourceType type, float count, bool keepTrack = true)
    {
        if (type == ResourceType.Energy)
            return;

        var resource = GetResourceOrCreate(type);
        if (resource.stored < count)
            count = resource.stored;
        if(keepTrack)
            resource.consumption += count;
        resource.stored -= count;
    }

    public float GetResourceStored(ResourceType type)
    {
        var resource = GetResourceOrCreate(type);
        return resource.stored;
    }

    public float GetResourceStorageMax(ResourceType type)
    {
        var resource = GetResourceOrCreate(type);
        return resource.storageMax;
    }

    public float GetLastSecondResourceMean(ResourceType type)
    {
        var resource = GetResourceOrCreate(type);
        if (type == ResourceType.Energy)
            return resource.production - resource.consumption;

        return resource.GetHistory();
    }

    public bool HaveResource(ResourceType type)
    {
        foreach(var r in m_resources)
        {
            if (r.type == type)
                return true;
        }

        return false;
    }
}
