using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingCrystalMine : BuildingBase
{
    [SerializeField] float m_energyConsumption = 1;
    [SerializeField] ResourceType m_generatedResource;
    [SerializeField] float m_generation = 1;
    [SerializeField] int m_mineRadius = 1;

    float m_energyEfficiency = 1;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.CrystalMine;
    }

    public override float EnergyUptakeWanted() 
    { 
        return m_energyConsumption; 
    }

    public override void EnergyUptake(float value) 
    {
        m_energyEfficiency = value / m_energyConsumption;
        if (m_energyEfficiency > 1)
            m_energyEfficiency = 1;

        m_energyEfficiency *= m_energyEfficiency;
    }

    protected override void Update()
    {
        base.Update();
        
        //todo collect resources
    }

    public override bool CanBePlaced(Vector3Int pos) 
    { 
        //todo check resources around
        return true; 
    }
}

