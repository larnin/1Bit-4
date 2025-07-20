using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIElementContainer : MonoBehaviour
{
    [SerializeField] float m_contentMoreSize = 0;

    List<UIElementBase> m_elements = new List<UIElementBase>();
    RectTransform m_container;
    RectTransform m_rectTransform;
    VerticalLayoutGroup m_group;

    private void Awake()
    {
        m_group = GetComponentInChildren<VerticalLayoutGroup>();
        m_container = m_group.GetComponent<RectTransform>();
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
        m_container.sizeDelta = new Vector2(m_container.sizeDelta.x, m_group.preferredHeight);

        var canvas = m_rectTransform.GetComponentInParent<Canvas>();
        var canvasTransform = canvas.GetComponent<RectTransform>();
        float maxHeight = canvasTransform.rect.height - m_rectTransform.anchoredPosition.y;

        float height = m_group.preferredHeight + m_contentMoreSize;
        if (height > maxHeight)
            height = maxHeight;
        m_rectTransform.sizeDelta = new Vector2(m_rectTransform.sizeDelta.x, height);
    }

    public float GetHeight()
    {
        return m_group.preferredHeight;
    }
}

