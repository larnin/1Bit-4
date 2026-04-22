using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSubObjectiveConstructBuilding : QuestSubObjectiveBase
{
    [SerializeField] BuildingType m_buildingType;
    public BuildingType buildingType { get { return m_buildingType; } set { m_buildingType = value; } }

    [SerializeField] int m_count = 1;
    public int count { get { return m_count; } set { m_count = value; } }

    SubscriberList m_subscriberList;

    int m_nb = 0;

    public override bool IsCompleted()
    {
        return m_nb >= m_count;
    }

    public override void Start()
    {
        if (m_subscriberList == null)
        {
            m_subscriberList = new SubscriberList();
            m_subscriberList.Add(new Event<BuildingListAddEvent>.Subscriber(OnNewBuilding));
        }
        m_subscriberList.Subscribe();

        m_nb = 0;
    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void End()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnNewBuilding(BuildingListAddEvent e)
    {
        if (e.building.GetTeam() != Team.Player)
            return;

        if (e.building.GetBuildingType() == m_buildingType)
            m_nb++;
    }
}