using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIElementFloatInput : UIElementBase
{
    TMP_Text m_label;
    TMP_InputField m_inputField;
    Action<float> m_valueChangeFunc;
    float m_lastValidValue = 0;
    Button m_moreButton;
    Button m_lessButton;
    RectTransform m_rect;

    float m_minValue = float.MinValue;
    float m_maxValue = float.MaxValue;

    private void Awake()
    {
        m_rect = GetComponent<RectTransform>();

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

        m_inputField.text = m_lastValidValue.ToString();
    }

    char OnInputValidation(string input, int charIndex, char addedChar)
    {
        if (addedChar == '-')
        {
            if (charIndex != 0)
                return '\0';
        }
        else if(addedChar == '.' || addedChar == ',')
        {
            if (input.IndexOf('.') > 0 || input.IndexOf(',') > 0)
                return '\0';
        }
        else if (addedChar < '0' || addedChar > '9')
            return '\0';

        return addedChar;
    }

    void OnTextChange(string text)
    {
        float newValue = 0;

        if (!float.TryParse(text, out newValue))
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

    void ValueOffset(float offset)
    {
        float newValue = Mathf.Clamp(m_lastValidValue + offset, m_minValue, m_maxValue);
        if (newValue != m_lastValidValue)
            m_lastValidValue = newValue;

        m_inputField.text = m_lastValidValue.ToString();
        OnTextChange(m_inputField.text);
    }

    public UIElementFloatInput SetValue(int value)
    {
        m_lastValidValue = 0;
        ValueOffset(value);

        return this;
    }

    public float GetValue()
    {
        return m_lastValidValue;
    }
    public UIElementFloatInput SetValueChangeFunc(Action<float> valueChangeFunc)
    {
        m_valueChangeFunc = valueChangeFunc;
        return this;
    }

    public UIElementFloatInput SetBounds(float minValue, float maxValue)
    {
        m_minValue = minValue;
        m_maxValue = maxValue;

        float newValue = Mathf.Clamp(m_lastValidValue, m_minValue, m_maxValue);
        if (m_lastValidValue != newValue)
        {
            m_lastValidValue = newValue;
            m_inputField.text = m_lastValidValue.ToString();
            OnTextChange(m_inputField.text);
        }

        return this;
    }

    public UIElementFloatInput SetLabel(string label)
    {
        m_label.text = label;

        return this;
    }

    private void Update()
    {
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
