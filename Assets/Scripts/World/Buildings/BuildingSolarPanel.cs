using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingSolarPanel : BuildingBase
{
    [SerializeField] float m_powerGeneration = 1;
    [SerializeField] bool m_bigPanel = false;

    public override BuildingType GetBuildingType()
    {
        if (m_bigPanel)
            return BuildingType.BigSolarPanel;
        return BuildingType.SolarPanel;
    }

    public override float EnergyGeneration()
    {
        return m_powerGeneration;
    }
}

