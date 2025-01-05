using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTurretFlameThrower : BuildingTurretBase
{
    [SerializeField] ResourceType m_resourceConsumption = ResourceType.Oil;
    [SerializeField] float m_consumption = 1;
    [SerializeField] float m_damages = 1;
    [SerializeField] StatusType m_status = StatusType.Burning;
    [SerializeField] float m_statusPower = 1;
    [SerializeField] float m_coneAngle = 20;
    [SerializeField] float m_coneRange = 10;
    [SerializeField] float m_coneProgressionSpeed = 5;
    [SerializeField] float m_coneSphereCount = 4;
    [SerializeField] bool m_debugDisplaySpheres;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Turret2;
    }

    protected override bool CanFire()
    {
        if (ResourceSystem.instance == null)
            return false;

        if (!ResourceSystem.instance.HaveResource(m_resourceConsumption))
            return false;

        float resourcesFrame = m_consumption * Time.deltaTime;
        return ResourceSystem.instance.GetResourceStored(m_resourceConsumption) >= resourcesFrame;
    }

    protected override bool IsContinuousWeapon()
    {
        return true;
    }

    protected override void StartFire()
    {
        
    }

    protected override void EndFire()
    {
        
    }
}

