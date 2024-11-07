using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

public class UIElementSimpleText : UIElementBase
{
    TMP_Text m_text;
    Func<string> m_textFunc;

    private void Awake()
    {
        m_text = GetComponentInChildren<TMP_Text>();
    }

    public override float GetHeight()
    {
        return m_text.renderedHeight;
    }

    public UIElementSimpleText SetText(string text)
    {
        m_text.text = text;
        return this;
    }

    public UIElementSimpleText SetTextFunc(Func<String> textFunc)
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
