using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class FreeCameraParams
{
    public float fov;
}

public class ControlCameraFree : ControlCameraBase
{
    public ControlCameraFree(FreeCameraParams camParams)
    {
        m_params = camParams;
    }

    public override void Enable()
    {
        
    }

    public override void Disable()
    {
        
    }

    public override void Update()
    {
        
    }

    public override void MoveTo(Vector3 pos)
    {
        
    }

    FreeCameraParams m_params;
}
