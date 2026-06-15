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

    public override int GetDetailCount()
    {
        return 4;
    }

    public override string GetDetailName(int index)
    {
        if (index == 0)
            return "Type";
        if (index == 1)
            return "constructed";
        if (index == 2)
            return "total";
        if (index == 3)
            return "remaining";
        return base.GetDetailName(index);
    }

    public override string GetDetail(int index)
    {
        if (index == 0)
            return m_buildingType.ToString();
        if (index == 1)
            return m_nb.ToString();
        if (index == 2)
            return m_count.ToString();
        if (index == 3)
            return (m_count - m_nb).ToString();
        return base.GetDetail(index);
    }
}