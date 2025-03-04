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
    [SerializeField] int m_distanceToOtherPannel = 2;

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

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Power").SetText(generation.ToString("#0.##"));
    }

    public override BuildingPlaceType CanBePlaced(Vector3Int pos)
    {
        var canPlace = base.CanBePlaced(pos);
        if (canPlace != BuildingPlaceType.Valid)
            return canPlace;

        if (BuildingList.instance == null)
            return BuildingPlaceType.Valid;

        var pannels = BuildingList.instance.GetAllBuilding(BuildingType.SolarPanel, Team.Player);
        pannels.AddRange(BuildingList.instance.GetAllBuilding(BuildingType.BigSolarPanel, Team.Player));

        var currentBounds = GetBounds(pos);
        var dist = new Vector3Int(m_distanceToOtherPannel, m_distanceToOtherPannel, m_distanceToOtherPannel);

        foreach (var p in pannels)
        {
            var bounds = p.GetBounds();
            bounds = new BoundsInt(bounds.position - dist, bounds.size + dist * 2);
            if (bounds.Intersects(currentBounds))
                return BuildingPlaceType.TooCloseSolarPannel;
        }

        return BuildingPlaceType.Valid;
    }

    public override void UpdateRotation()
    {
        if (GetRotation() != Rotation.rot_0)
            SetRotation(Rotation.rot_0);
    }
}

