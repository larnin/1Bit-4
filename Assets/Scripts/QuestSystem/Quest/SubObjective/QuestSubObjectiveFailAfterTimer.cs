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
}
