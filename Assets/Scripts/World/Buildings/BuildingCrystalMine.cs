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

    float m_energyEfficiency = 1;
    List<Vector3Int> m_crystals = new List<Vector3Int>();

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<IsCrystalUsedEvent>.Subscriber(IsCrystalUsed));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
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
        m_energyEfficiency = value / m_energyConsumption;
        if (m_energyEfficiency > 1)
            m_energyEfficiency = 1;

        m_energyEfficiency *= m_energyEfficiency;
    }

    public override void Start()
    {
        base.Start();
        m_crystals = GetCrystalsAround(GetPos());
    }

    protected override void Update()
    {
        base.Update();

        float count = m_crystals.Count * Time.deltaTime * m_energyEfficiency * m_generation;

        if (ResourceSystem.instance != null)
            ResourceSystem.instance.AddResource(m_generatedResource, count);
    }

    public override bool CanBePlaced(Vector3Int pos) 
    {
        if (!base.CanBePlaced(pos))
            return false;

        var points = GetCrystalsAround(pos);
        return points.Count > 0;
    }

    List<Vector3Int> GetCrystalsAround(Vector3Int pos)
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
            for (int j = min.z; j < max.z; j++)
            {
                var offset = min - bounds.min;
                if (offset.x > 0 && offset.x < bounds.size.x)
                    offset.x = 0;
                else if (offset.x >= bounds.size.x)
                    offset.x -= bounds.size.x;
                if (offset.y > 0 && offset.y < bounds.size.y)
                    offset.y = 0;
                else if (offset.y >= bounds.size.y)
                    offset.y -= bounds.size.y;

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

                points.Add(itemPos);
            }
        }

        return points;
    }

    public void IsCrystalUsed(IsCrystalUsedEvent e)
    {
        if (e.used)
            return;

        foreach(var p in m_crystals)
        {
            if(p == e.pos)
            {
                e.used = true;
                break;
            }
        }
    }
}

