using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

public class UIElementToggle : UIElementBase
{
    TMP_Text m_label;
    Toggle m_inputField;
    Action<bool> m_toggleChangeFunc;

    private void Awake()
    {
        var labelTr = transform.Find("Label");
        if (labelTr != null)
            m_label = labelTr.GetComponent<TMP_Text>();

        var valueTr = transform.Find("Value");
        if (valueTr != null)
        {
            m_inputField = valueTr.GetComponentInChildren<Toggle>();
            m_inputField.onValueChanged.AddListener(OnValueChange);
        }
    }

    void OnValueChange(bool value)
    {
        if (m_toggleChangeFunc != null)
            m_toggleChangeFunc(value);
    }

    public UIElementToggle SetValue(bool toggled)
    {
        if(m_inputField.isOn != toggled)
        {
            m_inputField.isOn = toggled;
            OnValueChange(toggled);

        }
        return this;
    }

    public bool IsToggled()
    {
        return m_inputField.isOn;
    }

    public UIElementToggle SetLabel(string label)
    {
        m_label.text = label;
        return this;
    }

    public UIElementToggle SetValueChangeFunc(Action<bool> toggleChangeFunc)
    {
        m_toggleChangeFunc = toggleChangeFunc;
        return this;
    }
}
