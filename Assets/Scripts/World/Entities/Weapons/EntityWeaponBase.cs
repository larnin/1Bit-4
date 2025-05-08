using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

public abstract class EntityWeaponBase : MonoBehaviour
{
    static readonly ProfilerMarker ms_profilerMarker = new ProfilerMarker(ProfilerCategory.Scripts, "EntityWeaponBase.GetNearestBuildingAtRange");

    const float updateTargetDelay = 0.2f;

    BuildingBase m_towerTarget;
    BuildingBase m_target;
    float m_updateTargetTimer = 0;

    public GameObject GetTarget()
    {
        if (m_target == null)
        {
            if (m_towerTarget == null)
                return null;
            return m_towerTarget.gameObject;
        }
        return m_target.gameObject;
    }

    public Vector3 GetTargetPos()
    {
        var target = GetTarget();
        if (target == null)
            return transform.position + transform.forward;

        return TurretBehaviour.GetTargetCenter(target.gameObject);
    }

    public abstract float GetMoveDistance();

    protected BuildingBase GetTower()
    {
        if (BuildingList.instance == null)
            return null;

        return BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
    }

    protected BuildingBase GetNearestBuildingAtRange(float range, Team team)
    {
        using (ms_profilerMarker.Auto())
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

    protected void UpdateTarget(float range)
    {
        m_updateTargetTimer -= Time.deltaTime;
        if (m_updateTargetTimer <= 0 || m_target == null)
        {
            m_updateTargetTimer = updateTargetDelay;

            var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), gameObject);
            Team targetTeam = TeamEx.GetOppositeTeam(team.team);

            if (BuildingList.instance == null)
                return;

            if (m_target != null)
            {
                if (Utility.IsDead(m_target.gameObject))
                    m_target = null;
            }

            if (m_towerTarget == null)
                m_towerTarget = GetTower();
            if (m_target == null)
                m_target = GetNearestBuildingAtRange(range, targetTeam);
        }
    }
}

