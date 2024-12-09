using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingPowerPlant : BuildingBase
{
    [SerializeField] float m_powerGeneration = 1;
    [SerializeField] List<OneResourceCost> m_resourceConsumption;

    float m_efficiency = 1;

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
        return BuildingType.PowerPlant;
    }

    public override float EnergyGeneration()
    {
        return m_powerGeneration * m_efficiency;
    }

    protected override void Update()
    {
        m_efficiency = 0;

        if (GameInfos.instance.paused)
            return;

        if (ConnexionSystem.instance != null && !ConnexionSystem.instance.IsConnected(this))
            return;

        if (ResourceSystem.instance == null)
            return;

        m_efficiency = 1;

        foreach(var r in m_resourceConsumption)
        {
            if(!ResourceSystem.instance.HaveResource(r.type))
            {
                m_efficiency = 0;
                break;
            }

            float stored = ResourceSystem.instance.GetResourceStored(r.type);
            float wanted = r.count * Time.deltaTime;
            if(wanted > stored)
            {
                float percent = stored / wanted;
                if (percent < m_efficiency)
                    m_efficiency = percent;
            }
        }

        foreach(var r in m_resourceConsumption)
        {
            float wanted = r.count * Time.deltaTime * m_efficiency;
            ResourceSystem.instance.RemoveResource(r.type, wanted);
        }
    }

    string EnergyGenerationStr()
    {
        return (m_powerGeneration * m_efficiency).ToString();
    }

    string OneResourceUptake(int index)
    {
        if (index < 0 || index >= m_resourceConsumption.Count)
            return "0";

        return (m_resourceConsumption[index].count * m_efficiency).ToString();
    }
    

    float GetEfficiency()
    {
        return m_efficiency;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Energy Generation").SetTextFunc(EnergyGenerationStr);

        for(int i = 0; i < m_resourceConsumption.Count; i++)
        {
            int index = i;
            var r = Global.instance.resourceDatas.GetResource(m_resourceConsumption[i].type);
            if (r == null)
                continue;

            string label = r.name + " Uptake";
            UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel(label).SetTextFunc(() => { return OneResourceUptake(index); });
        }
        
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Efficiency").SetMax(1).SetValueFunc(GetEfficiency).SetValueDisplayType(UIElementFillValueDisplayType.percent).SetNbDigits(0);
    }
}

