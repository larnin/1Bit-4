using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTurretEnergy : BuildingTurretBase
{
    [SerializeField] float m_energyStorage = 10;
    [SerializeField] float m_energyUptake = 2;
    [SerializeField] float m_energyPerFire = 1;
    [SerializeField] GameObject m_projectilePrefab;

    float m_energy = 0;

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
        return BuildingType.Turret1;
    }

    public override float EnergyUptakeWanted()
    {
        if (m_energy < m_energyStorage)
            return m_energyUptake;
        return 0;
    }

    public override void EnergyUptake(float value)
    {
        m_energy += value * Time.deltaTime;
        if (m_energy > m_energyStorage)
            m_energy = m_energyStorage;
    }

    protected override bool CanFire()
    {
        if (m_energy < m_energyPerFire)
            return false;

        var firePoint = GetCurrentFirepoint();
        if (firePoint == null)
            return false;

        if (!IsTargetVisible(firePoint.position))
            return false;

        return true;
    }

    protected override bool IsContinuousWeapon()
    {
        return false;
    }

    protected override void Fire()
    {
        if (!CanFire())
            return;

        m_energy -= m_energyPerFire;
        
        var firePoint = GetCurrentFirepoint();
        if (firePoint == null)
            return;

        if (m_projectilePrefab != null)
        {
            var obj = Instantiate(m_projectilePrefab);
            obj.transform.position = firePoint.position;
            obj.transform.rotation = firePoint.rotation;

            var projectile = obj.GetComponent<ProjectileBase>();
            if (projectile != null)
            {
                var target = GetTarget();
                if (target != null)
                {
                    projectile.SetTarget(target);
                    projectile.SetCaster(gameObject);

                    var multiplier = new GetStatEvent(StatType.DamagesMultiplier);
                    Event<GetStatEvent>.Broadcast(multiplier, gameObject);
                    projectile.SetDamagesMultiplier(multiplier.GetValue());
                }
            }
        }
    }

    float GetEnergy()
    {
        return m_energy;
    }

    float GetStorage()
    {
        return m_energyStorage;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Power storage").SetValueFunc(GetEnergy).SetMaxFunc(GetStorage).SetNbDigits(1).SetValueDisplayType(UIElementFillValueDisplayType.classic);
    }
}
