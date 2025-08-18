using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UIElementSimpleContainer : UIElementBase
{
    UIElementContainer m_container;

    private void Awake()
    {
        m_container = GetComponentInChildren<UIElementContainer>();
    }

    public UIElementContainer GetContainer()
    {
        return m_container;
    }

}
