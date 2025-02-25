using DG.Tweening;
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
public class OneDestructedBuildingData
{
    public Vector2Int size;
    public GameObject prefab;
    public GameObject particlePrefab;
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
    public bool firstFree;
    public ResourceCost cost;

    public bool IsFree()
    {
        if (cost.cost.Count == 0)
            return true;

        if (!firstFree)
            return false;

        if (BuildingList.instance == null)
            return true;

        int nb = BuildingList.instance.GetAllBuilding(type).Count;
        return nb == 0;
    }
}

[Serializable]
public class BuildingDestructionData
{
    public float displayDuration = 10;
    public float appearDuration = 1;
    public float hideDuration = 2;
    public float hideDistance = 2;
    public Ease hideCurve;
    public float DestructionAcceleration = 2;
    public float DestructionSpeed = 4;
}

[Serializable]
public class BuildingsDatas
{
    [SerializeField] List<OneBuildingData> m_buildings;
    public GameObject mineItemPrefab;
    public float lifeLossDisplayDuration = 10;
    public GameObject lifebarPrefab;
    [SerializeField] List<OneDestructedBuildingData> m_destructedBuildings;
    public BuildingDestructionData destructionDatas;
    public float regenSpeed;
    public float regenDelay;
    public string alarmSound;
    public float alarmVolume = 1;
    public float alarmRestartDelay = 30;
    public float alarmGlobalRestartDelay = 10;
    public float multiplePlaceReduction = 2;

    public OneBuildingData GetBuilding(BuildingType type)
    {
        foreach(var b in m_buildings)
        {
            if (b.type == type)
                return b;
        }

        return null;
    }

    public OneDestructedBuildingData GetDestructedBuildingDatas(Vector2Int size)
    {
        foreach(var b in m_destructedBuildings)
        {
            if (b.size == size)
                return b;
        }

        return null;
    }

    public float GetRealPlaceRadius(float left, float right)
    {
        if (left <= 0 || right <= 0)
            return left + right;

        float total = left + right - multiplePlaceReduction;
        if (total < left || total < right)
            return Mathf.Max(left, right);

        return total;
    }
}
