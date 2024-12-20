using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnnemyBehaviour : MonoBehaviour
{
    EntityWeaponBase m_weapon;
    EntityMove m_move;

    GameObject m_target;
    BuildingBase m_buildingTarget;

    private void Start()
    {
        m_weapon = GetComponent<EntityWeaponBase>();
        m_move = GetComponent<EntityMove>();
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (m_weapon == null || m_move == null)
            return;

        var target = m_weapon.GetTarget();

        if (target != m_target)
        {
            if (target != null)
            {
                Vector3 targetPos;
                m_buildingTarget = target.GetComponent<BuildingBase>();
                if (m_buildingTarget != null)
                    targetPos = m_buildingTarget.GetGroundCenter();
                else targetPos = target.transform.position;

                m_move.SetTarget(targetPos);
            }
            else m_move.Stop();

            m_target = target;
        }

        if (m_target != null)
        {
            float range = m_weapon.GetMoveDistance();
            Vector3 realTargetPos;
            if (m_buildingTarget != null)
                realTargetPos = m_buildingTarget.GetGroundCenter();
            else realTargetPos = target.transform.position;

            float distance = (realTargetPos - transform.position).sqrMagnitude;
            if(distance < range * range)
            {
                if (m_move.IsMoving())
                    m_move.Stop();
            }
            else if(!m_move.IsMoving())
                m_move.SetTarget(realTargetPos);
        }
    }
}

