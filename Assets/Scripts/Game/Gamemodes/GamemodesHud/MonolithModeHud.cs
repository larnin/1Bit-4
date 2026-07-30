using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MonolithModeHud : MonoBehaviour
{
    TMP_Text m_percentText;
    RectTransform m_fillBack;
    RectTransform m_fill;

    private void Awake()
    {
        var percent = transform.Find("Value");
        if (percent != null)
            m_percentText = percent.GetComponent<TMP_Text>();

        var fillBackObj = transform.Find("FillBack");
        if(fillBackObj != null)
        {
            m_fillBack = fillBackObj.GetComponent<RectTransform>();
            var fillObj = fillBackObj.Find("Fill");
            if (fillObj != null)
                m_fill = fillObj.GetComponent<RectTransform>();
        }    
    }

    public void SetDisabled()
    {
        gameObject.SetActive(false);
    }

    public void SetStatus(float time, float totalTime)
    {
        gameObject.SetActive(true);

        float percent = time / totalTime;

        int percentI = Mathf.FloorToInt(percent * 100);
        m_percentText.text = percentI.ToString() + "%";

        m_fill.anchorMax = new Vector2(percent, m_fill.anchorMax.y);
    }
}