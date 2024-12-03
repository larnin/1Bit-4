using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingList : MonoBehaviour
{
    List<BuildingBase> m_buildings = new List<BuildingBase>();
    Dictionary<ulong, BuildingBase> m_buildingsPos = new Dictionary<ulong, BuildingBase>();

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

        var bounds = building.GetBounds();
        var min = bounds.min;
        var max = bounds.max;

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    m_buildingsPos.Add(Utility.PosToID(new Vector3Int(x, y, z)), building);
                }
            }
        }

        if (ConnexionSystem.instance != null)
            ConnexionSystem.instance.OnBuildingChange();
    }

    public void UnRegister(BuildingBase building)
    {
        m_buildings.Remove(building);

        var bounds = building.GetBounds();
        var min = bounds.min;
        var max = bounds.max;

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    m_buildingsPos.Remove(Utility.PosToID(new Vector3Int(x, y, z)));
                }
            }
        }

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

    List<BuildingBase> GetAllBuilding(Func<BuildingBase, bool> condition)
    {
        List<BuildingBase> buildings = new List<BuildingBase>();

        foreach (var building in m_buildings)
        {
            if(condition != null && condition(building))
                buildings.Add(building);
        }

        return buildings;
    }

    public List<BuildingBase> GetAllBuilding(BuildingType type)
    {
        return GetAllBuilding(x => { return x.GetBuildingType() == type; });
    }

    public List<BuildingBase> GetAllBuilding(Team team)
    {
        return GetAllBuilding(x => { return x.GetTeam() == team; });
    }

    public List<BuildingBase> GetAllBuilding(BuildingType type,  Team team)
    {
        return GetAllBuilding(x => { return x.GetBuildingType() == type && x.GetTeam() == team; });
    }

    BuildingBase GetNearestBuilding(Vector3 pos, Func<BuildingBase, bool> condition)
    {
        float bestDistance = 0;
        BuildingBase bestBuilding = null;

        foreach (var building in m_buildings)
        {
            if (condition != null && !condition(building))
                continue;

            Vector3 buildingPos = building.GetPos();
            Vector3 buildingSize = building.GetSize();

            float dist = GetSqrDistance(pos, buildingPos, buildingSize);

            if (dist < bestDistance || bestBuilding == null)
            {
                bestBuilding = building;
                bestDistance = dist;
            }
        }

        return bestBuilding;
    }

    public BuildingBase GetNearestBuilding(Vector3 pos)
    {
        return GetNearestBuilding(pos, null);
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, BuildingType type)
    {
        return GetNearestBuilding(pos, x => { return x.GetBuildingType() == type; });
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, Team team)
    {
        return GetNearestBuilding(pos, x => { return x.GetTeam() == team; });
    }

    public BuildingBase GetNearestBuilding(Vector3 pos, BuildingType type, Team team)
    {
        return GetNearestBuilding(pos, x => { return x.GetBuildingType() == type && x.GetTeam() == team; });
    }

    public BuildingBase GetBuildingAt(Vector3Int pos)
    {
        BuildingBase b;
        if (!m_buildingsPos.TryGetValue(Utility.PosToID(pos), out b))
            return null;
        return b;
    }

    static float GetSqrDistance(Vector3 pos, Vector3 itemPos, Vector3 itemSize)
    {
        Vector3 dir = itemPos - pos;
        if (dir.x > 0)
        {
            if (dir.x < itemSize.x)
                dir.x = 0;
            else dir.x -= itemSize.x;
        }
        if (dir.y > 0)
        {
            if (dir.y < itemSize.y)
                dir.y = 0;
            else dir.y -= itemSize.y;
        }
        if (dir.z > 0)
        {
            if (dir.z < itemSize.z)
                dir.z = 0;
            else dir.z -= itemSize.z;
        }

        return  dir.sqrMagnitude;
    }
}
