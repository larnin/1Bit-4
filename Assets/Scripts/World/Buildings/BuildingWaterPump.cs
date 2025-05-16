using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingWaterPump : BuildingBase
{
    [SerializeField] float m_energyConsumption = 1;
    [SerializeField] ResourceType m_generatedResource;
    [SerializeField] float m_generation = 1;

    float m_energyUptake;
    float m_energyEfficiency = 1;
    bool m_connectedToWater = false;
    Rotation m_waterDirection;

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
        return BuildingType.WaterPump;
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
        m_connectedToWater = HaveWaterAround(GetPos());
    }

    protected override void OnUpdate()
    {
        if (m_connectedToWater)
        {
            float count = Time.deltaTime * m_energyEfficiency * m_generation;

            if (ResourceSystem.instance != null)
                ResourceSystem.instance.AddResource(m_generatedResource, count);
        }
    }

    public override BuildingPlaceType CanBePlaced(Vector3Int pos)
    {
        var canPlace = base.CanBePlaced(pos);
        if (canPlace != BuildingPlaceType.Valid)
            return canPlace;

        if (HaveWaterAround(pos))
            return BuildingPlaceType.Valid;
        return BuildingPlaceType.NeedWater;
    }

    bool HaveWaterAround(Vector3Int pos)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return false;

        var bounds = new BoundsInt(pos, GetSize());

        var size = bounds.size;

        for(int i = 0; i < 4; i++)
        {
            var rot = (Rotation)i;
            var dir = RotationEx.ToVectorInt(rot);
            var ortoDir = RotationEx.ToVectorInt(RotationEx.Add(rot, Rotation.rot_90));

            int dist = Mathf.Abs(ortoDir.x * size.x) + Mathf.Abs(ortoDir.y * size.z);

            Vector3Int initialPos = bounds.min - new Vector3Int(0, 1, 0);
            if (rot == Rotation.rot_0 || rot == Rotation.rot_90)
                initialPos.x += size.x - 1;
            if (rot == Rotation.rot_90 || rot == Rotation.rot_180)
                initialPos.z += size.z - 1;
            initialPos += new Vector3Int(dir.x, 0, dir.y);

            bool allWater = true;

            for(int j = 0; j < dist; j++)
            {
                var currentPos = initialPos + new Vector3Int(ortoDir.x, 0, ortoDir.y) * j;
                currentPos = GridEx.GetRealPosFromLoop(grid.grid, currentPos);
                if (GridEx.GetBlock(grid.grid, currentPos).type != BlockType.water)
                {
                    allWater = false;
                    break;
                }
            }

            if (allWater)
            {
                m_connectedToWater = true;
                m_waterDirection = rot;
                return true;
            }
        }

        m_connectedToWater = false;

        return false;
    }

    string EnergyUptakeStr()
    {
        return m_energyUptake.ToString("#0.##");
    }

    string CollectionStr()
    {
        return (m_generation * m_energyEfficiency).ToString("#0.##");
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
        if (r != null)
        {
            string label = r.name + " Collection";
            UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel(label).SetTextFunc(CollectionStr);
        }
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Efficiency").SetMax(1).SetValueFunc(GetEfficiency).SetValueDisplayType(UIElementFillValueDisplayType.percent).SetNbDigits(0);
    }

    public override void UpdateRotation()
    {
        SetRotation(RotationEx.Sub(m_waterDirection, Rotation.rot_90));
    }
}
