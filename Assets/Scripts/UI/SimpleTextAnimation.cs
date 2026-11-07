using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class SimpleTextAnimation : MonoBehaviour
{
    [SerializeField] float m_appearDuration = 0.5f;
    [SerializeField] float m_scaleStart = 1;
    [SerializeField] float m_scaleEnd = 2;
    [SerializeField] float m_horizontalScaleAmplification = 0.5f;
    [SerializeField] float m_duration = 2;
    [SerializeField] float m_delay = 0;

    TMP_Text m_text;
    float m_time = 0;

    private void Awake()
    {
        m_text = GetComponent<TMP_Text>();
        if(m_text != null)
            m_text.color = new Color(1, 1, 1, 0);
        m_time = -m_delay;
    }

    private void Update()
    {
        if (m_text == null)
            return;

        m_time += Time.deltaTime;

        float normTime = Mathf.Clamp01(m_time / m_duration);
        float scale = m_scaleStart * (1 - normTime) + m_scaleEnd * normTime;
        float horizontalScaleMultiplier = 1 + m_horizontalScaleAmplification * normTime;

        float intensity = 1;
        if (m_time < m_appearDuration)
            intensity = m_time / m_appearDuration;
        if (m_time > m_duration - m_appearDuration)
            intensity = (m_duration - m_time) / m_appearDuration;
        intensity = Mathf.Clamp01(intensity);

        transform.localScale = new Vector3(scale * horizontalScaleMultiplier, scale, scale);
        m_text.color = new Color(1, 1, 1, intensity);
    }

    public void Restart()
    {
        m_time = -m_delay;
    }
}
