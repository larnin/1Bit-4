using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingDeathParticles : MonoBehaviour
{
    float m_duration = -1;
    float m_timer;
    bool m_stopped = false;

    ParticleSystem m_particles;

    private void Awake()
    {
        m_particles = GetComponent<ParticleSystem>();
        if (m_particles != null)
            m_particles.Play();
    }

    public void SetDuration(float duration)
    {
        m_duration = duration;
    }

    private void Update()
    {
        if (m_duration < 0)
            return;

        if (GameInfos.instance.paused)
            return;

        m_timer += Time.deltaTime;

        if(!m_stopped && m_timer >= m_duration)
        {
            m_stopped = true;
            if(m_particles != null)
                m_particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (m_timer > m_duration + 2)
            Destroy(gameObject);
    }
}
