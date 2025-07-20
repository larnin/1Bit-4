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
    RectTransform m_imageTransform;
    Vector2 m_size;
    bool m_nativeSize = true;
    UIElementAlignment m_alignment = UIElementAlignment.center;

    Func<Sprite> m_spriteFunc;

    private void Awake()
    {
        m_image = GetComponentInChildren<Image>();
        if (m_image != null)
            m_imageTransform = m_image.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (m_spriteFunc != null)
            m_image.sprite = m_spriteFunc();

        if(m_nativeSize && m_image.sprite != null)
            m_size = m_image.sprite.rect.size;

        float parentWidth = m_size.x;
        if(transform.parent != null)
            parentWidth = (transform.parent as RectTransform).rect.width;

        var rect = new Rect(new Vector2(0, 0), m_size);
        if(m_alignment == UIElementAlignment.center)
            rect.position = new Vector2((parentWidth - m_size.x) / 2, rect.position.y);
        else if(m_alignment == UIElementAlignment.right)
            rect.position = new Vector2(parentWidth - m_size.x, rect.position.y);

        m_imageTransform.anchorMin = new Vector2(rect.x / parentWidth, m_imageTransform.anchorMin.y);
        m_imageTransform.anchorMax = new Vector2((rect.x + rect.width) / parentWidth, m_imageTransform.anchorMax.y); 
    }

    public UIElementSprite SetSprite(Sprite sprite)
    {
        m_image.sprite = sprite;
        return this;
    }

    public UIElementSprite SetSpriteFunc(Func<Sprite> spriteFunc)
    {
        m_spriteFunc = spriteFunc;
        return this;
    }

    public UIElementSprite SetSize(Vector2 size)
    {
        m_nativeSize = false;
        m_size = size;
        return this;
    }

    public UIElementSprite SetNativeSize()
    {
        m_nativeSize = true;
        return this;
    }

    public UIElementSprite SetAlignment(UIElementAlignment alignment)
    {
        m_alignment = alignment;
        return this;
    }
}
