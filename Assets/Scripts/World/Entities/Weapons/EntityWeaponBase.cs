using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

public class ProjectileStartInfos
{
    public string name;
    public GameObject caster;
    public GameObject target;
    public Vector3 position;
    public Quaternion rotation;
}

public abstract class EntityWeaponBase : MonoBehaviour
{
    static readonly ProfilerMarker ms_profilerMarker = new ProfilerMarker(ProfilerCategory.Scripts, "EntityWeaponBase.GetNearestBuildingAtRange");

    const float updateTargetDelay = 0.2f;
    float m_updateTargetTimer = 0;

    EnemyBehaviourV2 m_behaviour;
    GameObject m_target;

    protected virtual void Start()
    {
        m_behaviour = GetComponentInParent<EnemyBehaviourV2>();
    }

    public GameObject GetTarget()
    {
        return m_target;
    }

    public Vector3 GetTargetPos()
    {
        var target = GetTarget();
        if (target == null)
            return transform.position + transform.forward;

        return TurretBehaviour.GetTargetCenter(target);
    }

    public abstract float GetMoveDistance();

    public void UpdateTarget(float range)
    {
        if (m_behaviour == null)
        {
            m_target = null;
            return;
        }

        m_updateTargetTimer += Time.deltaTime; 
        if (m_updateTargetTimer < updateTargetDelay)
            return;
        m_updateTargetTimer = 0;

        var target = m_behaviour.GetTarget();
        if(target == null)
        {
            m_target = null;
            return;
        }

        if(Event<IsDeadEvent>.Broadcast(new IsDeadEvent(), target).isDead)
        {
            m_target = null;
            return;
        }

        Vector3 targetPos = TurretBehaviour.GetTargetCenter(target);
        float sqrDist = (transform.position - targetPos).sqrMagnitude;
        if(sqrDist > range * range)
        {
            m_target = null;
            return;
        }

        m_target = target;
    }
}

