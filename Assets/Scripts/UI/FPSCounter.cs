using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    TMP_Text m_text;

    float lastFPS = 0;

    private void Awake()
    {
        m_text = GetComponent<TMP_Text>();
        if (m_text == null)
            Destroy(gameObject);
    }

    private void Update()
    {
        float fps = 1 / Time.deltaTime;

        if (lastFPS == 0)
            lastFPS = fps;

        const float percent = 0.1f;

        lastFPS = lastFPS * (1 - percent) + fps * percent;

        m_text.text = Mathf.RoundToInt(lastFPS).ToString();
    }
}
