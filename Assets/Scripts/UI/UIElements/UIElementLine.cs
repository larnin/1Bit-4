using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

public class UIElementLine : UIElementBase
{
    Image m_image;

    private void Awake()
    {
        m_image = GetComponentInChildren<Image>();
    }

    public override float GetHeight()
    {
        if (m_image == null)
            return 0;

        if (m_image.sprite != null)
            return m_image.sprite.rect.height;

        return m_image.preferredHeight;
    }
}
