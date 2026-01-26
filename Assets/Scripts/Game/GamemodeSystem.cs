using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GamemodeSystem : MonoBehaviour
{

    static GamemodeSystem m_instance = null;
    public static GamemodeSystem instance { get { return m_instance; } }

    Dictionary<string, GamemodeBase> m_gamemodes = new Dictionary<string, GamemodeBase>();

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        foreach(var mode in m_gamemodes)
        {
            if (mode.Value == null)
                continue;

            mode.Value.End();
        }

        if (m_instance == this)
            m_instance = null;
    }

    public void StartGamemode(string name, GamemodeAssetBase gamemodeAsset, bool stopIfAlreadyPlaying = false)
    {
        GamemodeBase currentGamemode = null;
        if(m_gamemodes.TryGetValue(name, out currentGamemode))
        {
            if (!stopIfAlreadyPlaying)
                return;
            if(currentGamemode != null)
                currentGamemode.End();
            m_gamemodes.Remove(name);
        }

        if (gamemodeAsset == null)
            return;

        GamemodeBase gamemode = gamemodeAsset.MakeGamemode(this);
        if (gamemode == null)
            return;

        gamemode.Begin();

        m_gamemodes.Add(name, gamemode);
    }

    public void StopGamemode(string name)
    {
        GamemodeBase gamemode = null;
        if(m_gamemodes.TryGetValue(name, out gamemode))
        {
            if (gamemode != null)
                gamemode.End();
            m_gamemodes.Remove(name);
        }
    }

    public bool IsGamemodeRunning(string name)
    {
        return m_gamemodes.ContainsKey(name);
    }

    public List<string> GetGamemodesName()
    {
        List<string> names = new List<string>();
        foreach (var mode in m_gamemodes)
            names.Add(mode.Key);

        return names;
    }

    public GamemodeBase GetGamemode(string name)
    {
        GamemodeBase gamemode = null;
        if (m_gamemodes.TryGetValue(name, out gamemode))
            return gamemode;

        return null;
    }

    public GamemodeBase GetGamemode(GamemodeAssetBase asset)
    {
        foreach (var mode in m_gamemodes)
        {
            if (mode.Value == null)
                continue;
            if (mode.Value.GetAsset() == asset)
                return mode.Value;
        }

        return null;
    }

    public int GetGamemodeNb()
    {
        return m_gamemodes.Count();
    }

    public GamemodeBase GetGamemodeFromIndex(int index)
    {
        if (index < 0 || index > m_gamemodes.Count)
            return null;

        int nb = 0;
        foreach(var g in m_gamemodes)
        {
            if (index == nb)
                return g.Value;
            nb++;
        }

        return null;
    }

    private void Update()
    {
        foreach(var mode in m_gamemodes)
        {
            if (mode.Value == null)
                continue;
            mode.Value.Process();
        }
    }
}
