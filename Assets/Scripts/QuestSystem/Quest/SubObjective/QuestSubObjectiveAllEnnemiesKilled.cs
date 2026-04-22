using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSubObjectiveAllEnnemiesKilled : QuestSubObjectiveBase
{
    SubscriberList m_subscriberList;

    int m_entityCount = 1;

    public override bool IsCompleted()
    {
        return m_entityCount == 0;
    }

    public override void Start()
    {
        if(m_subscriberList == null)
        {
            m_subscriberList = new SubscriberList();
            m_subscriberList.Add(new Event<OnEnnemyKillEvent>.Subscriber(OnKill));
        }

        m_subscriberList.Subscribe();
    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void End()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnKill(OnEnnemyKillEvent e)
    {
        m_entityCount = 0;

        if (EntityList.instance == null)
            return;

        int nbEntity = EntityList.instance.GetEntityNb();
        for(int i = 0; i < nbEntity; i++)
        {
            var entity = EntityList.instance.GetEntityFromIndex(i);
            if (entity.GetTeam() != Team.Ennemy)
                continue;

            if (Event<IsDeadEvent>.Broadcast(new IsDeadEvent(), entity.gameObject).isDead)
                continue;

            m_entityCount++;
        }
    }
}
