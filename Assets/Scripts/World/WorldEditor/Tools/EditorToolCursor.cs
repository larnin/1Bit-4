using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorToolCursor : EditorToolBase
{
    CursorInterface m_cursor;

    public void SetCursor(CursorInterface cursor)
    {
        m_cursor = cursor;
    }

    public override void Begin()
    {
        m_cursor.SetCursorEnabled(true);
    }

    public override void Update()
    { 
        if(!m_cursor.IsCursorEnabled() && EditorToolHolder.instance != null && EditorToolHolder.instance.GetCurrentTool() == this)
        {
            EditorToolHolder.instance.SetCurrentTool(null);
        }

    }

    public override void End()
    {
        m_cursor.SetCursorEnabled(false);
    }
}
