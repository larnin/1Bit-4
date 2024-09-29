using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTower : BuildingBase
{
    [SerializeField] float m_powerGeneration = 10;
    [SerializeField] float m_placementRadius = 5;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Tower;
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
