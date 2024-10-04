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
        if (ConnexionSystem.instance == null)
            return m_powerGeneration;
        if (!ConnexionSystem.instance.IsConnected(this))
            return 0;

        float maxDistance = m_generationRadius * m_generationRadius;
        var pos = GetGroundCenter();
        var connected = ConnexionSystem.instance.ConnectedBuilding(this);
        foreach(var c in connected)
        {
            var otherPos = c.GetGroundCenter();
            float dist = (pos - otherPos).sqrMagnitude;

            if (dist > maxDistance)
                continue;
            maxDistance = dist;
        }

        float powerMultiplier = maxDistance / m_generationRadius / m_generationRadius;

        return m_powerGeneration * powerMultiplier;
    }

    public override float PlacementRadius()
    {
        return m_placementRadius;
    }
}
