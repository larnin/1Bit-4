using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum UIElementAlignment
{
    left,
    center,
    right
}

public abstract class UIElementBase : MonoBehaviour
{
    public abstract float GetHeight();
}
