using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum UIElementSpriteAlignment
{
    left,
    center,
    right
}

public class UIElementSprite : UIElementBase
{
    Image m_image;
    RectTransform m_imageTransform;
    Vector2 m_size;
    bool m_nativeSize = true;
    UIElementSpriteAlignment m_alignment = UIElementSpriteAlignment.center;

    Func<Sprite> m_spriteFunc;

    private void Awake()
    {
        m_image = GetComponentInChildren<Image>();
        if (m_image != null)
            m_imageTransform = m_image.GetComponent<RectTransform>();
    }

    public override float GetHeight()
    {
        return m_size.y;
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
        if(m_alignment == UIElementSpriteAlignment.center)
            rect.position = new Vector2((parentWidth - m_size.x) / 2, rect.position.y);
        else if(m_alignment == UIElementSpriteAlignment.right)
            rect.position = new Vector2(parentWidth - m_size.x, rect.position.y);
        
        //todo apply rect to image transform
    }

    UIElementSprite SetSprite(Sprite sprite)
    {
        m_image.sprite = sprite;
        return this;
    }

    UIElementSprite SetSpriteFunc(Func<Sprite> spriteFunc)
    {
        m_spriteFunc = spriteFunc;
        return this;
    }

    UIElementSprite SetSize(Vector2 size)
    {
        m_nativeSize = false;
        m_size = size;
        return this;
    }

    UIElementSprite SetNativeSize()
    {
        m_nativeSize = true;
        return this;
    }

    UIElementSprite SetAlignment(UIElementSpriteAlignment alignment)
    {
        m_alignment = alignment;
        return this;
    }
}
