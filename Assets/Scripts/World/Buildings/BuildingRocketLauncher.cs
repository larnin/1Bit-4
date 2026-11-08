using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BuildingRocketLauncher : BuildingTurretFreezer
{
    public override BuildingType GetBuildingType()
    {
        return BuildingType.Turret4;
    }
}
