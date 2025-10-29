using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class UndoElementBase
{
    public abstract void Apply();
    public abstract UndoElementBase GetRevertElement();
}
