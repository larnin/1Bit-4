using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IsCrystalUsedEvent
{
    public Vector3Int pos;
    public bool used;

    public IsCrystalUsedEvent(Vector3Int _pos)
    {
        pos = _pos;
        used = false;
    }
}

public class IsTitaniumUsedEvent
{
    public Vector3Int pos;
    public bool used;

    public IsTitaniumUsedEvent(Vector3Int _pos)
    {
        pos = _pos;
        used = false;
    }
}

public class GetTeamEvent
{
    public Team team = Team.Neutral;
}

