using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ProjectileSimpleExplosion : ProjectileBase
{
    const string m_colorName = "_RimColor";

    [SerializeField] LayerMask m_explosionLayer;
    [SerializeField] float m_explosionDuration = 1;
    [SerializeField] float m_explosionRadius = 3;
    [SerializeField] Ease m_explosionCurve = Ease.Linear;
    [SerializeField] float m_explosionFadeEndPercent = 0.8f;
    [SerializeField] string m_explosionSound;
    [SerializeField] float m_explosionSoundVolume = 1;
    [SerializeField] bool m_hitAll = false;
    [SerializeField] float m_delayBeforeFadeout = 1;

    float m_time = 0;

    Renderer m_explosionRenderer;
    Material m_explosionMaterial;
    Color m_explosionInitialColor;
    List<GameObject> m_hitEntities = new List<GameObject>();

    bool m_explosionEnded = false;
    bool m_soundPlayed = false;

    private void Awake()
    {
        m_explosionRenderer = GetComponentInChildren<Renderer>();
        if (m_explosionRenderer != null)
        {
            m_explosionMaterial = m_explosionRenderer.material;
            if (m_explosionMaterial != null)
            {
                m_explosionInitialColor = m_explosionMaterial.GetColor(m_colorName);
                m_explosionInitialColor.a = 1;
                m_explosionMaterial.SetColor(m_colorName, m_explosionInitialColor);
            }
        }

        UpdateExplosionRender();
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if(!m_soundPlayed)
        {
            if (SoundSystem.instance != null)
            {
                SoundSystem.instance.PlaySound(m_explosionSound, transform.position, m_explosionSoundVolume);
            }
            m_soundPlayed = true;
        }

        m_time += Time.deltaTime;
        if (m_time >= m_explosionDuration)
        {
            if(!m_explosionEnded)
            {
                m_explosionRenderer.enabled = false;
            }
            m_explosionEnded = true;

            if (m_time >= m_explosionDuration + m_delayBeforeFadeout)
                Destroy(gameObject);
            return;
        }

        float radius = DOVirtual.EasedValue(0, m_explosionRadius, m_time / m_explosionDuration, m_explosionCurve);

        var cols = Physics.OverlapSphere(transform.position, radius / 2, m_explosionLayer);
        foreach (var col in cols)
        {
            if (m_hitEntities.Contains(col.gameObject))
                continue;
            Event<HitEvent>.Broadcast(new HitEvent(new Hit(m_damages * m_damagesMultiplier, m_hitAll ? null : m_caster, m_damageType, m_damageEffect)), col.gameObject);
            m_hitEntities.Add(col.gameObject);
        }

        UpdateExplosionRender();
    }

    void UpdateExplosionRender()
    {
        float normTime = m_time / m_explosionDuration;

        float radius = DOVirtual.EasedValue(0, m_explosionRadius, normTime, m_explosionCurve);
        transform.localScale = Vector3.one * radius;

        Color c = m_explosionInitialColor;

        if (normTime > m_explosionFadeEndPercent)
        {
            float percent = (normTime - m_explosionFadeEndPercent) / (1 - m_explosionFadeEndPercent);
            c.a = 1 - percent;
            m_explosionMaterial.SetColor(m_colorName, c);
            m_explosionRenderer.material = m_explosionMaterial;
        }
    }
}
