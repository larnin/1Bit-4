using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface EntityMoveTargetInterface
{
    public abstract Vector3 GetNextPos();
    public abstract bool CanMove();
}
