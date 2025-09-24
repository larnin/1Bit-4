using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ToggleEditorToolCategoryEvent
{
    public EditorToolCategoryType category;

    public ToggleEditorToolCategoryEvent(EditorToolCategoryType _category)
    {
        category = _category;
    }
}

public class EnableEditorCustomToolEvent
{
    public UIElementContainer container;

    public bool enabled;

    public EnableEditorCustomToolEvent(bool _enabled)
    {
        enabled = _enabled;
    }
}

public class EditorSystemButtonClickedEvent
{
    public EditorSystemButtonType button;

    public EditorSystemButtonClickedEvent(EditorSystemButtonType _button)
    {
        button = _button;
    }
}

