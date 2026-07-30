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
    [SerializeField] GameObject m_HudPrefab;
    public GameObject GetHudPrefab() { return m_HudPrefab; }

    public abstract GamemodeBase MakeGamemode(GamemodeSystem owner);
}

public abstract class GamemodeBase
{
    protected GamemodeSystem m_owner;

    public GamemodeBase(GamemodeSystem owner)
    {
        m_owner = owner;
    }

    public GamemodeSystem GetOwner() { return m_owner; }

    public virtual void Begin() { }
    public virtual void Process() { }
    public virtual void End() { }

    public virtual void SetOwnedHud(GameObject Hud) { }

    public abstract GamemodeStatus GetStatus();
    public abstract GamemodeAssetBase GetAsset();
}
