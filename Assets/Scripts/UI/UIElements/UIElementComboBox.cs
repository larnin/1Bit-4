using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

public class UIElementComboBox : UIElementBase
{
    TMP_Text m_label;
    TMP_Dropdown m_dropdown;
    Action<int> m_valueChangeFunc;

    List<string> m_elements = new List<string>();
    int m_currentIndex = 0;

    private void Awake()
    {
        var labelTr = transform.Find("Label");
        if (labelTr != null)
            m_label = labelTr.GetComponent<TMP_Text>();

        var valueTr = transform.Find("Value");
        if (valueTr != null)
        {
            m_dropdown = valueTr.GetComponentInChildren<TMP_Dropdown>();
            m_dropdown.onValueChanged.AddListener(OnValueChange);
        }
    }

    void OnValueChange(int value)
    {
        if (m_valueChangeFunc != null)
            m_valueChangeFunc(value);

        m_currentIndex = value;
    }

    public int GetCurrentElementIndex()
    {
        return m_currentIndex;
    }

    public string GetCurrentElement()
    {
        return GetElementAt(m_currentIndex);
    }

    public string GetElementAt(int index)
    {
        if (index < 0 || index >= m_elements.Count)
            return "None";

        return m_elements[index];
    }

    public UIElementComboBox SetElementsFromEnum(Type enumType)
    {
        return SetElements(Enum.GetNames(enumType).ToList());
    }

    public UIElementComboBox SetElements(List<string> elements)
    {
        m_elements = elements.ToList();

        if (m_currentIndex < 0 || m_currentIndex >= m_elements.Count)
            m_currentIndex = 0;

        if(m_dropdown != null)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (var p in m_elements)
            {
                options.Add(new TMP_Dropdown.OptionData(p));
            }

            m_dropdown.options = options;
            m_dropdown.value = m_currentIndex;
        }

        return this;
    }

    public UIElementComboBox SetCurrentElementIndex(int index)
    {
        if (index < 0 || index >= m_elements.Count)
            return this;

        m_currentIndex = index;
        m_dropdown.value = m_currentIndex;

        return this;
    }

    public UIElementComboBox SetCurrentElement(string element)
    {
        for (int i = 0; i < m_elements.Count; i++)
        {
            if (m_elements[i] == element)
            {
                m_currentIndex = i;
                break;
            }
        }

        return this;
    }

    public UIElementComboBox SetValueChangeFunc(Action<int> valueChangeFunc)
    {
        m_valueChangeFunc = valueChangeFunc;
        return this;
    }

    public UIElementComboBox SetLabel(string label)
    {
        m_label.text = label;
        return this;
    }
}
