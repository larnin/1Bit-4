using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    List<BuildingBase> m_buildings = new List<BuildingBase>();

    static BuildingList m_instance = null;
    public static BuildingList instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void Register(BuildingBase building)
    {
        m_buildings.Add(building);

        if (ConnexionSystem.instance != null)
            ConnexionSystem.instance.OnBuildingChange();
    }

    public void UnRegister(BuildingBase building)
    {
        m_buildings.Remove(building);

        if (ConnexionSystem.instance != null)
            ConnexionSystem.instance.OnBuildingChange();
    }

    public int GetBuildingNb()
    {
        return m_buildings.Count;
    }

    public BuildingBase GetBuildingFromIndex(int index)
    {
        if (index < 0 || index >= m_buildings.Count)
            return null;

        return m_buildings[index];
    }

    public BuildingBase GetFirstBuilding(BuildingType type)
    {
        foreach (var building in m_buildings)
        {
            if (building.GetBuildingType() == type)
                return building;
        }

        return null;
    }

    public List<BuildingBase> GetAllBuilding(BuildingType type)
    {
        List<BuildingBase> buildings = new List<BuildingBase>();

        foreach(var building in m_buildings)
        {
            if (building.GetBuildingType() == type)
                buildings.Add(building);
        }

        return buildings;
    }

    public BuildingBase GetNearestBuilding(Vector3 pos)
    {
        float bestDistance = 0;
        BuildingBase bestBuilding = null;

        foreach(var building in m_buildings)
        {
            Vector3 buildingPos = building.GetPos();
            Vector3 buildingSize = building.GetSize() - Vector3Int.one;

            Vector3 dir = buildingPos - pos;
            if(dir.x > 0)
            {
                if (dir.x < buildingSize.x)
                    dir.x = 0;
                else dir.x -= buildingSize.x;
            }
            if (dir.y > 0)
            {
                if (dir.y < buildingSize.y)
                    dir.y = 0;
                else dir.y -= buildingSize.y;
            }
            if (dir.z > 0)
            {
                if (dir.z < buildingSize.z)
                    dir.z = 0;
                else dir.z -= buildingSize.z;
            }

            float dist = dir.sqrMagnitude;

            if(dist < bestDistance || bestBuilding == null)
            {
                bestBuilding = building;
                bestDistance = dist;
            }
        }

        return bestBuilding;
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, BuildingType type)
    {
        float bestDistance = 0;
        BuildingBase bestBuilding = null;

        foreach (var building in m_buildings)
        {
            if (building.GetBuildingType() != type)
                continue;

            Vector3 buildingPos = building.GetPos();
            Vector3 buildingSize = building.GetSize();

            Vector3 dir = buildingPos - pos;
            if (dir.x > 0)
            {
                if (dir.x < buildingSize.x)
                    dir.x = 0;
                else dir.x -= buildingSize.x;
            }
            if (dir.y > 0)
            {
                if (dir.y < buildingSize.y)
                    dir.y = 0;
                else dir.y -= buildingSize.y;
            }
            if (dir.z > 0)
            {
                if (dir.z < buildingSize.z)
                    dir.z = 0;
                else dir.z -= buildingSize.z;
            }

            float dist = dir.sqrMagnitude;

            if (dist < bestDistance || bestBuilding == null)
            {
                bestBuilding = building;
                bestDistance = dist;
            }
        }

        return bestBuilding;
    }

    public BuildingBase GetBuildingAt(Vector3Int pos)
    {
        foreach(var b in m_buildings)
        {
            var bounds = b.GetBounds();
            var min = bounds.min;
            var max = bounds.max;
            if (pos.x >= min.x && pos.x < max.x && pos.y >= min.y && pos.y < max.y && pos.z >= min.z && pos.z < max.z)
                return b;
        }
        return null;
    }
}
