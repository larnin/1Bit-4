using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityWeaponAutoDestroy : EntityWeaponBase
{
    [SerializeField] ProjectileChoice m_projectileType;
    [SerializeField] float m_rangeAttack;
    [SerializeField] float m_detectRange;

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
            return;

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
            Attack();
    }

    void Attack()
    {
        var startInfos = new ProjectileStartInfos();
        startInfos.name = m_projectileType.GetValue();
        startInfos.caster = gameObject;
        startInfos.target = GetTarget();
        startInfos.position = transform.position;
        startInfos.rotation = transform.rotation;

        ProjectileBase.ThrowProjectile(startInfos);

        Event<KillEvent>.Broadcast(new KillEvent(), gameObject);
    }
}
