using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public override void Update() { }

    public override void End()
    {
        m_cursor.SetCursorEnabled(false);
    }
}
