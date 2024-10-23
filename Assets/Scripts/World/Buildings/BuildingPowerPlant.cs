using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingPowerPlant : BuildingBase
{
    [SerializeField] float m_powerGeneration = 1;
    [SerializeField] List<OneResourceCost> m_resourceConsumption;

    float m_efficiency = 1;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.PowerPlant;
    }

    public override float EnergyGeneration()
    {
        return m_powerGeneration * m_efficiency;
    }

    protected override void Update()
    {
        m_efficiency = 0;

        if (ConnexionSystem.instance != null && !ConnexionSystem.instance.IsConnected(this))
            return;

        if (ResourceSystem.instance == null)
            return;

        m_efficiency = 1;

        foreach(var r in m_resourceConsumption)
        {
            if(!ResourceSystem.instance.HaveResource(r.type))
            {
                m_efficiency = 0;
                break;
            }

            float stored = ResourceSystem.instance.GetResourceStored(r.type);
            float wanted = r.count * Time.deltaTime;
            if(wanted > stored)
            {
                float percent = stored / wanted;
                if (percent < m_efficiency)
                    m_efficiency = percent;
            }
        }

        foreach(var r in m_resourceConsumption)
        {
            float wanted = r.count * Time.deltaTime * m_efficiency;
            ResourceSystem.instance.RemoveResource(r.type, wanted);
        }
    }
}

