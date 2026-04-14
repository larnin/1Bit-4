using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingGeothermalGenerator : BuildingBase
{
    [SerializeField] float m_powerGeneration = 1;

    bool m_havePit = false;
    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    public override void Start()
    {
        base.Start();
        m_havePit = HavePit(GetPos());
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_subscriberList.Unsubscribe();
    }

    public override BuildingType GetBuildingType()
    {
        return BuildingType.GeothermalGenerator;
    }

    public override float EnergyGeneration()
    {
        if(m_havePit)
            return m_powerGeneration;
        return 0;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        float generation = EnergyGeneration();
        float efficiency = generation / m_powerGeneration;

        if (m_havePit)
            UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Power").SetText(generation.ToString("#0.##"));
        else UIElementData.Create<UIElementSimpleText>(e.container).SetText("No on a Geothermal pit");
    }

    public override BuildingPlaceType CanBePlaced(Vector3Int pos)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
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
                    if (i == pos.x && k == pos.z)
                    {
                        if (ground.type != BlockType.Geothermal)
                            return BuildingPlaceType.NeedGeothermal;
                    }
                    else if (ground.type != BlockType.ground)
                        return BuildingPlaceType.InvalidPlace;
                    else if (BlockEx.GetShapeFromData(ground.data) != BlockShape.Full)
                        return BuildingPlaceType.InvalidPlace;

                    for (int j = min.y; j < max.y; j++)
                    {
                        var block = GridEx.GetBlock(grid.grid, new Vector3Int(i, j, k));
                        if (block.type != BlockType.air)
                            return BuildingPlaceType.InvalidPlace;
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

            if (GridEx.IntersectLoop(grid.grid, otherBounds, bounds))
                return BuildingPlaceType.InvalidPlace;
        }

        return BuildingPlaceType.Valid;
    }

    bool HavePit(Vector3Int pos)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return false;

        int height = GridEx.GetHeight(grid.grid, new Vector2Int(pos.x, pos.z));
        pos.y = height;
        var item = GridEx.GetBlock(grid.grid, pos);

        return item.type == BlockType.Geothermal;
    }
}
