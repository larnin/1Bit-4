using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingPylon : BuildingBase
{
    [SerializeField] float m_powerGeneration = 1;
    [SerializeField] float m_generationRadius = 4;
    [SerializeField] float m_placementRadius = 5;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Pylon;
    }

    public override float EnergyGeneration()
    {
        return m_powerGeneration;
    }

    public override float PlacementRadius()
    {
        return m_placementRadius;
    }
}
