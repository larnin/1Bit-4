using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSystemDetailWindow
{
    VisualElement m_parent;
    List<QuestSystemNode> m_nodes = new List<QuestSystemNode>();

    public void SetParent(VisualElement parent)
    {
        m_parent = parent;
        Draw();
    }

    void Draw()
    {
        if (m_parent == null)
            return;

        m_parent.Clear();

        if(m_nodes.Count == 0)
        {
            var label = new Label("Select a node to display its detail");
            m_parent.Add(label);
        }
        else if(m_nodes.Count == 1)
        {
            const string startString = "QuestSystemNode";

            string name = m_nodes[0].GetType().Name;
            if (name.StartsWith(startString))
                name = name.Substring(startString.Length);

            var label = new Label("Node " + name + " - " + m_nodes[0].NodeName);
            m_parent.Add(label);

            var element = m_nodes[0].GetDetailElement();
            if (element != null)
                m_parent.Add(element);
        }
        else
        {
            var label = new Label("Detail can't be displayed on multiple nodes");
            m_parent.Add(label);
        }
    }

    public void SetNodes(List<QuestSystemNode> nodes)
    {
        if (AreEquivalent(nodes, m_nodes))
            return;
            
        m_nodes = nodes;
        Draw();
    }

    bool AreEquivalent(List<QuestSystemNode> a, List<QuestSystemNode> b)
    {
        if (a.Count != b.Count)
            return false;

        foreach(var e in a)
        {
            if (!b.Contains(e))
                return false;
        }

        return true;
    }
}
