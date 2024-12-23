using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityWeaponGun : EntityWeaponBase
{
    [SerializeField] GameObject m_projectilePrefab;
    [SerializeField] GameObject m_firePrefab;
    [SerializeField] float m_rangeStopMove;
    [SerializeField] float m_fireRange;
    [SerializeField] float m_fireRate;

    List<Transform> m_firePoints = new List<Transform>();

    float m_fireTimer;
    int m_fireIndex;

    BuildingBase m_towerTarget;
    BuildingBase m_target;

    TurretBehaviour m_turret;

    private void Start()
    {
        m_turret = GetComponent<TurretBehaviour>();

        m_firePoints.Clear();
        GetFirePoints(transform);
    }

    void GetFirePoints(Transform transform)
    {
        int nbChild = transform.childCount;
        for(int i = 0; i < nbChild; i++)
        {
            var child = transform.GetChild(i);
            if (child.name == "FirePoint")
                m_firePoints.Add(child);

            GetFirePoints(child);
        }
    }

    public override float GetMoveDistance()
    {
        return m_rangeStopMove;
    }

    public override GameObject GetTarget()
    {
        if (m_target == null)
        {
            if (m_towerTarget == null)
                return null;
            return m_towerTarget.gameObject;
        }
        return m_target.gameObject;
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        UpdateTarget();
        UpdateTurret();
    }

    void UpdateTarget()
    {
        GetTeamEvent team = new GetTeamEvent();
        Event<GetTeamEvent>.Broadcast(team, gameObject);
        Team targetTeam = TeamEx.GetOppositeTeam(team.team);

        if (BuildingList.instance == null)
            return;

        if(m_towerTarget == null)
            m_towerTarget = GetTower();
        if(m_target == null)
            m_target = GetNearestBuildingAtRange(m_fireRange, targetTeam);
    }

    void UpdateTurret()
    {
        if(m_turret != null)
        {
            if (!IsTargetAtRange())
                m_turret.SetNoTarget();
            else m_turret.SetTarget(GetTargetPos());
        }

        float rateTimer = 1 / m_fireRate;
        m_fireTimer += Time.deltaTime;
        if(IsTargetAtRange() && (m_turret == null || m_turret.CanFire()))
        {
            while (m_fireTimer >= rateTimer)
            {
                m_fireTimer -= rateTimer;
                Fire();
            }
        }
        else if (m_fireTimer > rateTimer)
            m_fireTimer = rateTimer;
    }

    void Fire()
    {
        if (m_firePoints.Count == 0)
            return;

        if (m_fireIndex < 0 || m_fireIndex >= m_firePoints.Count)
            m_fireIndex = 0;

        Transform firePos = m_firePoints[m_fireIndex];

        if(m_firePrefab != null)
        {
            var obj = Instantiate(m_firePrefab);
            obj.transform.position = firePos.position;
            obj.transform.rotation = firePos.rotation;
        }

        if(m_projectilePrefab != null)
        {
            var obj = Instantiate(m_projectilePrefab);
            obj.transform.position = firePos.position;
            obj.transform.rotation = firePos.rotation;

            var projectile = obj.GetComponent<ProjectileBase>();
            if(projectile != null)
            {
                var target = GetTarget();
                if (target != null)
                {
                    projectile.SetTarget(target);
                    projectile.SetCaster(gameObject);
                    //multipliers & others stuffs
                }
            }
        }

        m_fireIndex++;
    }

    bool IsTargetAtRange()
    {
        var target = m_target == null ? m_towerTarget : m_target;
        if (target == null)
            return false;

        Vector3 pos = target.GetGroundCenter();
        float dist = VectorEx.SqrMagnitudeXZ(pos - transform.position);

        return dist < m_fireRange * m_fireRange;
    }

    Vector3 GetTargetPos()
    {
        var target = m_target == null ? m_towerTarget : m_target;
        if(target == null)
            return transform.position + transform.forward;

        Vector3 pos = target.GetGroundCenter();

        var type = target.GetBuildingType();
        var data = Global.instance.buildingDatas.GetBuilding(type);
        if (data != null)
            pos.y += data.size.y / 2.0f;

        return pos;
    }
}
