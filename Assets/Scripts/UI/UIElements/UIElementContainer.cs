using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UIElementContainer : MonoBehaviour
{
    List<UIElementBase> m_elements = new List<UIElementBase>();
    RectTransform m_container;
    RectTransform m_rectTransform;

    private void Awake()
    {
        var containerTransform = transform.Find("Container");
        if (containerTransform != null)
            m_container = containerTransform.GetComponent<RectTransform>();
        if (m_container == null)
            m_container = GetComponent<RectTransform>();
        m_rectTransform = GetComponent<RectTransform>();
    }

    public void AddElement(UIElementBase element)
    {
        if (m_elements.Contains(element))
            return;

        m_elements.Add(element);

        element.transform.SetParent(m_container, false);
    }

    public void RemoveElement(int index)
    {
        if (index < 0 || index >= m_elements.Count)
            return;

        m_elements.RemoveAt(index);
    }

    public void RemoveAndDestroyElement(int index)
    {
        if (index < 0 || index >= m_elements.Count)
            return;

        var element = m_elements[index];
        m_elements.RemoveAt(index);
        Destroy(element.gameObject);
    }

    public void RemoveAll()
    {
        m_elements.Clear();
    }

    public void RemoveAndDestroyAll()
    {
        foreach (var e in m_elements)
            Destroy(e.gameObject);
        m_elements.Clear();
    }

    public int GetElementNb()
    {
        return m_elements.Count;
    }

    public UIElementBase GetElementByIndex(int index)
    {
        if (index < 0 || index >= m_elements.Count)
            return null;
        return m_elements[index];
    }

    private void Update()
    {
        float top = 0;

        foreach(var e in m_elements)
        {
            if (e == null)
                continue;

            float height = e.GetHeight();

            var tr = e.GetComponent<RectTransform>();
            if (tr == null)
                continue;

            tr.anchoredPosition = new Vector2(0, -top);
            tr.anchorMin = new Vector2(0, tr.anchorMin.y);
            tr.anchorMax = new Vector2(1, tr.anchorMax.y);
            tr.sizeDelta = new Vector2(0, height);

            top += height;
            top += Global.instance.UIElementDatas.spacing;
        }

        top -= Global.instance.UIElementDatas.spacing;
        m_rectTransform.sizeDelta = new Vector2(m_rectTransform.sizeDelta.x, top - m_container.sizeDelta.y);
    }
}

