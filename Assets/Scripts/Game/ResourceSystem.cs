using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceSystem : MonoBehaviour
{
    class ResourceInfo
    {
        public ResourceType type;
        public float stored;
        public float storageMax;
        public float production;
        public float consumption;
    }

    List<ResourceInfo> m_resources = new List<ResourceInfo>();

    private void Update()
    {
        UpdateEnergy();
    }

    private void LateUpdate()
    {
        foreach(var r in m_resources)
        {
            r.production = 0;
            r.consumption = 0;
        }
    }

    void UpdateEnergy()
    {
        ResourceInfo energy = null;
        foreach(var r in m_resources)
        {
            if(r.type == ResourceType.Energy)
            {
                energy = r;
                break;
            }
        }
        if(energy == null)
        {
            energy = new ResourceInfo();
            energy.type = ResourceType.Energy;
            m_resources.Add(energy);
        }

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
}
