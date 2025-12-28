using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveSetBuildingUnlocked : QuestSubObjectiveBase
{
    [SerializeField]
    BuildingType m_buildingType;
    public BuildingType buildingType { get { return m_buildingType; } set { m_buildingType = value; } }

    [SerializeField]
    bool m_unlocked = true;
    public bool unlocked { get { return m_unlocked; } set { m_unlocked = value; } }


    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        if (GameSystem.instance != null)
            GameSystem.instance.SetBuildingUnlocked(m_buildingType, m_unlocked);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
