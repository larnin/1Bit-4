using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ToggleToolCategoryEvent
{
    public ToolCategoryType category;

    public ToggleToolCategoryEvent(ToolCategoryType _category)
    {
        category = _category;
    }
}
