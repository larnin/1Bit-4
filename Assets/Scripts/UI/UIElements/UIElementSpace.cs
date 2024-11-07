using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UIElementSpace : UIElementBase
{
    float m_space = 1;

    public override float GetHeight()
    {
        return m_space;
    }
    public UIElementSpace SetSpace(float space)
    {
        m_space = space;
        return this;
    }
}
