using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingBattery : BuildingBase
{
    [SerializeField] float m_energyStorage = 10;
    [SerializeField] float m_energyUptake = 2;
    [SerializeField] bool m_bigBattery = false;

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
        if (m_bigBattery)
            return BuildingType.BigBattery;
        return BuildingType.Battery;
    }

    public override float EnergyUptakeWanted() 
    {
        if(m_energy < m_energyStorage)
            return m_energyUptake;
        return 0;
    }

    public override void EnergyUptake(float value)
    {
        m_energy += value * Time.deltaTime;
        if (m_energy > m_energyStorage)
            m_energy = m_energyStorage;
    }

    public override EnergyUptakePriority EnergyPriority() 
    { 
        return EnergyUptakePriority.storage; 
    }

    public override float EnergyStorageValue() 
    { 
        return m_energy; 
    }

    public override float EnergyStorageMax() 
    {
        return m_energyStorage;
    }

    public override void ConsumeStoredEnergy(float value) 
    {
        m_energy -= value;
        if (m_energy < 0)
            m_energy = 0;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Energy storage").SetValueFunc(EnergyStorageValue).SetMaxFunc(EnergyStorageMax).SetValueDisplayType(UIElementFillValueDisplayType.classic).SetNbDigits(0);
    }
}
