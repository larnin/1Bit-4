using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTurretFreezer : BuildingTurretBase
{
    [SerializeField] ResourceType m_resourceConsumption = ResourceType.Oil;
    [SerializeField] float m_consumption = 1;
    [SerializeField] ProjectileChoice m_projectileType;
    [SerializeField] string m_fireSound;
    [SerializeField] float m_fireSoundVolume = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_subscriberList.Unsubscribe();
    }

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Turret3;
    }

    protected override bool CanFire()
    {
        if (ResourceSystem.instance == null)
            return false;

        if (!ResourceSystem.instance.HaveResource(m_resourceConsumption))
            return false;

        if (ResourceSystem.instance.GetResourceStored(m_resourceConsumption) < m_consumption)
            return false;

        var firePoint = GetCurrentFirepoint();
        if (firePoint == null)
            return false;

        if (!IsTargetVisible(firePoint.position))
            return false;

        return true;
    }

    protected override void Fire()
    {
        if (!CanFire())
            return;

        if (ResourceSystem.instance == null)
            return;

        ResourceSystem.instance.RemoveResource(m_resourceConsumption, m_consumption);
        
        var firePoint = GetCurrentFirepoint();
        if (firePoint == null)
            return;

        var startInfos = new ProjectileStartInfos();
        startInfos.name = m_projectileType.GetValue();
        startInfos.caster = gameObject;
        startInfos.target = GetTarget();
        startInfos.position = firePoint.position;
        startInfos.rotation = firePoint.rotation;

        ProjectileBase.ThrowProjectile(startInfos);

        if (SoundSystem.instance != null)
        {
            SoundSystem.instance.PlaySound(m_fireSound, firePoint.position, m_fireSoundVolume);
        }
    }

    protected override bool IsContinuousWeapon()
    {
        return false;
    }


    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);
    }
}