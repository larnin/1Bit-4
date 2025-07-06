using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIElementButton : UIElementBase
{
    RectTransform m_transform;
    Button m_button;
    TMP_Text m_text;
    Action m_clickFunc;
    Func<string> m_textFunc;

    private void Awake()
    {
        m_transform = GetComponent<RectTransform>();
        m_button = GetComponentInChildren<Button>();
        m_text = GetComponentInChildren<TMP_Text>();

        m_button.onClick.AddListener(OnButtonClick);
    }

    public override float GetHeight()
    {
        return m_transform.rect.height;
    }

    void OnButtonClick()
    {
        if (m_clickFunc != null)
            m_clickFunc();
    }

    public UIElementButton SetClickFunc(Action clickFunc)
    {
        m_clickFunc = clickFunc;
        return this;
    }

    public UIElementButton SetText(string text)
    {
        m_text.text = text;
        return this;
    }

    public UIElementButton SetTextFunc(Func<String> textFunc)
    {
        m_textFunc = textFunc;
        return this;
    }

    private void Update()
    {
        if (m_textFunc != null)
            m_text.text = m_textFunc();
    }
}
