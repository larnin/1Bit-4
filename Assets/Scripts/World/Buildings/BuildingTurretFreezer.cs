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
    [SerializeField] GameObject m_projectilePrefab;

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

    protected override bool IsContinuousWeapon()
    {
        return false;
    }


    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        //UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Power storage").SetValueFunc(GetEnergy).SetMaxFunc(GetStorage).SetNbDigits(1).SetValueDisplayType(UIElementFillValueDisplayType.classic);
    }
}