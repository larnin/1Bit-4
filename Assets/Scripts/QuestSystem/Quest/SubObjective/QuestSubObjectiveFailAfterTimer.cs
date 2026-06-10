using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveFailAfterTimer : QuestSubObjectiveBase
{
    [SerializeField]
    float m_duration = 1;
    public float duration { get { return m_duration; } set { m_duration = value; } }

    float m_timer = 0;
    public float timer { get { return m_timer; } }

    public override bool IsCompleted()
    {
        return m_timer < m_duration;
    }

    public override bool CanFail()
    {
        return true;
    }

    public override bool IsFail()
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


    public override int GetDetailCount()
    {
        return 3;
    }

    public override string GetDetailName(int index)
    {
        if (index == 0)
            return "timer";
        if (index == 1)
            return "duration";
        if (index == 2)
            return "remaining";
        return base.GetDetailName(index);
    }

    public override string GetDetail(int index)
    {
        if (index == 0)
            return m_timer.ToString();
        if (index == 1)
            return m_duration.ToString();
        if (index == 2)
            return (m_duration - m_timer).ToString();
        return base.GetDetail(index);
    }
}
