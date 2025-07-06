using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIElementIntInput : UIElementBase
{
    TMP_Text m_label;
    TMP_InputField m_inputField;
    Action<int> m_valueChangeFunc;
    int m_lastValidValue = 0;
    Button m_moreButton;
    Button m_lessButton;

    int m_minValue = int.MinValue;
    int m_maxValue = int.MaxValue;

    private void Awake()
    {
        var labelTr = transform.Find("Label");
        if (labelTr != null)
            m_label = labelTr.GetComponent<TMP_Text>();

        var valueTr = transform.Find("Value");
        if (valueTr != null)
        {
            m_inputField = valueTr.GetComponentInChildren<TMP_InputField>();
            m_inputField.onValidateInput += OnInputValidation;
            m_inputField.onEndEdit.AddListener(OnTextChange);
        }

        var buttonTransform = transform.Find("More");
        if (buttonTransform != null)
            m_moreButton = buttonTransform.GetComponentInChildren<Button>();
        buttonTransform = transform.Find("Less");
        if (buttonTransform != null)
            m_lessButton = buttonTransform.GetComponentInChildren<Button>();

        if (m_moreButton != null)
            m_moreButton.onClick.AddListener(OnMoreClick);
        if (m_lessButton != null)
            m_lessButton.onClick.AddListener(OnLessClick);
    }

    char OnInputValidation(string input, int charIndex, char addedChar)
    {
        if (addedChar == '-')
        {
            if (charIndex != 0)
                return '\0';
        }
        else if (addedChar < '0' || addedChar > '9')
            return '\0';

        return addedChar;
    }

    void OnTextChange(string text)
    {
        int newValue = 0;

        if (!int.TryParse(text, out newValue))
        {
            m_inputField.text = m_lastValidValue.ToString();
            return;
        }

        if (m_lastValidValue == newValue)
            return;

        m_lastValidValue = newValue;

        if (m_valueChangeFunc != null)
            m_valueChangeFunc(newValue);
    }

    void OnMoreClick()
    {
        ValueOffset(1);
    }

    void OnLessClick()
    {
        ValueOffset(-1);
    }

    void ValueOffset(int offset)
    {
        int newValue = Mathf.Clamp(m_lastValidValue + offset, m_minValue, m_maxValue);
        if (newValue != m_lastValidValue)
            m_lastValidValue = newValue;

        m_inputField.text = m_lastValidValue.ToString();
        OnTextChange(m_inputField.text);
    }

    public override float GetHeight()
    {
        return m_inputField.textComponent.renderedHeight;
    }

    public UIElementIntInput SetValue(int value)
    {
        ValueOffset(value);

        return this;
    }

    public int GetValue()
    {
        return m_lastValidValue;
    }
    public UIElementIntInput SetValueChangeFunc(Action<int> valueChangeFunc)
    {
        m_valueChangeFunc = valueChangeFunc;
        return this;
    }

    public UIElementIntInput SetBounds(int minValue, int maxValue)
    {
        m_minValue = minValue;
        m_maxValue = maxValue;

        int newValue = Mathf.Clamp(m_lastValidValue, m_minValue, m_maxValue);
        if(m_lastValidValue != newValue)
        {
            m_lastValidValue = newValue;
            m_inputField.text = m_lastValidValue.ToString();
            OnTextChange(m_inputField.text);
        }

        return this;
    }

    public UIElementIntInput SetLabel(string label)
    {
        m_label.text = label;
        return this;
    }
}
