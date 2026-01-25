using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShowLoadingScreenEvent
{
    public bool start;

    public ShowLoadingScreenEvent(bool _start)
    {
        start = _start;
    }
}

public class HideLoadingScreenEvent { }

public class GetCanvasEvent
{
    public Canvas canvas;
}