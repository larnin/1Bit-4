using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum Team
{
    Player,
    Ennemy,
    Neutral
}

public static class TeamEx
{
    public static Team GetOppositeTeam(Team team)
    {
        if (team == Team.Player)
            return Team.Ennemy;
        if (team == Team.Ennemy)
            return Team.Player;
        return Team.Neutral;
    }
}

[Serializable]
public class OneResourceCost
{
    public ResourceType type;
    public float count;
}

[Serializable]
public class ResourceCost
{
    public List<OneResourceCost> cost;

    public bool HaveMoney()
    {
        if (ResourceSystem.instance == null)
            return false;

        foreach(var r in cost)
        {
            if (!ResourceSystem.instance.HaveResource(r.type))
                return false;

            float count = ResourceSystem.instance.GetResourceStored(r.type);
            if (count < r.count)
                return false;
        }

        return true;
    }

    public void ConsumeCost()
    {
        if (ResourceSystem.instance == null)
            return;

        foreach (var r in cost)
        {
            if (!ResourceSystem.instance.HaveResource(r.type))
                continue;

            ResourceSystem.instance.RemoveResource(r.type, r.count, false);
        }
    }
}

[Serializable]
public class OneBuildingData
{
    public BuildingType type;
    public GameObject prefab;
    public Sprite sprite;
    public Sprite spriteWithBorder;
    public Vector3Int size = Vector3Int.one;
    public Team team = Team.Player;
    public string name;
    [Multiline]
    public string description;
    public ResourceCost cost;
}

[Serializable]
public class BuildingsDatas
{
    [SerializeField] List<OneBuildingData> m_buildings;
    public GameObject mineItemPrefab;
    public float lifeLossDisplayDuration = 10;

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
