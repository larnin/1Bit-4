using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BuildSelectionDetailCommonEvent
{
    public UIElementContainer container;

    public BuildSelectionDetailCommonEvent(UIElementContainer _container)
    {
        container = _container;
    }
}

public class BuildSelectionDetailLifeEvent
{
    public UIElementContainer container;

    public BuildSelectionDetailLifeEvent(UIElementContainer _container)
    {
        container = _container;
    }
}

public class BuildSelectionDetailStatusEvent
{
    public UIElementContainer container;

    public BuildSelectionDetailStatusEvent(UIElementContainer _container)
    {
        container = _container;
    }
}