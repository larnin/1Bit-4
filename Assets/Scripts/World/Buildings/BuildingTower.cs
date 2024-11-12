using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTower : BuildingBase
{
    [SerializeField] float m_powerGeneration = 10;
    [SerializeField] float m_placementRadius = 5;

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
        return BuildingType.Tower;
    }

    public override float EnergyGeneration()
    {
        return m_powerGeneration;
    }

    public override float PlacementRadius()
    {
        return m_placementRadius;
    }

    float value = 0;

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Power").SetText(m_powerGeneration.ToString());
    }
}
