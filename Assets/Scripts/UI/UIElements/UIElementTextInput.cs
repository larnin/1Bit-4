using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

public class UIElementTextInput : UIElementBase
{
    TMP_Text m_label;
    TMP_InputField m_inputField;
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

    public override float GetHeight()
    {
        return m_inputField.textComponent.renderedHeight;
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
    }

    public UIElementTextInput SetLabel(string label)
    {
        m_label.text = label;
        return this;
    }
}
