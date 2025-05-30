using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSystemErrorWindow
{
    class ErrorData
    {
        public string line;
        public string source;
    }

    VisualElement m_parent;
    List<ErrorData> m_lines = new List<ErrorData>();

    public void SetParent(VisualElement parent)
    {
        m_parent = parent;
        Draw();
    }

    public void AddError(string value, string source)
    {
        ErrorData data = new ErrorData();
        data.line = value;
        data.source = source;

        m_lines.Add(data);
        Draw();
    }

    public void ClearErrors(string source = null)
    {
        if (source == null)
            m_lines.Clear();
        else
        {
            for (int i = 0; i < m_lines.Count; i++)
            {
                if (m_lines[i].source == source)
                {
                    m_lines.RemoveAt(i);
                    i--;
                }
            }
        }
        Draw();
    }

    void Draw()
    {
        if (m_parent == null)
            return;

        m_parent.Clear();

        var label = new Label("Errors");
        m_parent.Add(label);

        var scrollView = new ScrollView();
        m_parent.Add(scrollView);

        foreach (var l in m_lines)
        {
            var line = new Label(l.line);
            scrollView.Add(line);
        }
    }
}
