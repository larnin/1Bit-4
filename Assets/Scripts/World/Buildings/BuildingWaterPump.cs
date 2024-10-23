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
    [SerializeField] int m_pumpRadius = 1;

    float m_energyEfficiency = 1;
    bool m_connectedToWater = false;

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

    protected override void Update()
    {
        base.Update();

        if (!IsAdded())
            return;

        if (ConnexionSystem.instance != null && !ConnexionSystem.instance.IsConnected(this))
            return;

        if (m_connectedToWater)
        {
            float count = Time.deltaTime * m_energyEfficiency * m_generation;

            if (ResourceSystem.instance != null)
                ResourceSystem.instance.AddResource(m_generatedResource, count);
        }
    }

    public override bool CanBePlaced(Vector3Int pos)
    {
        if (!base.CanBePlaced(pos))
            return false;

        return HaveWaterAround(pos);
    }

    bool HaveWaterAround(Vector3Int pos)
    {
        GetGridEvent grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        if (grid.grid == null)
            return false;

        var bounds = new BoundsInt(pos, GetSize());

        var min = bounds.min;
        var max = bounds.max - Vector3Int.one;
        min -= new Vector3Int(m_pumpRadius, 0, m_pumpRadius);
        max += new Vector3Int(m_pumpRadius, 0, m_pumpRadius);

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

                if (MathF.Abs(offset.x) + Math.Abs(offset.y) > m_pumpRadius)
                    continue;

                int height = GridEx.GetHeight(grid.grid, new Vector2Int(i, j));
                if (height < 0 || Mathf.Abs(pos.y - height) > 1)
                    continue;

                Vector3Int itemPos = new Vector3Int(i, height, j);
                var item = GridEx.GetBlock(grid.grid, itemPos);
                if (item != BlockType.water)
                    continue;
                
                return true;
            }
        }

        return false;
    }
}
