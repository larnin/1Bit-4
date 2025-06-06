﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTitaniumMine : BuildingBase
{
    [SerializeField] float m_energyConsumption = 1;
    [SerializeField] ResourceType m_consumedResource;
    [SerializeField] float m_consumedResourceNb = 1;
    [SerializeField] ResourceType m_generatedResource;
    [SerializeField] float m_generatedResourceNb = 1;
    [SerializeField] float m_generatedResourceCycle = 1;
    [SerializeField] int m_mineRadius = 1;

    class MineData
    {
        public Vector3Int pos;
        public GameObject mineObject;
    }

    float m_energyUptake;
    float m_consumeMultiplier = 0;
    float m_energyEfficiency = 1;
    float m_timer = 0;
    List<MineData> m_titaniums = new List<MineData>();

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Add(new Event<IsTitaniumUsedEvent>.Subscriber(IsTitaniumUsed));
        m_subscriberList.Subscribe();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_subscriberList.Unsubscribe();
    }

    public override BuildingType GetBuildingType()
    {
        return BuildingType.TitaniumMine;
    }

    public override float EnergyUptakeWanted()
    {
        return m_energyConsumption * m_titaniums.Count;
    }

    public override void EnergyUptake(float value)
    {
        m_energyUptake = value;
        m_energyEfficiency = value / EnergyUptakeWanted();
        if (m_energyEfficiency > 1)
            m_energyEfficiency = 1;

        m_energyEfficiency *= m_energyEfficiency;
    }

    public override void Start()
    {
        base.Start();
        if (m_titaniums.Count != 0)
        {
            foreach (var c in m_titaniums)
            {
                if (c.mineObject != null)
                    Destroy(c.mineObject);
            }
            m_titaniums.Clear();
        }
        m_titaniums = GetTitaniumsAround(GetPos());
        foreach (var item in m_titaniums)
            CreateMineItem(item);
    }

    protected override void OnUpdate()
    {
        m_consumeMultiplier = 0;
        if (ResourceSystem.instance != null)
        {
            if (ResourceSystem.instance.HaveResource(m_consumedResource))
            {
                float consumeCount = m_consumedResourceNb * Time.deltaTime * m_titaniums.Count * m_energyEfficiency;
                float stored = ResourceSystem.instance.GetResourceStored(m_consumedResource);
                if(stored > 0 && consumeCount > 0)
                {
                    if (stored > consumeCount)
                        m_consumeMultiplier = 1;
                    else
                    {
                        m_consumeMultiplier = stored / consumeCount;
                        consumeCount = stored;
                    }
                }

                if (consumeCount > 0)
                    ResourceSystem.instance.RemoveResource(m_consumedResource, consumeCount);
            }
        }

        if (m_consumeMultiplier > 0)
        {
            m_timer += Time.deltaTime * m_energyEfficiency * m_consumeMultiplier;
            if (m_timer >= m_generatedResourceCycle)
            {
                m_timer -= m_generatedResourceCycle;
                if (ResourceSystem.instance != null)
                    ResourceSystem.instance.AddResource(m_generatedResource, m_generatedResourceNb * m_titaniums.Count);
            }
        }
    }

    public override BuildingPlaceType CanBePlaced(Vector3Int pos)
    {
        var points = GetTitaniumsAround(pos);
        UpdateCursorMinesUsed(points);

        var canPlace = base.CanBePlaced(pos);
        if (canPlace != BuildingPlaceType.Valid)
            return canPlace;
        
        if (m_titaniums.Count > 0)
            return BuildingPlaceType.Valid;
        return BuildingPlaceType.NeedTitanim;
    }

    List<MineData> GetTitaniumsAround(Vector3Int pos)
    {
        List<MineData> points = new List<MineData>();

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return points;

        var bounds = new BoundsInt(pos, GetSize());

        var min = bounds.min;
        var max = bounds.max - Vector3Int.one;
        min -= new Vector3Int(m_mineRadius, 0, m_mineRadius);
        max += new Vector3Int(m_mineRadius, 0, m_mineRadius);

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.y; j <= max.y; j++)
            {
                for (int k = min.z; k <= max.z; k++)
                {
                    var offset = new Vector2Int(i - bounds.min.x, k - bounds.min.z);
                    if (offset.x > 0 && offset.x < bounds.size.x)
                        offset.x = 0;
                    else if (offset.x >= bounds.size.x)
                        offset.x -= bounds.size.x - 1;
                    if (offset.y > 0 && offset.y < bounds.size.z)
                        offset.y = 0;
                    else if (offset.y >= bounds.size.z)
                        offset.y -= bounds.size.z - 1;

                    if (offset.x == 0 && offset.y == 0)
                        continue;

                    if (MathF.Abs(offset.x) + Math.Abs(offset.y) > m_mineRadius)
                        continue;

                    var realPos = GridEx.GetRealPosFromLoop(grid.grid, new Vector3Int(i, 0, j));

                    Vector3Int itemPos = new Vector3Int(realPos.x, j, realPos.z);
                    var item = GridEx.GetBlock(grid.grid, itemPos);
                    if (item.type != BlockType.Titanium)
                        continue;

                    var titanium = Event<IsTitaniumUsedEvent>.Broadcast(new IsTitaniumUsedEvent(itemPos));
                    if (titanium.used)
                        continue;

                    MineData data = new MineData();
                    data.pos = itemPos;

                    points.Add(data);
                }
            }
        }

        return points;
    }

    public void IsTitaniumUsed(IsTitaniumUsedEvent e)
    {
        if (!IsAdded())
            return;

        if (e.used)
            return;

        foreach (var p in m_titaniums)
        {
            if (p.pos == e.pos)
            {
                e.used = true;
                break;
            }
        }
    }
    string EnergyUptakeStr()
    {
        return m_energyUptake.ToString("#0.##");
    }

    string ResourceUptakeStr()
    {
        return (m_consumedResourceNb * m_consumeMultiplier).ToString("#0.##");
    }

    string TitaniumCollectionStr()
    {
        return m_generatedResourceNb.ToString("#0.##");
    }

    float GetCycleValue()
    {
        return m_timer;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Energy Uptake").SetTextFunc(EnergyUptakeStr);
        var r = Global.instance.resourceDatas.GetResource(m_consumedResource);
        if (r != null)
        {
            string label = r.name + " Uptake";
            UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel(label).SetTextFunc(ResourceUptakeStr);
        }

        r = Global.instance.resourceDatas.GetResource(m_generatedResource);
        if(r != null)
        {
            string label = r.name + " Collection Each Cycle";
            UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel(label).SetTextFunc(TitaniumCollectionStr);
        }
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Cycle").SetMax(m_generatedResourceCycle).SetValueFunc(GetCycleValue).SetValueDisplayType(UIElementFillValueDisplayType.percent).SetNbDigits(0);
    }


    void CreateMineItem(MineData data)
    {
        var prefab = Global.instance.buildingDatas.mineItemPrefab;
        if (prefab == null)
            return;

        data.mineObject = Instantiate(prefab);
        data.mineObject.transform.parent = transform;
        data.mineObject.transform.rotation = Quaternion.identity;

        data.mineObject.transform.position = data.pos;
    }

    void UpdateCursorMinesUsed(List<MineData> datas)
    {
        foreach (var d in datas)
        {
            var item = m_titaniums.Find(x => { return x.pos == d.pos; });
            if (item != null)
                d.mineObject = item.mineObject;
            else CreateMineItem(d);
        }

        foreach (var item in m_titaniums)
        {
            var d = datas.Find(x => { return x.pos == item.pos; });
            if (d == null)
                Destroy(item.mineObject);
        }

        m_titaniums = datas;

        foreach (var d in datas)
            d.mineObject.transform.position = d.pos;
    }
}
