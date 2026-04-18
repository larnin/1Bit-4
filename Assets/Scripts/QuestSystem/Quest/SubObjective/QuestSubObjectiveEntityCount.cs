using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSubObjectiveEntityCount : QuestSubObjectiveBase
{
    public enum EntityOperator
    {
        Equal,
        Inequal,
        Less,
        LessOrEqual,
        More,
        MoreOrEqual,
    }

    [SerializeField] bool m_useType = true;
    public bool useType { get { return m_useType; } set { m_useType = value; } }

    [SerializeField] string m_entityType = "";
    public string entityType { get { return m_entityType; } set { m_entityType = value; } }

    [SerializeField] bool m_useTeam = true;
    public bool useTeam { get { return m_useTeam; } set { m_useTeam = value; } }

    [SerializeField] Team m_team = Team.Player;
    public Team team { get { return m_team; } set { m_team = value; } }

    [SerializeField] int m_count = 0;
    public int count { get { return m_count; } set { m_count = value; } }

    [SerializeField] EntityOperator m_operator = EntityOperator.Equal;
    public EntityOperator entityOperator { get { return m_operator; } set { m_operator = value; } }

    public override bool IsCompleted()
    {
        int nb = GetEntityNb();
        switch(m_operator)
        {
            case EntityOperator.Equal:
                return nb == m_count;
            case EntityOperator.Inequal:
                return nb != m_count;
            case EntityOperator.Less:
                return nb < m_count;
            case EntityOperator.LessOrEqual:
                return nb <= m_count;
            case EntityOperator.More:
                return nb > m_count;
            case EntityOperator.MoreOrEqual:
                return nb >= m_count;
        }

        return false;
    }

    public override void Start()
    {

    }

    public override void Update(float deltaTime)
    {
        
    }

    public override void End()
    {
        
    }

    int GetEntityNb()
    {
        if (EntityList.instance == null)
            return 0;

        int count = 0;

        int nbEntity = EntityList.instance.GetEntityNb();
        for(int i = 0; i < nbEntity; i++)
        {
            var e = EntityList.instance.GetEntityFromIndex(i);

            if(m_useType && e.GetEntityType() != m_entityType)
                continue;

            if (m_useTeam && e.GetTeam() != m_team)
                continue;

            if (Event<IsDeadEvent>.Broadcast(new IsDeadEvent(), e.gameObject).isDead)
                continue;

            count++;
        }

        return count;
    }
}
