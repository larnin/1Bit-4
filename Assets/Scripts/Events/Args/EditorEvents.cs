using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ToggleEditorToolCategoryEvent
{
    public EditorToolCategoryType category;

    public ToggleEditorToolCategoryEvent(EditorToolCategoryType _category)
    {
        category = _category;
    }
}

class EnableEditorCustomToolEvent
{
    public UIElementContainer container;

    public bool enabled;

    public EnableEditorCustomToolEvent(bool _enabled)
    {
        enabled = _enabled;
    }
}

class EditorSystemButtonClickedEvent
{
    public EditorSystemButtonType button;

    public EditorSystemButtonClickedEvent(EditorSystemButtonType _button)
    {
        button = _button;
    }
}

