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

public class GetGridGenerationStatusEvent
{
    public int generatedChunks = 0;
    public int totalChunks = 1;
}

public class GenerationFinishedEvent { }