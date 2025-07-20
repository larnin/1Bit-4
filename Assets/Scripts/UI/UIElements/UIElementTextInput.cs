using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class UIElementTextInput : UIElementBase
{
    TMP_Text m_label;
    TMP_InputField m_inputField;
    RectTransform m_rect;
    Action<string> m_textChangeFunc;
    Func<string> m_textFunc; 

    private void Awake()
    {
        var labelTr = transform.Find("Label");
        if (labelTr != null)
            m_label = labelTr.GetComponent<TMP_Text>();

        var valueTr = transform.Find("Value");
        if (valueTr != null)
        {
            m_inputField = valueTr.GetComponentInChildren<TMP_InputField>();
            m_inputField.onValueChanged.AddListener(OnTextChange);
        }
    }

    void OnTextChange(string text)
    {
        if (m_textChangeFunc != null)
            m_textChangeFunc(text);
    }

    public UIElementTextInput SetText(string text)
    {
        bool textChange = text != m_inputField.text;
        m_inputField.text = text;
        if(textChange)
            OnTextChange(text);
        return this;
    }

    public UIElementTextInput SetTextFunc(Func<String> textFunc)
    {
        m_textFunc = textFunc;
        return this;
    }

    public string GetText()
    {
        return m_inputField.text;
    }
    public UIElementTextInput SetTextChangeFunc(Action<string> textChangeFunc)
    {
        m_textChangeFunc = textChangeFunc;
        return this;
    }

    public UIElementTextInput SetLabel(string label)
    {
        m_label.text = label;

        return this;
    }

    private void Update()
    {
        if (m_textFunc != null)
        {
            string newText = m_textFunc();
            if (newText != m_inputField.text)
            {
                m_inputField.text = m_textFunc();
                OnTextChange(newText);
            }
        }

        var inputTransform = m_inputField.GetComponent<RectTransform>();
        var labelSize = m_label.renderedWidth;
        if (labelSize < 0)
            labelSize = 0;

        var anchor = inputTransform.anchoredPosition;
        var size = inputTransform.sizeDelta;

        float right = -anchor.x - size.x / 2;
        float left = labelSize + 2;

        anchor.x = (left - right) / 2;
        size.x = -left - right;

        inputTransform.anchoredPosition = anchor;
        inputTransform.sizeDelta = size;
    }
}
