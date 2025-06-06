﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GetNormalsTextureEvent
{
    public Texture normals;
    public Texture depth;
}

public class GetCameraScaleEvent
{
    public float scale = 1;
}

public class SetDecalsEnabledEvent
{
    public bool enabled = false;

    public SetDecalsEnabledEvent(bool _enabled)
    {
        enabled = _enabled;
    }
}