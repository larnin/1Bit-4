using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class EntityWeaponBase : MonoBehaviour
{
    public abstract GameObject GetTarget();
    public abstract float GetMoveDistance();

    protected BuildingBase GetTower()
    {
        if (BuildingList.instance == null)
            return null;

        return BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
    }

    protected BuildingBase GetNearestBuildingAtRange(float range, Team team)
    {
        if (BuildingList.instance == null)
            return null;

        var building = BuildingList.instance.GetNearestBuilding(transform.position, team, AliveType.Alive);
        if (building == null)
            return null;

        float dist = (building.GetGroundCenter() - transform.position).sqrMagnitude;
        if (dist <= range * range)
            return building;
        return null;
    }
}

