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

    public virtual void Enable()
    {
        m_isEnabled = true;
    }

    public virtual void Disable()
    {
        m_isEnabled = false;
    }

    public abstract void Update();

    public abstract void MoveTo(Vector3 pos);

    protected GameCamera m_gameCamera;
    protected bool m_isEnabled;
}
