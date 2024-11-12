using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UIElementLabelAndText : UIElementBase
{
    TMP_Text m_label;
    RectTransform m_labelTransform;
    TMP_Text m_text;
    RectTransform m_textTransform;
    RectTransform m_transform;

    UIElementAlignment m_labelAlignment = UIElementAlignment.left;
    UIElementAlignment m_textAlignment = UIElementAlignment.right;
    bool m_forceTextMultiline;
    bool m_displayedTextMultiline;

    Func<string> m_labelFunc;
    Func<string> m_textFunc;

    private void Awake()
    {
        var labelTr = transform.Find("Label");
        if(labelTr != null)
        {
            m_label = labelTr.GetComponent<TMP_Text>();
            m_labelTransform = labelTr as RectTransform;
        }

        var textTr = transform.Find("Text");
        if(textTr != null)
        {
            m_text = textTr.GetComponent<TMP_Text>();
            m_textTransform = textTr as RectTransform;
        }

        m_transform = transform as RectTransform;
    }

    public override float GetHeight()
    {
        if (m_displayedTextMultiline)
            return m_label.renderedHeight + m_text.renderedHeight;
        return m_text.renderedHeight;
    }

    public UIElementLabelAndText SetLabel(string label)
    {
        m_label.text = label;
        return this;
    }

    public UIElementLabelAndText SetLabelFunc(Func<string> labelFunc)
    {
        m_labelFunc = labelFunc;
        return this;
    }

    public UIElementLabelAndText SetLabelAlignment(UIElementAlignment labelAlignment)
    {
        m_labelAlignment = labelAlignment;
        return this;
    }

    public UIElementLabelAndText SetText(string text)
    {
        m_text.text = text;
        return this;
    }

    public UIElementLabelAndText SetTextFunc(Func<string> textFunc)
    {
        m_textFunc = textFunc;
        return this;
    }

    public UIElementLabelAndText SetTextAlignment(UIElementAlignment textAlignment)
    {
        m_textAlignment = textAlignment;
        return this;
    }

    public UIElementLabelAndText ForceMultiline(bool multiline)
    {
        m_forceTextMultiline = multiline;
        return this;
    }

    private void Update()
    {
        if (m_labelFunc != null)
            m_label.text = m_labelFunc();

        if (m_textFunc != null)
            m_text.text = m_textFunc();

        float width = m_transform.rect.width;

        m_displayedTextMultiline = m_forceTextMultiline;
        float textsWidth = m_text.renderedWidth + m_label.renderedWidth + 5;
        if (textsWidth > width)
            m_displayedTextMultiline = true;

        if(m_displayedTextMultiline)
        {
            m_textTransform.localPosition = new Vector3(m_textTransform.localPosition.x, -m_label.renderedHeight, m_textTransform.localPosition.z);

            if (m_labelAlignment == UIElementAlignment.left)
                m_label.alignment = TextAlignmentOptions.TopLeft;
            else if (m_labelAlignment == UIElementAlignment.right)
                m_label.alignment = TextAlignmentOptions.TopRight;
            else if (m_labelAlignment == UIElementAlignment.center)
                m_label.alignment = TextAlignmentOptions.Top;

            if (m_textAlignment == UIElementAlignment.left)
                m_text.alignment = TextAlignmentOptions.TopLeft;
            else if (m_textAlignment == UIElementAlignment.right)
                m_text.alignment = TextAlignmentOptions.TopRight;
            else if (m_textAlignment == UIElementAlignment.center)
                m_text.alignment = TextAlignmentOptions.Top;
        }
        else
        {
            m_label.alignment = TextAlignmentOptions.TopLeft;
            m_text.alignment = TextAlignmentOptions.TopRight;
        }
    }
}
