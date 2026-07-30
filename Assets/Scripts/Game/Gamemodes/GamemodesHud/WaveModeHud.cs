using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class WaveModeHud : MonoBehaviour
{
    TMP_Text m_labelText;
    TMP_Text m_valueText;

    private void Awake()
    {
        var label = transform.Find("Label");
        if (label != null)
            m_labelText = label.GetComponent<TMP_Text>();

        var value = transform.Find("Value");
        if (value != null)
            m_valueText = value.GetComponent<TMP_Text>();
    }

    public void SetWave(int index, int total)
    {
        if (m_labelText == null)
            return;

        m_labelText.text = "Wave " + index.ToString() + "/" + total.ToString();
    }

    public void SetSpawning()
    {
        if (m_valueText == null)
            return;

        m_valueText.text = "Spawning ...";
    }

    public void SetWaitingTime(float time)
    {
        if (m_valueText == null)
            return;

        string timer = Utility.FormateTime(time, true);

        m_valueText.text = "Waiting " + timer;
    }
}
