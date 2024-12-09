using System;
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

    float m_energyUptake;
    float m_consumeMultiplier = 0;
    float m_energyEfficiency = 1;
    float m_timer = 0;
    List<Vector3Int> m_titaniums = new List<Vector3Int>();

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
        return BuildingType.CrystalMine;
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
        m_titaniums = GetTitaniumsAround(GetPos());
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

        m_consumeMultiplier = 0;
        if (ResourceSystem.instance != null)
        {
            if (ResourceSystem.instance.HaveResource(m_consumedResource))
            {
                float consumeCount = m_consumedResourceNb * Time.deltaTime;
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
                    ResourceSystem.instance.RemoveResource(m_consumedResource, m_consumedResourceNb);
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

    public override bool CanBePlaced(Vector3Int pos)
    {
        if (!base.CanBePlaced(pos))
            return false;

        var points = GetTitaniumsAround(pos);
        return points.Count > 0;
    }

    List<Vector3Int> GetTitaniumsAround(Vector3Int pos)
    {
        List<Vector3Int> points = new List<Vector3Int>();

        GetGridEvent grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        if (grid.grid == null)
            return points;

        var bounds = new BoundsInt(pos, GetSize());

        var min = bounds.min;
        var max = bounds.max - Vector3Int.one;
        min -= new Vector3Int(m_mineRadius, 0, m_mineRadius);
        max += new Vector3Int(m_mineRadius, 0, m_mineRadius);

        for (int i = min.x; i <= max.x; i++)
        {
            for (int j = min.z; j <= max.z; j++)
            {
                var offset = new Vector2Int(i - bounds.min.x, j - bounds.min.z);
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

                int height = GridEx.GetHeight(grid.grid, new Vector2Int(i, j));
                if (height < 0 || height < pos.y || height >= bounds.max.y)
                    continue;

                Vector3Int itemPos = new Vector3Int(i, height, j);
                var item = GridEx.GetBlock(grid.grid, itemPos);
                if (item != BlockType.Titanium)
                    continue;

                IsTitaniumUsedEvent titanium = new IsTitaniumUsedEvent(itemPos);
                Event<IsTitaniumUsedEvent>.Broadcast(titanium);
                if (titanium.used)
                    continue;

                points.Add(itemPos);
            }
        }

        return points;
    }

    public void IsTitaniumUsed(IsTitaniumUsedEvent e)
    {
        if (e.used)
            return;

        foreach (var p in m_titaniums)
        {
            if (p == e.pos)
            {
                e.used = true;
                break;
            }
        }
    }
    string EnergyUptakeStr()
    {
        return m_energyUptake.ToString();
    }

    string ResourceUptakeStr()
    {
        return (m_consumedResourceNb * m_consumeMultiplier).ToString();
    }

    string TitaniumCollectionStr()
    {
        return m_generatedResourceNb.ToString();
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
}
