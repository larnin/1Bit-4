using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GamemodeStatus
{
    Ongoing,
    Completed,
    Failed,
}

public abstract class GamemodeAssetBase : ScriptableObject
{
    public abstract GamemodeBase GetGamemode(GameSystem owner);
}

public abstract class GamemodeBase
{
    protected GameSystem m_owner;

    public GamemodeBase(GameSystem owner)
    {
        m_owner = owner;
    }

    public GameSystem GetOwner() { return m_owner; }

    public virtual void Begin() { }
    public virtual void Process() { }
    public virtual void End() { }

    public virtual GamemodeStatus GetStatus() { return GamemodeStatus.Ongoing; }
}
