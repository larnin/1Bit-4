using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StatsInfos
{
    public int kills;
    public int spawnersDestroyed;
    public int buildingsBuild;
    public int buildingsLost;
}

public class StatsSystem : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    StatsInfos m_stats = new StatsInfos();

    static StatsSystem m_instance = null;
    public static StatsSystem instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;

        m_subscriberList.Add(new Event<OnEnnemyKillEvent>.Subscriber(OnKill));
        m_subscriberList.Add(new Event<OnBuildingBuildEvent>.Subscriber(OnBuildingBuild));
        m_subscriberList.Add(new Event<OnBuildingDestroyEvent>.Subscriber(OnBuildingDestroyed));
        m_subscriberList.Add(new Event<OnBuildingRemovedEvent>.Subscriber(OnBuildingRemoved));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;

        m_subscriberList.Unsubscribe();
    }

    void OnKill(OnEnnemyKillEvent e)
    {
        m_stats.kills++;
    }

    void OnBuildingBuild(OnBuildingBuildEvent e)
    {
        m_stats.buildingsBuild++;
    }

    void OnBuildingRemoved(OnBuildingRemovedEvent e)
    {
        m_stats.buildingsBuild--;
    }

    void OnBuildingDestroyed(OnBuildingDestroyEvent e)
    {
        if (e.building.GetTeam() == Team.Player)
            m_stats.buildingsLost++;
        else if (e.building.GetTeam() == Team.Ennemy && e.building.GetBuildingType() == BuildingType.EnnemySpawner)
            m_stats.spawnersDestroyed++;
    }

    public StatsInfos GetStats()
    {
        return m_stats;
    }
}
