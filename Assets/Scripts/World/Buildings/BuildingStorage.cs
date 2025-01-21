using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingStorage : BuildingBase
{
    [SerializeField] float m_energyUptake;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Storage;
    }

    public override float EnergyUptakeWanted()
    {
        return m_energyUptake;
    }
}
