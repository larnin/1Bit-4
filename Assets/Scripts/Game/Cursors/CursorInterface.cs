using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface CursorInterface
{
    public abstract void SetCursorEnabled(bool enabled);
    public abstract bool IsCursorEnabled();
}
