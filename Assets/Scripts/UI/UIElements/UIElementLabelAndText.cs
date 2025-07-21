using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIElementLabelAndText : UIElementBase
{
    TMP_Text m_label;
    TMP_Text m_text;
    RectTransform m_textTransform;
    LayoutElement m_labelLayout;


    Func<string> m_labelFunc;
    Func<string> m_textFunc;

    private void Awake()
    {
        var labelTr = transform.Find("Label");
        if(labelTr != null)
        {
            m_label = labelTr.GetComponent<TMP_Text>();
            m_labelLayout = labelTr.GetComponent<LayoutElement>();
        }

        var textTr = transform.Find("Text");
        if(textTr != null)
        {
            m_text = textTr.GetComponent<TMP_Text>();
        }
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


    private void Update()
    {
        if (m_labelFunc != null)
            m_label.text = m_labelFunc();

        if (m_textFunc != null)
            m_text.text = m_textFunc();

        if (m_labelLayout != null)
        {

            m_labelLayout.minWidth = m_label.preferredWidth;
        }
    }
}
