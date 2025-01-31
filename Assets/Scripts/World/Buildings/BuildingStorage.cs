using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingStorage : BuildingBase
{
    [SerializeField] float m_energyUptake;

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
        return BuildingType.Storage;
    }

    public override float EnergyUptakeWanted()
    {
        return m_energyUptake;
    }

    string EnergyUptakeStr()
    {
        return m_energyUptake.ToString("#0.##");
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Energy Uptake").SetTextFunc(EnergyUptakeStr);
    }
}
