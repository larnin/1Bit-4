using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingCrystalMine : BuildingBase
{
    [SerializeField] float m_energyConsumption = 1;
    [SerializeField] ResourceType m_generatedResource;
    [SerializeField] float m_generation = 1;
    [SerializeField] int m_mineRadius = 1;

    class MineData
    {
        public Vector3Int pos;
        public GameObject mineObject;
    }

    float m_energyUptake;
    float m_energyEfficiency = 1;
    List<MineData> m_crystals = new List<MineData>();

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Add(new Event<IsCrystalUsedEvent>.Subscriber(IsCrystalUsed));
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
        m_crystals = GetCrystalsAround(GetPos());
        foreach (var item in m_crystals)
            CreateMineItem(item);
    }

    protected override void OnUpdate()
    {
        float count = m_crystals.Count * Time.deltaTime * m_energyEfficiency * m_generation;

        if (ResourceSystem.instance != null)
            ResourceSystem.instance.AddResource(m_generatedResource, count);
    }

    public override BuildingPlaceType CanBePlaced(Vector3Int pos) 
    {
        var points = GetCrystalsAround(pos);
        UpdateCursorCrystalsUsed(points);

        var canPlace = base.CanBePlaced(pos);
        if (canPlace != BuildingPlaceType.Valid)
            return canPlace;
        
        if (m_crystals.Count > 0)
            return BuildingPlaceType.Valid;
        return BuildingPlaceType.NeedCrystal;
    }

    List<MineData> GetCrystalsAround(Vector3Int pos)
    {
        List<MineData> points = new List<MineData>();

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
                if (height < 0 || Mathf.Abs(pos.y - height) > 1)
                    continue;

                Vector3Int itemPos = new Vector3Int(i, height, j);
                var item = GridEx.GetBlock(grid.grid, itemPos);
                if (item != BlockType.crystal)
                    continue;

                IsCrystalUsedEvent crystal = new IsCrystalUsedEvent(itemPos);
                Event<IsCrystalUsedEvent>.Broadcast(crystal);
                if (crystal.used)
                    continue;

                MineData data = new MineData();
                data.pos = itemPos;

                points.Add(data);
            }
        }

        return points;
    }

    public void IsCrystalUsed(IsCrystalUsedEvent e)
    {
        if (e.used)
            return;

        if (!IsAdded())
            return;

        foreach(var p in m_crystals)
        {
            if(p.pos == e.pos)
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

    string CrystalCollectionStr()
    {
        return (m_generation * m_crystals.Count * m_energyEfficiency).ToString("#0.##");
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
            string label = r.name + " Collection";
        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel(label).SetTextFunc(CrystalCollectionStr);
        }
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Efficiency").SetMax(1).SetValueFunc(GetEfficiency).SetValueDisplayType(UIElementFillValueDisplayType.percent).SetNbDigits(0);
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

    void UpdateCursorCrystalsUsed(List<MineData> datas)
    {
        foreach(var d in datas)
        {
            var item = m_crystals.Find(x => { return x.pos == d.pos; });
            if (item != null)
                d.mineObject = item.mineObject;
            else CreateMineItem(d);
        }

        foreach(var item in m_crystals)
        {
            var d = datas.Find(x => { return x.pos == item.pos; });
            if (d == null)
                Destroy(item.mineObject);
        }

        m_crystals = datas;

        foreach(var d in datas)
            d.mineObject.transform.position = d.pos;
    }
}

