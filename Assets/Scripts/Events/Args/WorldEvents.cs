using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGridEvent
{
    public Grid grid;
}

public class SetGridEvent
{
    public Grid grid;

    public SetGridEvent(Grid _grid)
    {
        grid = _grid;
    }
}

public class GenerationFinishedEvent { }