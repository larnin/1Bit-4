using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ControlCameraBase
{
    public virtual void SetParent(GameCamera camera)
    {
        m_gameCamera = camera;
    }

    public abstract void Enable();
    public abstract void Disable();
    public abstract void Update();

    public abstract void MoveTo(Vector3 pos);

    protected GameCamera m_gameCamera;
}
