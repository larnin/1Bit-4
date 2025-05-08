using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectileSimple : ProjectileBase
{
    [SerializeField] float m_speed = 5;
    [SerializeField] float m_maxLife = 5;
    [SerializeField] LayerMask m_hitLayer;
    [SerializeField] LayerMask m_groundLayer;
    [SerializeField] GameObject m_hitPrefab;

    float m_time = 0;

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        var dir = transform.forward;

        var nextPos = transform.position + dir * Time.deltaTime * m_speed;
        var ray = new Ray(transform.position, dir);

        RaycastHit hit;
        var haveHit = Physics.Raycast(ray, out hit, Time.deltaTime * m_speed + 0.01f, m_hitLayer);
        if (haveHit)
            OnHit(hit);
        transform.position = nextPos;

        m_time += Time.deltaTime;
        if (m_time > m_maxLife)
            Destroy(gameObject);
    }

    void OnHit(RaycastHit hit)
    {
        bool needDestroy = false;
        
        var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), hit.collider.gameObject);
        if (team.team == TeamEx.GetOppositeTeam(m_casterTeam))
        {
            Event<HitEvent>.Broadcast(new HitEvent(new Hit(m_damages * m_damagesMultiplier, m_caster, m_damageType, m_damageEffect)), hit.collider.gameObject);

            needDestroy = true;
        }
        if ((m_groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            needDestroy = true;

        if(needDestroy)
        {
            if (m_hitPrefab != null)
            {
                var obj = Instantiate(m_hitPrefab);
                obj.transform.position = hit.point;
                obj.transform.forward = hit.normal;
            }

            Destroy(gameObject);
        }
    }
}
