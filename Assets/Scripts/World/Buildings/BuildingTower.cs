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
        UIElementData.Create<UIElementSimpleText>(e.container).SetText("Title").SetAlignment(UIElementAlignment.center);
        UIElementData.Create<UIElementSprite>(e.container).SetSprite(Global.instance.buildingDatas.GetBuilding(BuildingType.Tower).sprite).SetNativeSize().SetAlignment(UIElementAlignment.right);
        UIElementData.Create<UIElementLine>(e.container);
        UIElementData.Create<UIElementFillValue>(e.container).SetValueFunc(() => { 
            value += Time.deltaTime; if (value >= 100) value = 0; return value; 
        }).SetMax(100).SetLabel("Awesomeness:").SetNbDigits(0).SetValueDisplayType(UIElementFillValueDisplayType.percent);
        UIElementData.Create<UIElementLine>(e.container);
        UIElementData.Create<UIElementSimpleText>(e.container).SetText("Some longer text that use at least 2 lignes, and maybe more");
        UIElementData.Create<UIElementLine>(e.container);
        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("The Label").SetText("You can use a duck to elevate the sun in a small cave").SetTextAlignment(UIElementAlignment.center);
        UIElementData.Create<UIElementLine>(e.container);
    }
}
