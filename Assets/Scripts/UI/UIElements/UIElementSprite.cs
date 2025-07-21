using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIElementSprite : UIElementBase
{
    Image m_image;
    Transform m_left;
    Transform m_right;
    LayoutElement m_element;
    float m_scale = 1;

    Func<Sprite> m_spriteFunc;

    private void Awake()
    {
        m_image = GetComponentInChildren<Image>();
        if(m_image != null)
            m_element = m_image.GetComponent<LayoutElement>();
        m_left = transform.Find("Left");
        m_right = transform.Find("Right");
    }

    private void Update()
    {
        if (m_spriteFunc != null)
        {
            m_image.sprite = m_spriteFunc();
            UpdateSize();
        }
    }

    public UIElementSprite SetSprite(Sprite sprite)
    {
        m_image.sprite = sprite;
        UpdateSize();
        return this;
    }

    public UIElementSprite SetSpriteFunc(Func<Sprite> spriteFunc)
    {
        m_spriteFunc = spriteFunc;
        return this;
    }

    public UIElementSprite SetPreserveAspect(bool preserve)
    {
        m_image.preserveAspect = preserve;
        return this;
    }

    public void SetScale(float scale)
    {
        m_scale = scale;
        UpdateSize();
    }

    public UIElementSprite SetAlignment(UIElementAlignment alignment)
    {
        m_left.gameObject.SetActive(alignment != UIElementAlignment.left);
        m_right.gameObject.SetActive(alignment != UIElementAlignment.right);
        return this;
    }

    void UpdateSize()
    {
        if (m_image.sprite == null || m_element == null)
            return;

        var size = m_image.sprite.rect.size;

        m_element.minWidth = size.x * m_scale;
        m_element.minHeight = size.y * m_scale;
    }
}
