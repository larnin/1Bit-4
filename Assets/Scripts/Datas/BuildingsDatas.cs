using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class OneBuildingData
{
    public BuildingType type;
    public GameObject prefab;
}

[Serializable]
public class BuildingsDatas
{
    [SerializeField] List<OneBuildingData> m_buildings;

    public GameObject GetPrefab(BuildingType type)
    {
        var b = GetBuilding(type);
        if (b == null)
            return null;
        return b.prefab;
    }

    OneBuildingData GetBuilding(BuildingType type)
    {
        foreach(var b in m_buildings)
        {
            if (b.type == type)
                return b;
        }

        return null;
    }
}
