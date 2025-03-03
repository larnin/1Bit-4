using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

public class BuildingPylon : BuildingBase
{
    [SerializeField] float m_placementRadius = 5;
    [SerializeField] bool m_bigPylon = false;

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
        if (m_bigPylon)
            return BuildingType.BigPylon;
        return BuildingType.Pylon;
    }

    public override float PlacementRadius()
    {
        return m_placementRadius;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);
    }
}
