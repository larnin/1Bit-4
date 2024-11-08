using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public enum UIElementFillValueDisplayType
{
    classic,
    percent,
    valueOnly,
    hidden,
}

public class UIElementFillValue : UIElementBase
{
    TMP_Text m_label;
    TMP_Text m_valueText;
    RectTransform m_transform;
    RectTransform m_valueTransform;
    RectTransform m_fillBackTransform;
    RectTransform m_fillTransform;

    UIElementFillValueDisplayType m_valueDisplayType = UIElementFillValueDisplayType.classic;
    float m_value = 0;
    float m_max = 1;
    int m_nbDigits = 3;

    Func<string> m_labelFunc;
    Func<float> m_valueFunc;
    Func<float> m_maxFunc;

    private void Awake()
    {
        m_transform = GetComponent<RectTransform>();

        var labelTr = transform.Find("Label");
        if (labelTr != null)
            m_label = labelTr.GetComponent<TMP_Text>();

        var valueTr = transform.Find("Value");
        if(valueTr != null)
        {
            m_valueText = valueTr.GetComponent<TMP_Text>();
            m_valueTransform = valueTr as RectTransform;
        }

        var fillBackTr = transform.Find("FillBack");
        if(fillBackTr != null)
        {
            m_fillBackTransform = fillBackTr as RectTransform;
            m_fillTransform = fillBackTr.Find("Fill") as RectTransform;
        }
    }

    private void Update()
    {
        if (m_labelFunc != null && m_label != null)
            m_label.text = m_labelFunc();

        if (m_valueFunc != null)
            m_value = m_valueFunc();
        if (m_maxFunc != null)
            m_max = m_maxFunc();

        if(m_valueDisplayType != UIElementFillValueDisplayType.hidden)
        {
            m_valueText.gameObject.SetActive(true);

            m_valueText.text = GetValueText();

            float width = m_valueText.renderedWidth + 5;
            float parentWidth = m_transform.rect.width;
            m_valueTransform.anchorMin = new Vector2(1 - (width / parentWidth), m_valueTransform.anchorMin.y);
            m_fillBackTransform.anchorMax = new Vector2(1 - (width / parentWidth), m_fillBackTransform.anchorMax.y);
        }
        else
        {
            m_valueText.gameObject.SetActive(false);
            m_fillBackTransform.anchorMax = new Vector2(1, m_fillBackTransform.anchorMax.y);
        }

        float fillPercent = m_value / m_max;
        fillPercent = Mathf.Clamp01(fillPercent);

        m_fillTransform.anchorMax = new Vector2(fillPercent, m_fillTransform.anchorMax.y);
    }

    string GetValueText()
    {
        string format = "#0.";
        for (int i = 0; i < m_nbDigits; i++)
            format += '0';

        switch(m_valueDisplayType)
        {
            case UIElementFillValueDisplayType.classic:
                return m_value.ToString(format) + '/' + m_max.ToString(format);
            case UIElementFillValueDisplayType.percent:
                return (m_value / m_max * 100).ToString(format) + '%';
            case UIElementFillValueDisplayType.valueOnly:
                return m_value.ToString(format);
            default:
                return "";
        }
    }

    public override float GetHeight()
    {
        return m_label.renderedHeight + m_valueText.renderedHeight;
    }

    public UIElementFillValue SetLabel(string label)
    {
        m_label.text = label;
        return this;
    }

    public UIElementFillValue SetLabelFunc(Func<string> labelFunc)
    {
        m_labelFunc = labelFunc;
        return this;
    }

    public UIElementFillValue SetValueDisplayType(UIElementFillValueDisplayType displayType)
    {
        m_valueDisplayType = displayType;
        return this;
    }

    public UIElementFillValue SetValue(float value)
    {
        m_value = value;
        return this;
    }

    public UIElementFillValue SetValueFunc(Func<float> valueFunc)
    {
        m_valueFunc = valueFunc;
        return this;
    }

    public UIElementFillValue SetMax(float max)
    {
        m_max = max;
        return this;
    }

    public UIElementFillValue SetMaxFunc(Func<float> maxFunc)
    {
        m_maxFunc = maxFunc;
        return this;
    }

    public UIElementFillValue SetNbDigits(int nbDigits)
    {
        m_nbDigits = nbDigits;
        return this;
    }
}
