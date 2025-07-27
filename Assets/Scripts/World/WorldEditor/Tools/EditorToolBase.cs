using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class EditorToolBase
{
    public void SetHolder(EditorToolHolder holder)
    {
        m_holder = holder;
    }

    public abstract void Begin();
    public abstract void Update();
    public abstract void End();

    protected EditorToolHolder m_holder;
}
