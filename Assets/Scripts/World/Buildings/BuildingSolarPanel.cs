using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingSolarPanel : BuildingBase
{
    [SerializeField] float m_powerGeneration = 1;
    [SerializeField] bool m_bigPanel = false;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public override BuildingType GetBuildingType()
    {
        if (m_bigPanel)
            return BuildingType.BigSolarPanel;
        return BuildingType.SolarPanel;
    }

    public override float EnergyGeneration()
    {
        return m_powerGeneration;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        float generation = EnergyGeneration();
        float efficiency = generation / m_powerGeneration;

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Power").SetText(generation.ToString());
    }
}

