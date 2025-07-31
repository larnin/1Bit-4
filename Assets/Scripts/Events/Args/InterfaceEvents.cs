using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GetCameraEvent
{
    public Camera camera;
    public Camera UICamera;
}

public class GetAllMainCameraEvent
{
    public List<Camera> cameras;
}

public class CameraMoveEvent
{
    public Camera camera;
    public Camera UICamera;

    public CameraMoveEvent(Camera _camera, Camera _UICamera)
    {
        camera = _camera;
        UICamera = _UICamera;
    }
}

public class GetCameraRotationEvent
{
    public float rotation = 0;
}

public class GetCameraDuplicationEvent
{
    public List<Vector2Int> duplications = new List<Vector2Int>();
}

public class SetHoveredObjectEvent
{
    public GameObject hoveredObject;

    public SetHoveredObjectEvent(GameObject obj)
    {
        hoveredObject = obj;
    }
}

public class IsScrollLockedEvent
{
    public bool scrollLocked = false;
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
