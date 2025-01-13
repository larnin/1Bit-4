using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityWeaponMelee : EntityWeaponBase
{
    [SerializeField] float m_rangeAttack;
    [SerializeField] float m_detectRange;
    [SerializeField] float m_attackDelay;
    [SerializeField] float m_hitRadius;
    [SerializeField] float m_hitOffset;
    [SerializeField] float m_damages = 1;
    [SerializeField] DamageType m_damageType = DamageType.Normal;
    [SerializeField] float m_damageEffect = 1;
    [SerializeField] LayerMask m_hitLayer;

    BuildingBase m_towerTarget;
    BuildingBase m_target;

    float m_hitTimer;

    public override float GetMoveDistance()
    {
        return m_rangeAttack;
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

        if (Utility.IsDead(gameObject))
            return;

        UpdateTarget();
        UpdateHit();
    }

    void UpdateTarget()
    {
        GetTeamEvent team = new GetTeamEvent();
        Event<GetTeamEvent>.Broadcast(team, gameObject);
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
            m_target = GetNearestBuildingAtRange(m_detectRange, targetTeam);
    }

    void UpdateHit()
    {
        var target = m_target == null ? m_towerTarget : m_target;
        if (target == null)
        {
            m_hitTimer = 0;
            return;
        }

        var pos = target.GetGroundCenter();

        var dist = (pos - transform.position).MagnitudeXZ();

        var size = target.GetSize();
        dist -= Mathf.Max(size.x, size.z);

        if (dist <= m_rangeAttack)
        {
            m_hitTimer += Time.deltaTime;
            if(m_hitTimer >= m_attackDelay)
            {
                m_hitTimer = 0;
                Attack();
            }
        }
        else m_hitTimer = 0;
    }

    void Attack()
    {
        var pos = transform.position + transform.forward * m_hitOffset;

        var cols = Physics.OverlapSphere(pos, m_hitRadius, m_hitLayer.value);

        var hit = new Hit(m_damages, gameObject, m_damageType, m_damageEffect);

        foreach (var col in cols)
            Event<HitEvent>.Broadcast(new HitEvent(hit), col.gameObject);
    }
}
