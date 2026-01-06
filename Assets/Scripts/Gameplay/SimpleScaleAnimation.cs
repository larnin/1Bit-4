using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SimpleScaleAnimation : MonoBehaviour
{
    const string m_colorName = "_RimColor";

    [SerializeField] float m_duration = 1;
    [SerializeField] float m_radius = 3;
    [SerializeField] Ease m_curve = Ease.Linear;
    [SerializeField] float m_fadeEndPercent = 0.8f;

    float m_time = 0;

    Color m_explosionInitialColor;
    Renderer m_explosionRenderer;
    Material m_explosionMaterial;

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
    }

    private void Update()
    {
        m_time += Time.deltaTime;
        if (m_time >= m_duration)
            Destroy(gameObject);

        UpdateRender();
    }

    void UpdateRender()
    {
        float normTime = m_time / m_duration;
        if (normTime > 1)
            normTime = 1;

        float radius = DOVirtual.EasedValue(0, m_radius, normTime, m_curve);
        transform.localScale = Vector3.one * radius;

        Color c = m_explosionInitialColor;

        if (normTime > m_fadeEndPercent)
        {
            float percent = (normTime - m_fadeEndPercent) / (1 - m_fadeEndPercent);
            c.a = 1 - percent;
            m_explosionMaterial.SetColor(m_colorName, c);
            m_explosionRenderer.material = m_explosionMaterial;
        }
    }
}
