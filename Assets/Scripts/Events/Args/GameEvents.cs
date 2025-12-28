using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class OnEnnemyKillEvent 
{
    public EnnemyBehaviour ennemy;

    public OnEnnemyKillEvent(EnnemyBehaviour _ennemy)
    {
        ennemy = _ennemy;
    }
}

public class OnEnnemyDamagedEvent
{
    public EnnemyBehaviour ennemy;

    public OnEnnemyDamagedEvent(EnnemyBehaviour _ennemy)
    {
        ennemy = _ennemy;
    }
}

public class OnSpawnerDestroyEvent 
{
    public BuildingBase building;

    public OnSpawnerDestroyEvent(BuildingBase _building)
    {
        building = _building;
    }
}

public class OnSpawnerDamagedEvent
{
    public BuildingBase building;
    public float lifeLossPercent;

    public OnSpawnerDamagedEvent(BuildingBase _building, float percent)
    {
        building = _building;
        lifeLossPercent = percent;
    }
}

public class DisplaySpawnerInfosEvent
{
    public BuildingBase building;
    public UIElementContainer container;

    public DisplaySpawnerInfosEvent(BuildingBase _building, UIElementContainer _container)
    {
        building = _building;
        container = _container;
    }
}

public class OnBuildingBuildEvent
{
    public BuildingType type;
    public Vector3Int pos;

    public OnBuildingBuildEvent(BuildingType _type, Vector3Int _pos)
    {
        type = _type;
        pos = _pos;
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

public class DisplayEndLevelEvent
{
    public bool levelSuccess;
    public bool displayed = false;

    public DisplayEndLevelEvent(bool _levelSucces)
    {
        levelSuccess = _levelSucces;
    }
}

public class TowerDeathEvent { }

public class QuestEndLevelEvent
{
    public bool succes = true;

    public QuestEndLevelEvent(bool _succes)
    {
        succes = _succes;
    }
}