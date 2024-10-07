using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityWeaponGun : EntityWeaponBase
{
    enum TurretState
    {
        NoTarget,
        MovingToTarget,
        Target,
        MovingToDefault,
    }

    [SerializeField] GameObject m_projectilePrefab;
    [SerializeField] GameObject m_firePrefab;
    [SerializeField] float m_rangeStopMove;
    [SerializeField] float m_fireRange;
    [SerializeField] float m_fireRate;
    [SerializeField] float m_turretRotSpeed;

    List<Transform> m_firePoints = new List<Transform>();
    Transform m_turretPivot;
    Quaternion m_turretInitialRotation;
    Quaternion m_turretStartRotation;
    TurretState m_turretState;
    float m_turretTimer;
    float m_turretTimerMax;

    float m_fireTimer;
    int m_fireIndex;

    BuildingBase m_towerTarget;
    BuildingBase m_target;

    private void Start()
    {
        m_turretPivot = transform.Find("Pivot");

        Transform initialPoint = m_turretPivot == null ? transform : m_turretPivot;
        m_firePoints.Clear();
        GetFirePoints(initialPoint);
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
        UpdateTarget();
        UpdateTurret();
    }

    void UpdateTarget()
    {
        if (BuildingList.instance == null)
            return;

        if(m_towerTarget == null)
            m_towerTarget = GetTower();
        if(m_target == null)
            m_target = GetNearestBuildingAtRange(m_fireRange);
    }

    void UpdateTurret()
    {
        if (m_turretPivot == null)
        {
            m_turretState = TurretState.Target;
        }
        else
        {
            switch (m_turretState)
            {
                case TurretState.NoTarget:
                    {
                        if(IsTargetAtRange())
                        {
                            m_turretStartRotation = m_turretPivot.rotation;

                            var targetPos = GetTargetPos();
                            var forward = (targetPos - m_turretPivot.position).normalized;

                            Quaternion targetAngle = Quaternion.LookRotation(forward, Vector3.up);
                            float angle = Mathf.Abs(Quaternion.Angle(m_turretStartRotation, m_turretInitialRotation));
                            m_turretTimer = 0;
                            m_turretTimerMax = angle / m_turretRotSpeed;

                            m_turretState = TurretState.MovingToTarget;
                        }

                        break;
                    }
                case TurretState.MovingToTarget:
                    {
                        if(!IsTargetAtRange())
                        {
                            m_turretStartRotation = m_turretPivot.localRotation;
                            float angle = Mathf.Abs(Quaternion.Angle(m_turretStartRotation, m_turretInitialRotation));
                            m_turretTimer = 0;
                            m_turretTimerMax = angle / m_turretRotSpeed;

                            m_turretState = TurretState.MovingToDefault;
                            break;
                        }

                        m_turretTimer += Time.deltaTime;
                        if(m_turretTimer >= m_turretTimerMax)
                        {
                            m_turretTimer = m_turretTimerMax;
                            m_turretState = TurretState.Target;
                        }

                        float normTime = m_turretTimer / m_turretTimerMax;

                        var targetPos = GetTargetPos();
                        var forward = (targetPos - m_turretPivot.position).normalized;
                        Quaternion targetAngle = Quaternion.LookRotation(forward, Vector3.up);

                        transform.rotation = Quaternion.Lerp(m_turretStartRotation, targetAngle, normTime);

                        break;
                    }
                case TurretState.Target:
                    {
                        if (!IsTargetAtRange())
                        {
                            m_turretStartRotation = m_turretPivot.localRotation;
                            float angle = Mathf.Abs(Quaternion.Angle(m_turretStartRotation, m_turretInitialRotation));
                            m_turretTimer = 0;
                            m_turretTimerMax = angle / m_turretRotSpeed;

                            m_turretState = TurretState.MovingToDefault;
                        }

                        break;
                    }
                case TurretState.MovingToDefault:
                    {
                        if(IsTargetAtRange())
                        {
                            m_turretState = TurretState.NoTarget;
                            break;
                        }

                        m_turretTimer += Time.deltaTime;
                        if (m_turretTimer >= m_turretTimerMax)
                        {
                            m_turretTimer = m_turretTimerMax;
                            m_turretState = TurretState.NoTarget;
                        }

                        float normTime = m_turretTimer / m_turretTimerMax;

                        transform.localRotation = Quaternion.Lerp(m_turretStartRotation, m_turretInitialRotation, normTime);

                        break;
                    }
            }
        }

        if(m_turretState == TurretState.Target)
        {
            var targetPos = GetTargetPos();
            m_turretPivot.LookAt(targetPos, Vector3.up);

            float rateTimer = 1 / m_fireRate;
            m_fireTimer += Time.deltaTime;
            while (m_fireTimer >= rateTimer)
            {
                m_fireTimer -= rateTimer;
                Fire();
            }
        }
        else
        {
            float rateTimer = 1 / m_fireRate;
            m_fireTimer += Time.deltaTime;
            if (m_fireTimer > rateTimer)
                m_fireTimer = rateTimer;
        }
    }

    void Fire()
    {
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
            //todo projectile stuff
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
