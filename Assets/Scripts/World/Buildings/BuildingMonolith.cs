using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingMonolith : BuildingBase
{
    enum State
    {
        Idle,
    }

    [SerializeField] GameObject m_orbsPivot;

    State m_state = State.Idle;
    float m_timer = 0;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Monolith;
    }

    protected override void SaveImpl(JsonObject obj)
    {
        //todo
    }

    protected override void LoadImpl(JsonObject obj)
    {
        //todo
    }
}
