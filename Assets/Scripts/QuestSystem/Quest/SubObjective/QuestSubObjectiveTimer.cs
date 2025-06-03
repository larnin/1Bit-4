using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSubObjectiveTimer : QuestSubObjectiveBase
{
    float m_duration = 1;
    float m_timer = 0;

    public override bool IsCompleted()
    {
        return m_timer >= m_duration;
    }

    public override void Start()
    {
        m_timer = 0;
    }

    public override void Update(float deltaTime)
    {
        m_timer += deltaTime;
    }

    public override void End() { }
}

