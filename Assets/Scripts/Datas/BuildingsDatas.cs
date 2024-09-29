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
    public Sprite sprite;
    public Sprite spriteWithBorder;
    public Vector3Int size = Vector3Int.one;
}

[Serializable]
public class BuildingsDatas
{
    [SerializeField] List<OneBuildingData> m_buildings;

    public OneBuildingData GetBuilding(BuildingType type)
    {
        foreach(var b in m_buildings)
        {
            if (b.type == type)
                return b;
        }

        return null;
    }
}
