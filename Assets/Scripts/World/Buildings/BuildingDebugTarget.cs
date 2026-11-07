using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BuildingDebugTarget : BuildingBase
{
    public override BuildingType GetBuildingType()
    {
        return BuildingType.DebugTarget;
    }
}
