using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIElementFoldable : UIElementBase
{
    [SerializeField] Sprite m_foldedSprite;
    [SerializeField] Sprite m_openedSprite;

    Image m_headerImage;
    TMP_Text m_headerText;
    Button m_headerButton;

    UIElementContainer m_container;

    Func<string> m_headerTextFunc;

    bool m_folded = false;

    private void Awake()
    {
        var headerObj = transform.Find("Header");
        if(headerObj != null)
        {
            m_headerImage = headerObj.GetComponentInChildren<Image>();
            m_headerText = headerObj.GetComponentInChildren<TMP_Text>();
            m_headerButton = headerObj.GetComponentInChildren<Button>();
            if(m_headerButton != null)
                m_headerButton.onClick.AddListener(OnHeaderClick);
        }

        var containerObj = transform.Find("Content");
        if (containerObj != null)
            m_container = containerObj.GetComponentInChildren<UIElementContainer>();
    }

    private void Start()
    {
        UpdateContainerDisplay();
    }

    public UIElementContainer GetContainer()
    {
        return m_container;
    }

    float GetHeaderHeight()
    {
        return m_headerText.renderedHeight;
    }

    void OnHeaderClick()
    {
        m_folded = !m_folded;
        UpdateContainerDisplay();
    }

    public UIElementFoldable SetHeaderText(string text)
    {
        m_headerText.text = text;
        return this;
    }

    public UIElementFoldable SetHeaderTextFunc(Func<String> textFunc)
    {
        m_headerTextFunc = textFunc;
        return this;
    }

    void UpdateContainerDisplay()
    {
        m_container.gameObject.SetActive(!m_folded);
        m_headerImage.sprite = m_folded ? m_foldedSprite : m_openedSprite;
    }

    private void Update()
    {

    }
}
