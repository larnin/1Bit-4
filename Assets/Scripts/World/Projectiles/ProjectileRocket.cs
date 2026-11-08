using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectileRocket : ProjectileSimple
{
    [SerializeField] float m_rotationSpeed = 5;
    [SerializeField] float m_searchRadius = 10;

    Team m_targetTeam = Team.Neutral;

    protected override void Update()
    {
        if (m_target == null)
            SearchNewTarget();

        if(m_target != null)
        {
            if (m_targetTeam == Team.Neutral)
                m_targetTeam = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), m_target).team;

            var dir = transform.forward;
            var targetDir = (m_target.transform.position - transform.position).normalized;

            float angle = Vector3.Angle(dir, targetDir);
            if (angle > 0.01f)
            {
                float deltaAngle = m_rotationSpeed * Time.deltaTime;
                if (deltaAngle >= angle)
                    transform.forward = targetDir;
                else
                {
                    float anglePercent = deltaAngle / angle;
                    Vector3 newDir = Vector3.Slerp(dir, targetDir, anglePercent);
                    transform.forward = newDir;
                }
            }
        }


        base.Update();
    }

    void SearchNewTarget()
    {
        if (m_targetTeam == Team.Neutral)
            return;

        m_target = BuildingTurretBase.GetTarget(transform.position, m_searchRadius, m_targetTeam);
    }
}
