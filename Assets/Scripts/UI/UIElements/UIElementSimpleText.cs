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
    UIElementAlignment m_alignment = UIElementAlignment.left;

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

    public UIElementSimpleText SetAlignment(UIElementAlignment alignment)
    {
        m_alignment = alignment;
        return this;
    }

    private void Update()
    {
        if (m_textFunc != null)
            m_text.text = m_textFunc();

        if (m_alignment == UIElementAlignment.left)
            m_text.alignment = TextAlignmentOptions.TopLeft;
        else if (m_alignment == UIElementAlignment.right)
            m_text.alignment = TextAlignmentOptions.TopRight;
        else if (m_alignment == UIElementAlignment.center)
            m_text.alignment = TextAlignmentOptions.Top;
    }
}
