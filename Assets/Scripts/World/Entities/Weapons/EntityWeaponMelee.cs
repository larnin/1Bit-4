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

    float m_hitTimer;

    public override float GetMoveDistance()
    {
        return m_rangeAttack;
    }
    
    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        if (Utility.IsDead(gameObject))
            return;

        UpdateTarget(m_detectRange);
        UpdateHit();
    }

    void UpdateHit()
    {
        var target = GetTarget();
        if (target == null)
        {
            m_hitTimer = 0;
            return;
        }
        
        Vector3Int size = Vector3Int.one;
        var pos = transform.position;

        var targetType = GameSystem.GetEntityType(target);
        if (targetType == EntityType.Building)
        {
            var building = target.GetComponent<BuildingBase>();
            if (building != null)
            {
                var center = building.GetGroundCenter();
                size = building.GetSize();
                center.y += size.y / 2.0f;

                pos = center;
            }
        }

        var dist = (pos - transform.position).MagnitudeXZ();
        
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
        var multiplier = new GetStatEvent(StatType.DamagesMultiplier);
        Event<GetStatEvent>.Broadcast(multiplier, gameObject);

        var pos = transform.position + transform.forward * m_hitOffset;

        var cols = Physics.OverlapSphere(pos, m_hitRadius, m_hitLayer.value);

        var hit = new Hit(m_damages * multiplier.GetValue(), gameObject, m_damageType, m_damageEffect);

        foreach (var col in cols)
            Event<HitEvent>.Broadcast(new HitEvent(hit), col.gameObject);
    }
}
