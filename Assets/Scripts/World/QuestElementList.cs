using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestElementList : MonoBehaviour
{
    List<QuestElement> m_elements = new List<QuestElement>();

    static QuestElementList m_instance = null;
    public static QuestElementList instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void Register(QuestElement element)
    {
        m_elements.Add(element);
    }

    public void UnRegister(QuestElement element)
    {
        m_elements.Remove(element);
    }

    public int GetElementNb()
    {
        return m_elements.Count;
    }

    public QuestElement GetElementFromIndex(int index)
    {
        if (index < 0 || index >= m_elements.Count)
            return null;

        return m_elements[index];
    }

    public List<QuestElement> GetElementsByName(string name)
    {
        List<QuestElement> elements = new List<QuestElement>();

        foreach(var e in m_elements)
        {
            if (e.name == name)
                elements.Add(e);
        }

        return elements;
    }

    public QuestElement GetFirstElementByName(string name)
    {
        foreach (var e in m_elements)
        {
            if (e.name == name)
                return e;
        }

        return null;
    }
}
