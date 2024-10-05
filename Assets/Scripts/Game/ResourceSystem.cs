using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceSystem : MonoBehaviour
{
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

        List<OneResourceHistory> m_history = new List<OneResourceHistory>();
        public void UpdateHistory()
        {
            var newPoint = new OneResourceHistory();
            newPoint.count = production - consumption;
            newPoint.time = Time.time;

            m_history.Insert(0, newPoint);

            for(int i = 0; i < m_history.Count; i++)
            {
                if(m_history[i].time < Time.time - 1)
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

    private void Update()
    {
        UpdateEnergy();
    }

    private void LateUpdate()
    {
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

        for(int i = 0; i < nbBuilding; i++)
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
            consumptionPercent = consumptionWanted / energy.production;
            storagePercent = 0;
        }
        else if (energy.production < consumptionWanted + storageWanted)
            storagePercent = (energy.production - consumptionWanted) / storageWanted;
        if (consumptionPercent < 1)
        {
            float deltaConsumption = (1 - consumptionPercent) * consumptionWanted;
            deltaConsumption *= Time.deltaTime;

            if (energy.stored < deltaConsumption)
            {
                float realPercent = deltaConsumption / energy.stored;
                consumptionPercent += (1 - consumptionPercent) * realPercent;
                deltaConsumption = energy.stored;
            }
            else consumptionPercent = 1;

            for (int i = 0; i < nbBuilding; i++)
            {
                var b = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);

                float stored = b.EnergyStorageValue();
                float percentUse = stored / energy.stored;
                b.ConsumeStoredEnergy(percentUse * deltaConsumption);
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

        return resource;
    }

    public void AddResource(ResourceType type, float count)
    {
        if (type == ResourceType.Energy)
            return;

        var resource = GetResourceOrCreate(type);
        resource.production += count;
        resource.stored += count;
    }

    public void RemoveResource(ResourceType type, float count)
    {
        if (type == ResourceType.Energy)
            return;

        var resource = GetResourceOrCreate(type);
        if (resource.stored < count)
            count = resource.stored;
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
