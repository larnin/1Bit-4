using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

public class UIElementSpace : UIElementBase
{
    float m_space = 1;
    LayoutElement m_elemnt;

    private void Awake()
    {
        m_elemnt = GetComponent<LayoutElement>();
    }

    public UIElementSpace SetSpace(float space)
    {
        m_space = space;
        if (m_elemnt != null)
            m_elemnt.minHeight = space;
        return this;
    }
}
