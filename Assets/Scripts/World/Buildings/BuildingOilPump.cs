using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingOilPump : BuildingBase
{
    [SerializeField] float m_energyConsumption = 1;
    [SerializeField] ResourceType m_generatedResource;
    [SerializeField] float m_generation = 1;

    float m_energyUptake;
    float m_energyEfficiency = 1;
    bool m_onOilSpot = false;

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
        return BuildingType.OilPump;
    }

    public override float EnergyUptakeWanted()
    {
        return m_energyConsumption;
    }

    public override void EnergyUptake(float value)
    {
        m_energyUptake = value;
        m_energyEfficiency = value / m_energyConsumption;
        if (m_energyEfficiency > 1)
            m_energyEfficiency = 1;

        m_energyEfficiency *= m_energyEfficiency;
    }

    public override void Start()
    {
        base.Start();
        m_onOilSpot = HaveOilSpot(GetPos());
    }

    protected override void Update()
    {
        base.Update();

        if (GameInfos.instance.paused)
            return;

        if (!IsAdded())
            return;

        if (ConnexionSystem.instance != null && !ConnexionSystem.instance.IsConnected(this))
            return;

        if (m_onOilSpot)
        {
            float count = Time.deltaTime * m_energyEfficiency * m_generation;

            if (ResourceSystem.instance != null)
                ResourceSystem.instance.AddResource(m_generatedResource, count);
        }
    }

    public override bool CanBePlaced(Vector3Int pos)
    {
        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        var bounds = GetBounds(pos);
        if (grid.grid != null)
        {
            Vector3Int min = bounds.min;
            Vector3Int max = bounds.max;

            for (int i = min.x; i < max.x; i++)
            {
                for (int k = min.z; k < max.z; k++)
                {
                    var ground = GridEx.GetBlock(grid.grid, new Vector3Int(i, min.y - 1, k));
                    if(i == pos.x && k == pos.z)
                    {
                        if (ground != BlockType.oil)
                            return false;
                    } else if (ground != BlockType.ground)
                        return false;

                    for (int j = min.y; j < max.y; j++)
                    {
                        var block = GridEx.GetBlock(grid.grid, new Vector3Int(i, j, k));
                        if (block != BlockType.air)
                            return false;
                    }
                }
            }
        }

        //test if an other building already here
        int nbBuilding = BuildingList.instance.GetBuildingNb();
        for (int i = 0; i < nbBuilding; i++)
        {
            var b = BuildingList.instance.GetBuildingFromIndex(i);
            var otherBounds = b.GetBounds();

            if (Utility.Intersects(otherBounds, bounds))
                return false;
        }

        return true;
    }

    bool HaveOilSpot(Vector3Int pos)
    {
        GetGridEvent grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        if (grid.grid == null)
            return false;

        int height = GridEx.GetHeight(grid.grid, new Vector2Int(pos.x, pos.z));
        pos.y = height;
        var item = GridEx.GetBlock(grid.grid, pos);

        return item == BlockType.oil;
    }

    string EnergyUptakeStr()
    {
        return m_energyUptake.ToString();
    }

    string OilCollectionStr()
    {
        return (m_generation * m_energyEfficiency).ToString();
    }

    float GetEfficiency()
    {
        return m_energyEfficiency;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Energy Uptake").SetTextFunc(EnergyUptakeStr);

        var r = Global.instance.resourceDatas.GetResource(m_generatedResource);
        if(r != null)
        {
            var label = r.name + " Collection";
            UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel(label).SetTextFunc(OilCollectionStr);
        }
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Efficiency").SetMax(1).SetValueFunc(GetEfficiency).SetValueDisplayType(UIElementFillValueDisplayType.percent).SetNbDigits(0);
    }
}
