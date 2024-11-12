using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingPylon : BuildingBase
{
    [SerializeField] float m_powerGeneration = 1;
    [SerializeField] float m_generationRadius = 4;
    [SerializeField] float m_placementRadius = 5;
    [SerializeField] bool m_bigPylon = false;

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
        if (m_bigPylon)
            return BuildingType.BigPylon;
        return BuildingType.Pylon;
    }

    public override float EnergyGeneration()
    {
        if (ConnexionSystem.instance == null)
            return m_powerGeneration;

        float maxDistance = m_generationRadius * m_generationRadius;
        var pos = GetGroundCenter();
        var connected = ConnexionSystem.instance.GetConnectedBuilding(this);
        foreach(var c in connected)
        {
            if (c.GetBuildingType() != BuildingType.Pylon)
                continue;
            var otherPos = c.GetGroundCenter();
            float dist = (pos - otherPos).sqrMagnitude;

            if (dist > maxDistance)
                continue;
            maxDistance = dist;
        }

        float powerMultiplier = maxDistance / m_generationRadius / m_generationRadius;

        return m_powerGeneration * powerMultiplier;
    }

    public override float PlacementRadius()
    {
        return m_placementRadius;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        float generation = EnergyGeneration();
        float efficiency = generation / m_powerGeneration;

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Power").SetText(generation.ToString());

        if(efficiency < 0.99f)
        {
            UIElementData.Create<UIElementSimpleText>(e.container).SetText("Pylons Too close to each other");
            UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Efficiency").SetText(((int)(efficiency * 100)).ToString() + "%");
        }
    }
}
