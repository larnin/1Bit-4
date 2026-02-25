using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveHaveResource : QuestSubObjectiveBase
{
    public enum ValueOperator
    {
        MoreOrEqual,
        LessOrEqual,
    }

    [SerializeField] ResourceType m_resourceType;
    public ResourceType resourceType { get { return m_resourceType; } set { m_resourceType = value; } }

    [SerializeField] ValueOperator m_operator;
    public ValueOperator valueOperator { get { return m_operator; } set { m_operator = value; } }

    [SerializeField] float m_quantity;
    public float quantity { get { return m_quantity; } set { m_quantity = value; } }

    public override bool IsCompleted()
    {
        if (ResourceSystem.instance == null)
            return false;

        if (!ResourceSystem.instance.HaveResource(m_resourceType))
            return false;

        float currentQuantity = ResourceSystem.instance.GetResourceStored(m_resourceType);

        if (m_operator == ValueOperator.MoreOrEqual && currentQuantity >= m_quantity)
            return true;

        if (m_operator == ValueOperator.LessOrEqual && currentQuantity <= m_quantity)
            return true;

        return false;
    }

    public override void Start() { }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
