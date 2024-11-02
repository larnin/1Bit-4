using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GetCameraEvent
{
    public Camera camera;
}

public class SetHoveredObjectEvent
{
    public GameObject hoveredObject;

    public SetHoveredObjectEvent(GameObject obj)
    {
        hoveredObject = obj;
    }
}

public class DisplaySelectionBoxEvent
{
    public Vector3 pos1;
    public Vector3 pos2;

    public DisplaySelectionBoxEvent(Vector3 _pos1, Vector3 _pos2)
    {
        pos1 = _pos1;
        pos2 = _pos2;
    }
}

public class HideSelectionBoxEvent { }

public class IsMouseOverUIEvent
{
    public bool overUI = false;
}