using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OnKillEvent { }

public class OnSpawnerDestroyEvent { }

public class OnBuildingBuildEvent
{
    public BuildingType type;

    public OnBuildingBuildEvent(BuildingType _type)
    {
        type = _type;
    }
}

public class OnBuildingRemovedEvent
{
    public BuildingType type;

    public OnBuildingRemovedEvent(BuildingType _type)
    {
        type = _type;
    }
}

public class OnBuildingDestroyedEvent
{
    public BuildingType type;

    public OnBuildingDestroyedEvent(BuildingType _type)
    {
        type = _type;
    }
}

public class BuildingListAddEvent
{
    public BuildingBase building;

    public BuildingListAddEvent(BuildingBase _building)
    {
        building = _building;
    }
}

public class BuildingListRemoveEvent
{
    public BuildingBase building;

    public BuildingListRemoveEvent(BuildingBase _building)
    {
        building = _building;
    }
}

public class ConnexionsUpdatedEvent
{

}

public class GetEntityIDEvent
{
    public Guid id = Guid.Empty;
}