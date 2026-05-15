using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSubObjectiveSetAllBuildingsUnlocked : QuestSubObjectiveBase
{
    [SerializeField]
    bool m_unlocked = true;
    public bool unlocked { get { return m_unlocked; } set { m_unlocked = value; } }

    [SerializeField]
    bool m_globalyUnlocked = false;
    public bool globalyUnlocked { get { return m_globalyUnlocked; } set { m_globalyUnlocked = value; } }

    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        var allBuildings = Enum.GetValues(typeof(BuildingType));
        foreach (var type in allBuildings)
        {
            var b = (BuildingType)type;

            if (m_globalyUnlocked)
                GameInfos.instance.persistant.SetBuildingUnlocked(b, m_unlocked);

            if (GameSystem.instance != null)
                GameSystem.instance.SetBuildingUnlocked(b, m_unlocked);
        }
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
